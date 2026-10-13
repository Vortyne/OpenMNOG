using Mnog.Engine.Assets;
using Mnog.Engine.Display;
using SkiaSharp;

namespace Mnog.Engine.Render;

/// <summary>
/// Draws artwork that isn't changing from images instead of its vector shapes. A big clip or shape whose content
/// (shapes, child positions, colours, frames, text) has stayed the same for a couple of game frames is drawn once
/// into an image (on the GPU when drawing there) covering the visible part plus a margin; while only its position
/// changes (panning) the image is drawn instead, at the nearest whole pixel. Anything changing is drawn as usual,
/// in its place between the cached pieces, so animated parts of a static layer stay live.
/// </summary>
/// <remarks>
/// Drawing a scene's large vector layers again every frame while panning is what limits the frame rate; most of
/// their artwork never changes. Objects being faded (alpha), masks, and anything in smooth
/// mode's in-between motion are always drawn live. MNOG_NOCACHE=1 turns this off (for comparison).
/// </remarks>
public sealed class LayerCache
{
    public static readonly bool Disabled = Environment.GetEnvironmentVariable("MNOG_NOCACHE") is { Length: > 0 };

    /// <summary>Smaller objects (device pixels, area) are drawn as usual: caching them gains little.</summary>
    const float MinArea = 48 * 48;
    /// <summary>Game frames an object's content must stay the same before it's cached.</summary>
    const int StableTicks = 2;

    sealed class Entry
    {
        public ulong Hash;
        public long StableSince;
        public SKImage? Image;
        public SKMatrix Basis;
        public SKRectI Region;
        public long LastUsed;
        public readonly Queue<long> Builds = new();
        /// <summary>The game frames it last changed in.</summary>
        public readonly Queue<long> Changes = new();
        public long PausedUntil;
    }

    // Keyed by the object and the part of a widescreen-split object it's drawn in (0 outside one): each part draws
    // it at its own stretch, so has its own image.
    readonly Dictionary<(DisplayObject Object, int Part), Entry> _entries = [];
    readonly Dictionary<DisplayObject, (ulong Hash, bool Volatile)> _hashes = new(ReferenceEqualityComparer.Instance);
    long _frame, _tick;
    // Time spent making images this drawn frame (Stopwatch ticks): past a budget, the rest wait for the next frames
    // (drawn as they are meanwhile), so a scene's arrival isn't one long frame of image building.
    long _buildTime;
    static readonly long BuildBudget = System.Diagnostics.Stopwatch.Frequency * 4 / 1000;

    /// <summary>Drops every cached image (a setting changed how artwork is drawn).</summary>
    public void Clear()
    {
        foreach (var e in _entries.Values) e.Image?.Dispose();
        _entries.Clear();
    }

    /// <summary>Called before each frame is drawn.</summary>
    public void BeginFrame(long tick)
    {
        _frame++;
        _buildTime = 0;
        _tick = tick;
        _hashes.Clear();
        if (_frame % 60 != 0) return;
        foreach (var (o, e) in _entries.Where(kv => kv.Key.Object.Removed || _frame - kv.Value.LastUsed > 240).ToList())
        {
            e.Image?.Dispose();
            _entries.Remove(o);
        }
    }

    /// <summary>Draws an object from its cached image when it can; false to have it drawn as usual.</summary>
    public readonly Dictionary<string, int> Stats = [];
    void Count(string k) => Stats[k] = Stats.GetValueOrDefault(k) + 1;

    /// <param name="render">How to draw it into the image (by default as usual): a widescreen-split object's parts.</param>
    /// <param name="part">Inside a widescreen-split object, which of its parts this is drawn in (see the entries).</param>
    public bool TryDraw(RenderContext ctx, DisplayObject c, SKMatrix world, CxForm cx, Action<RenderContext>? render = null, int part = 0)
    {
        bool r = TryDrawInner(ctx, c, world, cx, render, part, out string why);
        if (StatsOn) Count(why);
        if (why is "BUILD" or "changed" or "thrash" && FrameEvents.Count < 200) FrameEvents.Add($"{why} {c.SlashPath}");
        if (WhyOn && why is "changed" or "BUILD" or "thrash") Console.WriteLine($"CACHEWHY {why} {c.SlashPath} {(c as GraphicObject)?.Asset?.Id}");
        return r;
    }

    /// <summary>What the cache rebuilt or couldn't use this drawn frame (for the host's slow-frame log).</summary>
    public static readonly List<string> FrameEvents = [];
    static readonly bool WhyOn = Environment.GetEnvironmentVariable("MNOG_CACHEWHY") != null;
    static readonly bool StatsOn = Environment.GetEnvironmentVariable("MNOG_CACHESTATS") != null;

    bool TryDrawInner(RenderContext ctx, DisplayObject c, SKMatrix world, CxForm cx, Action<RenderContext>? render, int part, out string why)
    {
        why = "type";
        if (c is not (MovieClip or GraphicObject) || c.ClipDepth > 0) return false;
        why = "alpha"; if (cx.AM != 1 || cx.AA != 0) return false;
        var dm = ctx.Base.PreConcat(world);
        why = "degenerate"; if (dm.ScaleX * dm.ScaleY - dm.SkewX * dm.SkewY == 0) return false;
        var (hash, isVolatile) = Hash(c);
        why = isVolatile ? "volatile" : "clip"; if (isVolatile || ctx.HoldsClip(c)) return false;
        if (!_entries.TryGetValue((c, part), out var e)) _entries[(c, part)] = e = new Entry { Hash = hash, StableSince = _tick };
        e.LastUsed = _frame;
        if (e.Hash != hash)
        {
            e.Hash = hash;
            e.StableSince = _tick;
            e.Changes.Enqueue(_tick);
            e.Image?.Dispose();
            e.Image = null;
            why = "changed";
            return false;
        }
        // Something that keeps changing (an animation holding each drawing for a few frames, like IV.4-N's lava) is
        // drawn as it is: an image of it would be redrawn every few frames and hardly used.
        while (e.Changes.Count > 0 && _tick - e.Changes.Peek() > 60) e.Changes.Dequeue();
        why = "animated"; if (e.Changes.Count >= 3) return false;
        why = "unstable";
        if (_tick - e.StableSince < StableTicks || _tick < e.PausedUntil) return false;
        var bounds = dm.MapRect(c.LocalBounds);
        why = "small"; if (bounds.Width * bounds.Height < MinArea) return false;
        var view = ctx.Canvas.DeviceClipBounds;
        // (Its bounds don't count a widescreen stretch (its own or a child's), which reaches further: they're widened by
        // as much as the stretch could add, sideways and downwards. Relative to the object, not the view: the image
        // must keep covering it as it pans, not be redrawn every frame.)
        if (c.Player.Widescreen)
        {
            float sx = 1, sy = 1;
            void Take(DisplayObject o)
            {
                sx = Math.Max(sx, Math.Max(o.WidescreenScaleX, o.WidescreenScaleLeft ?? 1));
                sy = Math.Max(sy, o.WidescreenScaleY);
            }
            Take(c);
            if (c is MovieClip { } holding) foreach (var k in holding.Children.Values) Take(k);
            if (sx != 1 || sy != 1)
            {
                float extraX = bounds.Width * (sx - 1), extraY = bounds.Height * (sy - 1);
                bounds = new SKRect(bounds.Left - extraX, bounds.Top, bounds.Right + extraX, bounds.Bottom + extraY);
            }
        }
        var need = SKRect.Intersect(bounds, view);
        why = "offview"; if (need.IsEmpty) return true; // nothing of it is in view
        // The image is reused while the object is only moved: its scale, rotation and skew are the same.
        if (e.Image != null && Math.Abs(dm.ScaleX - e.Basis.ScaleX) < 1e-4 && Math.Abs(dm.ScaleY - e.Basis.ScaleY) < 1e-4 &&
            Math.Abs(dm.SkewX - e.Basis.SkewX) < 1e-4 && Math.Abs(dm.SkewY - e.Basis.SkewY) < 1e-4)
        {
            int dx = (int)Math.Round(dm.TransX - e.Basis.TransX), dy = (int)Math.Round(dm.TransY - e.Basis.TransY);
            var placed = new SKRectI(e.Region.Left + dx, e.Region.Top + dy, e.Region.Right + dx, e.Region.Bottom + dy);
            if (placed.Contains(SKRectI.Ceiling(need)) || ContainsLoose(placed, need))
            {
                Draw(ctx, e.Image, placed.Left, placed.Top, cx);
                why = "HIT";
                return true;
            }
        }
        // (Re)draw the image: the visible part plus a margin to pan into. Something redrawn this often isn't
        // worth caching for a while (a zoom, an animated size like IV.4-N's lava, or panning faster than the margin
        // covers). Counted in game frames, not drawn ones: the same at any display refresh rate.
        while (e.Builds.Count > 0 && _tick - e.Builds.Peek() > 60) e.Builds.Dequeue();
        if (e.Builds.Count >= 3)
        {
            why = "thrash";
            e.PausedUntil = _tick + 120;
            e.Image?.Dispose();
            e.Image = null;
            return false;
        }
        why = "budget"; if (_buildTime > BuildBudget) return false;
        long buildStart = System.Diagnostics.Stopwatch.GetTimestamp();
        var area = view;
        area.Inflate(view.Width / 2, view.Height / 4);
        // (Sideways, all of it when that fits one image: a panorama is then never redrawn partway through a pan, which
        // stalled panning across IX.4-E for a tenth of a second or more.)
        // Up and down too when the whole of it fits: a quick vertical pan (IX.4B's, up its tall door) outran the margin,
        // redrawing it every few frames until it counted as thrashing and was drawn live for seconds.
        if (bounds.Width <= 8192 && bounds.Height <= 8192 && bounds.Width * bounds.Height <= 48_000_000)
            area = SKRectI.Ceiling(bounds, true);
        else if (bounds.Width <= 8192 && bounds.Width * area.Height <= 48_000_000)
            area = new SKRectI(Math.Min(area.Left, (int)Math.Floor(bounds.Left)), area.Top, Math.Max(area.Right, (int)Math.Ceiling(bounds.Right)), area.Bottom);
        var region = SKRectI.Ceiling(SKRect.Intersect(bounds, area), true);
        if (region.Width <= 0 || region.Height <= 0 || region.Width > 8192 || region.Height > 8192) return false;
        var info = new SKImageInfo(region.Width, region.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = ctx.Canvas.Context is GRRecordingContext gpu ? SKSurface.Create(gpu, true, info) : SKSurface.Create(info);
        if (surface == null) return false;
        surface.Canvas.Clear(SKColors.Transparent);
        var sub = new RenderContext(surface.Canvas, SKMatrix.CreateTranslation(-region.Left, -region.Top).PreConcat(ctx.Base), ctx.Player, ctx.T) { Cache = null };
        if (render != null) render(sub); else c.Render(sub, world, CxForm.Identity);
        surface.Canvas.ResetMatrix();
        e.Image?.Dispose();
        e.Image = surface.Snapshot();
        e.Basis = dm;
        e.Region = region;
        e.Builds.Enqueue(_tick);
        _buildTime += System.Diagnostics.Stopwatch.GetTimestamp() - buildStart;
        why = "BUILD";
        if (WhyOn) Console.WriteLine($"CACHEBUILD {c.SlashPath} part {part}: bounds {bounds} view {view} need {need} region {region} scale {dm.ScaleX:0.###},{dm.ScaleY:0.###}");
        Draw(ctx, e.Image, region.Left, region.Top, cx);
        return true;
    }

    /// <summary>Whether the image covers what's needed, give or take the half pixel of rounding.</summary>
    static bool ContainsLoose(SKRectI placed, SKRect need) =>
        placed.Left <= need.Left + 0.5f && placed.Top <= need.Top + 0.5f && placed.Right >= need.Right - 0.5f && placed.Bottom >= need.Bottom - 0.5f;

    readonly SKPaint _paint = new();

    void Draw(RenderContext ctx, SKImage image, int x, int y, CxForm cx)
    {
        ctx.Canvas.Save();
        ctx.Canvas.ResetMatrix();
        _paint.ColorFilter = cx.IsIdentity ? null : cx.ToFilter();
        ctx.Canvas.DrawImage(image, x, y, _paint);
        ctx.Canvas.Restore();
    }

    /// <summary>
    /// A hash of what an object draws, apart from its own position and colour (which apply on top of its image);
    /// volatile when something in it is being interpolated between game frames (smooth mode), so differs from
    /// one drawn frame to the next.
    /// </summary>
    (ulong Hash, bool Volatile) Hash(DisplayObject o)
    {
        if (_hashes.TryGetValue(o, out var known)) return known;
        var h = new Hasher();
        bool isVolatile = false;
        h.Add(o.GetType().GetHashCode());
        h.Add(o.Asset?.Id ?? -1);
        h.Add(o.Asset?.Movie.GetHashCode() ?? 0);
        void Children(IEnumerable<DisplayObject> children)
        {
            foreach (var c in children)
            {
                h.Add(c.Depth);
                h.Add(c.Visible ? 1 : 0);
                h.Add(c.ClipDepth);
                h.Add((c.WidescreenScaleX != 1 || c.WidescreenScaleY != 1) && c.Player.Widescreen ? 1 : 0);
                var m = c.Matrix;
                h.Add(m.ScaleX); h.Add(m.SkewX); h.Add(m.TransX); h.Add(m.SkewY); h.Add(m.ScaleY); h.Add(m.TransY);
                var x = c.ColorTransform;
                h.Add(x.RM); h.Add(x.GM); h.Add(x.BM); h.Add(x.AM); h.Add(x.RA); h.Add(x.GA); h.Add(x.BA); h.Add(x.AA);
                if (c.LerpMatrix || c.LerpColor) isVolatile = true;
                var (ch, cv) = Hash(c);
                h.Add((long)ch);
                isVolatile |= cv;
            }
        }
        // A frosted panel blurs what's under it on screen: whatever holds it isn't kept as a still image.
        if (o.Player.VectorImages && RenderContext.IsFrosted(o)) isVolatile = true;
        switch (o)
        {
            case MovieClip mc:
                h.Add(mc.CurrentFrame);
                Children(mc.Children.Values);
                break;
            case ButtonObject b:
                // (What its current state draws, not which state it's in: an invisible button (IX.4-E's paths, which
                // the scene pans under the cursor) going over and back draws the same, and its layer needn't be redrawn.)
                Children(b.StateChildren);
                break;
            case GraphicObject g:
                h.Add(g.Ratio);
                // A live backdrop (Player.Backdrop) animates by itself: never kept as a still image.
                if (g.Asset is ShapeAsset s && o.Player.VectorImages && o.Player.HasBackdrop(s)) isVolatile = true;
                break;
            case EditTextObject t:
                h.Add(t.Text.GetHashCode());
                // A text being typed out (smooth mode reveals it between frames), or with the caret blinking.
                if (o.Player.Smooth && t.PrevText != t.Text || o.Player.Focus == t) isVolatile = true;
                break;
            case BitmapObject bm:
                h.Add(bm.Image.UniqueId);
                break;
        }
        return _hashes[o] = (h.Value, isVolatile);
    }

    /// <summary>FNV-1a over the values added.</summary>
    struct Hasher
    {
        ulong _h;
        public Hasher() => _h = 14695981039346656037;
        public readonly ulong Value => _h;
        public void Add(long v)
        {
            for (int i = 0; i < 8; i++) { _h ^= (byte)(v >> (i * 8)); _h *= 1099511628211; }
        }
        public void Add(int v) => Add((long)v);
        public void Add(float v) => Add((long)BitConverter.SingleToInt32Bits(v));
        public void Add(uint v) => Add((long)v);
    }
}

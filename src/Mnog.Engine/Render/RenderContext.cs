using Mnog.Engine.Assets;
using Mnog.Engine.Display;
using Mnog.Format;
using SkiaSharp;

namespace Mnog.Engine.Render;

/// <summary>Draws the display list with Skia. World matrices are in stage pixels; Base maps them to the window.</summary>
public sealed class RenderContext(SKCanvas canvas, SKMatrix baseMatrix, Player player, float t = 1)
{
    /// <summary>Fraction of the way from the previous game frame to the current one (smooth mode).</summary>
    public readonly float T = t;
    public readonly SKCanvas Canvas = canvas;
    public readonly SKMatrix Base = baseMatrix;
    readonly SKPaint _paint = new() { IsAntialias = true };
    /// <summary>Debugging: time spent drawing each shape part (MNOG_DRAWPROF).</summary>
    public static readonly Dictionary<string, double>? DrawProfile = Environment.GetEnvironmentVariable("MNOG_DRAWPROF") != null ? [] : null;

    /// <summary>Widescreen: objects drawn clipped to a rectangle (window pixels).</summary>
    public readonly Dictionary<DisplayObject, SKRect> Clips = [];

    public Player Player => player;

    /// <summary>Draws unchanging artwork from images (see LayerCache); null to draw everything as shapes.</summary>
    public LayerCache? Cache { get; init; }

    int _splitPart; // drawing a widescreen-split object: which of its parts (1..3), 0 otherwise

    HashSet<DisplayObject>? _clipHolders;

    /// <summary>Whether an object is, or contains, one drawn with its own clip (which an image of it would lose).</summary>
    public bool HoldsClip(DisplayObject o)
    {
        if (Clips.Count == 0) return false;
        if (_clipHolders == null)
        {
            _clipHolders = new(ReferenceEqualityComparer.Instance);
            foreach (var k in Clips.Keys)
                for (DisplayObject? a = k; a != null && _clipHolders.Add(a); a = a.Parent) { }
        }
        return _clipHolders.Contains(o);
    }

    void SetMatrix(SKMatrix world) => Canvas.SetMatrix(Base.PreConcat(world));

    /// <summary>Draws children in depth order, applying masks (an object with ClipDepth masks the depths above it).</summary>
    public void DrawContainer(SortedList<int, DisplayObject> children, SKMatrix world, CxForm cx)
    {
        var list = children.Values;
        int maskEnd = -1;
        DisplayObject? mask = null;
        SKMatrix maskWorld = default;
        for (int i = 0; i < list.Count; i++)
        {
            var c = list[i];
            if (mask != null && c.Depth > maskEnd)
            {
                EndMask(mask, maskWorld);
                mask = null;
            }
            var childWorld = world.PreConcat(c.DrawMatrix(T));
            if (c.ClipDepth > 0)
            {
                if (mask != null) EndMask(mask, maskWorld);
                // The masked objects are drawn into a layer (limited to the mask's bounds), which the mask's
                // shapes then cut out when the group ends. (ClipMasks, for SVG output, which has no layer blending:
                // clipped to the mask's outline instead.)
                if (ClipMasks)
                {
                    using var outline = new SKPath();
                    MaskPath(c, childWorld, outline);
                    Canvas.Save();
                    Canvas.ResetMatrix();
                    Canvas.ClipPath(outline, SKClipOperation.Intersect, true);
                }
                else
                {
                    Canvas.ResetMatrix();
                    Canvas.SaveLayer(Base.PreConcat(childWorld).MapRect(c.LocalBounds), null);
                }
                mask = c;
                maskWorld = childWorld;
                maskEnd = c.ClipDepth;
                continue;
            }
            if (!c.Visible) continue;
            if (player.VectorImages && IsFrosted(c)) Frost(c, childWorld);
            if (Clips.Count > 0 && Clips.TryGetValue(c, out var clipRect))
            {
                Canvas.Save();
                Canvas.ResetMatrix();
                Canvas.ClipRect(clipRect);
                c.Render(this, childWorld, cx.Concat(c.DrawColor(T)));
                Canvas.Restore();
            }
            else if (IsSplit(c))
            {
                // Drawn in parts (see DrawSplit); kept in the layer cache as one image like anything else, so panning
                // slides it instead of redrawing it twice a frame.
                var childCx = cx.Concat(c.DrawColor(T));
                if (Cache == null || _splitPart > 0 || !Cache.TryDraw(this, c, childWorld, childCx, sub => sub.DrawSplit(c, world, childWorld, CxForm.Identity)))
                    DrawSplit(c, world, childWorld, childCx);
            }
            else
            {
                var childCx = cx.Concat(c.DrawColor(T));
                if (Cache == null || !Cache.TryDraw(this, c, childWorld, childCx, part: _splitPart)) c.Render(this, childWorld, childCx);
            }
        }
        if (mask != null) EndMask(mask, maskWorld);
    }

    /// <summary>A widescreen-split object: drawn in parts, each stretched its own way (DrawSplit).</summary>
    bool IsSplit(DisplayObject c) => player.Widescreen &&
        (c.WidescreenSplitY != null && c.WidescreenScaleY != 1 || c.WidescreenSplit != null && c.WidescreenScaleX != 1);

    /// <summary>Draws a widescreen-split object (IsSplit) in its parts, each clipped and stretched its own way.</summary>
    void DrawSplit(DisplayObject c, SKMatrix world, SKMatrix childWorld, CxForm childCx)
    {
        // Each part numbered, so what's in it is kept in the layer cache at that part's stretch (an image per part).
        void Part(int n, Action draw)
        {
            int was = _splitPart;
            _splitPart = n;
            draw();
            _splitPart = was;
        }
        if (c.WidescreenSplitY is float splitY && c.WidescreenScaleY != 1)
        {
            // Above the split as it is; below it stretched down from there (split along the device's y).
            float sy = Base.PreConcat(childWorld).MapPoint(0, splitY).Y;
            Canvas.Save();
            Canvas.ResetMatrix();
            Canvas.ClipRect(new SKRect(-1e6f, -1e6f, 1e6f, sy));
            Part(1, () => c.Render(this, childWorld, childCx));
            Canvas.Restore();
            Canvas.Save();
            Canvas.ResetMatrix();
            Canvas.ClipRect(new SKRect(-1e6f, sy, 1e6f, 1e6f));
            Part(2, () => c.Render(this, childWorld.PreConcat(SKMatrix.CreateScale(1, c.WidescreenScaleY, 0, splitY)), childCx));
            Canvas.Restore();
        }
        else
        {
            // Left of the split as it is, right of it stretched from there (split along the device's x). (Its own
            // left and right: on screen they swap if it's drawn mirrored.)
            var plain = world.PreConcat(c.UnstretchedDrawMatrix(T));
            var toDevice = Base.PreConcat(plain);
            bool mirrored = toDevice.MapVector(1, 0).X < 0;
            float splitX = toDevice.MapPoint(c.SplitX, 0).X;
            // (With a left split as well: left of it stretched from there leftwards, the middle as it is.)
            float leftX = mirrored ? 1e6f : -1e6f;
            if (c.WidescreenSplitLeft != null)
            {
                float lx = c.SplitLeftX;
                leftX = toDevice.MapPoint(lx, 0).X;
                Canvas.Save();
                Canvas.ResetMatrix();
                Canvas.ClipRect(mirrored ? new SKRect(leftX, -1e6f, 1e6f, 1e6f) : new SKRect(-1e6f, -1e6f, leftX, 1e6f));
                Part(1, () => c.Render(this, plain.PreConcat(SKMatrix.CreateScale(c.WidescreenScaleLeft ?? c.WidescreenScaleX, 1, lx, 0)), childCx));
                Canvas.Restore();
            }
            Canvas.Save();
            Canvas.ResetMatrix();
            Canvas.ClipRect(new SKRect(Math.Min(leftX, splitX), -1e6f, Math.Max(leftX, splitX), 1e6f));
            Part(2, () => c.Render(this, plain, childCx));
            Canvas.Restore();
            Canvas.Save();
            Canvas.ResetMatrix();
            Canvas.ClipRect(mirrored ? new SKRect(-1e6f, -1e6f, splitX, 1e6f) : new SKRect(splitX, -1e6f, 1e6f, 1e6f));
            Part(3, () => c.Render(this, childWorld, childCx));
            Canvas.Restore();
        }
    }

    readonly SKPaint _keepInside = new() { BlendMode = SKBlendMode.DstIn };
    readonly SKPaint _maskFill = new() { IsAntialias = true, Color = SKColors.Black };

    /// <summary>Keeps the masked layer only where the mask's shapes are, then composites it.</summary>
    /// <summary>A sky photo's colours as a vertical gradient (each of 12 bands of rows averaged), placed by its fill's matrix.</summary>
    static SKShader? SkyGradient(FillDoc fill, MovieAsset movie, CxForm cx)
    {
        if (fill.Bitmap is not int id || movie.Symbol(id) is not ImageAsset { Image: { } img }) return null;
        using var bmp = SKBitmap.FromImage(img);
        const int n = 12;
        var colors = new SKColor[n];
        var pos = new float[n];
        for (int k = 0; k < n; k++)
        {
            int y0 = k * bmp.Height / n, y1 = Math.Max(y0 + 1, (k + 1) * bmp.Height / n);
            long r = 0, g = 0, b = 0, c = 0;
            for (int y = y0; y < y1; y += 2)
                for (int x = 0; x < bmp.Width; x += 4) { var p = bmp.GetPixel(x, y); r += p.Red; g += p.Green; b += p.Blue; c++; }
            colors[k] = cx.Apply(new SKColor((byte)(r / c), (byte)(g / c), (byte)(b / c)));
            pos[k] = (k + 0.5f) / n;
        }
        return SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(0, img.Height), colors, pos, SKShaderTileMode.Clamp, Geo.Matrix(fill.Matrix));
    }

    /// <summary>
    /// SVG output: a bitmap fill drawn as a finished picture of the area it fills (on the device, at the canvas's
    /// scale), clipped to its outline, as SVG patterns can't mirror an image or recolour it.
    /// </summary>
    void RasterFill(SKPath path)
    {
        var device = Canvas.TotalMatrix.MapRect(path.Bounds);
        device.Intersect(SKRect.Create(-4000, -4000, 12000, 12000));
        var r = SKRectI.Ceiling(device);
        if (r.Width <= 0 || r.Height <= 0 || r.Width > 8192 || r.Height > 8192) { Canvas.DrawPath(path, _paint); return; }
        using var surface = SKSurface.Create(new SKImageInfo(r.Width, r.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
        surface.Canvas.SetMatrix(SKMatrix.CreateTranslation(-r.Left, -r.Top).PreConcat(Canvas.TotalMatrix));
        surface.Canvas.DrawPath(path, _paint);
        using var image = surface.Snapshot();
        var m = Canvas.TotalMatrix;
        Canvas.Save();
        Canvas.ClipPath(path, SKClipOperation.Intersect, true);
        Canvas.ResetMatrix();
        Canvas.DrawImage(image, r.Left, r.Top);
        Canvas.Restore();
        Canvas.SetMatrix(m);
    }

    /// <summary>An image with a colour filter applied to its pixels.</summary>
    static SKImage Tinted(SKImage image, SKColorFilter filter)
    {
        using var surface = SKSurface.Create(new SKImageInfo(image.Width, image.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
        surface.Canvas.Clear(SKColors.Transparent);
        using var paint = new SKPaint { ColorFilter = filter };
        surface.Canvas.DrawImage(image, 0, 0, paint);
        return surface.Snapshot();
    }

    /// <summary>Masks drawn as clip paths, not layers (SVG output: see DrawContainer).</summary>
    public bool ClipMasks;

    /// <summary>With ClipMasks: the sky photos drawn as a gradient of their colours (false: as the photos).</summary>
    public bool SkyGradients = true;
    SKShader? _svgShader;

    /// <summary>A mask's outline on the canvas (its shapes, as DrawMaskShapes fills them), added to <paramref name="into"/>.</summary>
    void MaskPath(DisplayObject o, SKMatrix world, SKPath into)
    {
        var m = Base.PreConcat(world);
        void Add(SKPath p)
        {
            using var t = new SKPath(p);
            t.Transform(m);
            t.FillType = SKPathFillType.Winding;
            into.AddPath(t);
        }
        switch (o)
        {
            case GraphicObject g when g.Asset is ShapeAsset sh:
                foreach (var p in sh.Parts) if (p.Fill != null) Add(p.Path);
                break;
            case GraphicObject g when g.Asset is MorphAsset mo:
                for (int k = 0; k < mo.Parts.Count; k++) if (mo.Parts[k].FillStart != null) Add(player.MorphPath(mo, k, g.Ratio));
                break;
            case MovieClip mc:
                foreach (var c in mc.Children.Values) MaskPath(c, world.PreConcat(c.DrawMatrix(T)), into);
                break;
            default:
                using (var r = new SKPath()) { r.AddRect(o.LocalBounds); Add(r); }
                break;
        }
        into.FillType = SKPathFillType.Winding;
    }

    void EndMask(DisplayObject mask, SKMatrix world)
    {
        if (ClipMasks) { Canvas.Restore(); return; }
        Canvas.SaveLayer(_keepInside);
        DrawMaskShapes(mask, world);
        Canvas.Restore();
        Canvas.Restore();
    }

    /// <summary>Fills a mask object's geometry opaquely (overlapping pieces add up, as in Flash).</summary>
    void DrawMaskShapes(DisplayObject o, SKMatrix world)
    {
        SetMatrix(world);
        switch (o)
        {
            case GraphicObject g when g.Asset is ShapeAsset s:
                foreach (var p in s.Parts)
                    if (p.Fill != null) Canvas.DrawPath(p.Path, _maskFill);
                break;
            case GraphicObject g when g.Asset is MorphAsset m:
                for (int k = 0; k < m.Parts.Count; k++)
                    if (m.Parts[k].FillStart != null) Canvas.DrawPath(player.MorphPath(m, k, g.Ratio), _maskFill);
                break;
            case MovieClip mc:
                foreach (var c in mc.Children.Values) DrawMaskShapes(c, world.PreConcat(c.DrawMatrix(T)));
                break;
            default:
                Canvas.DrawRect(o.LocalBounds, _maskFill);
                break;
        }
    }

    // ---- Graphics ----

    public void DrawGraphic(GraphicObject g, SKMatrix world, CxForm cx)
    {
        if (cx.AM <= 0 && cx.AA <= 0) return;
        switch (g.Asset)
        {
            case ShapeAsset s: DrawShape(s, world, cx); break;
            case MorphAsset m: DrawMorph(m, g.Ratio, world, cx); break;
            case TextAsset t: DrawText(t, world, cx); break;
        }
    }

    void DrawShape(ShapeAsset s, SKMatrix world, CxForm cx)
    {
        if (s.Detailed && DrawCached(s, world, cx)) return;
        SetMatrix(world);
        var drawn = player.VectorImages ? s.VectorBounds : s.Bounds;
        int index = -1;
        if (!Canvas.QuickReject(drawn) || drawn.IsEmpty)
            foreach (var part in s.Parts)
            {
                index++;
                if (player.VectorImages && DrawLiveBackdrop(part, s.Movie, world, cx))
                {
                    // Its vector replacement over it ("overlay": the chronicle orbs' glass).
                    if (part.Fill is { Type: "bitmap", Bitmap: int over } && player.Backdrop(s.Movie, over) is { Overlay: true })
                    {
                        SetMatrix(world);
                        DrawVectorImage(part, s.Movie, cx);
                    }
                    continue;
                }
                // ("hideImage": the photo not drawn beneath its replacement: its soft edge would show past the
                // replacement's crisp one. The replacement, if it has shapes, still is; until it's first been drawn (in
                // the background), the photo shows in its place, as with any other replacement.)
                if (player.VectorImages && part.Fill is { Type: "bitmap", Bitmap: int hid } && s.Movie.Symbol(hid) is ImageAsset { Vector: not null, VectorHidesImage: true } hidden &&
                    !(hidden.Vector.Count > 0 && hidden.VectorCache.Count == 0 && hidden.VectorDone.IsEmpty))
                {
                    if (hidden.Vector.Count > 0) DrawVectorImage(part, s.Movie, cx);
                    continue;
                }
                ConfigurePaint(part, s.Movie, cx, world);
                long t0 = DrawProfile != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0;
                if (ClipMasks && SkyGradients && part.Fill is { Type: "bitmap", Bitmap: int skyId } skyFill &&
                    (s.Movie.Name.StartsWith("sky", StringComparison.OrdinalIgnoreCase) || s.Movie.Symbol(skyId) is ImageAsset { Image: { Width: 1156, Height: 302 } }) &&
                    SkyGradient(skyFill, s.Movie, cx) is { } sky)
                {
                    // (SVG: the sky photos as a vertical gradient of their own colours, row by row.)
                    using var skyPaint = new SKPaint { IsAntialias = true, Shader = sky };
                    Canvas.DrawPath(part.Path, skyPaint);
                }
                else if (ClipMasks && part.Fill is { Type: "bitmap" } && part.Stroke == null) RasterFill(part.Path);
                else Canvas.DrawPath(part.Path, _paint);
                // (Only where it meets another of the shape's fills.)
                if (cx.IsOpaque && s.SharedEdges(index) is { } shared) Seal(shared);
                if (player.VectorImages) DrawVectorImage(part, s.Movie, cx);
                if (DrawProfile != null)
                {
                    var key = $"{s.Movie.Name}#{s.Id} {(_paint.Style == SKPaintStyle.Stroke ? $"stroke w={_paint.StrokeWidth}" : "fill")} aa={_paint.IsAntialias} shader={_paint.Shader != null} cf={_paint.ColorFilter != null} alpha={_paint.Color.Alpha}";
                    DrawProfile[key] = DrawProfile.GetValueOrDefault(key) + System.Diagnostics.Stopwatch.GetElapsedTime(t0).TotalMilliseconds;
                }
            }
        Canvas.ResetMatrix();
    }

    /// <summary>Draws a detailed shape from its cached image (redrawn when its on-screen scale or sub-pixel
    /// position changes); false when it's rotated or skewed, which is drawn as usual.</summary>
    bool DrawCached(ShapeAsset s, SKMatrix world, CxForm cx)
    {
        var dm = Base.PreConcat(world);
        if (dm.SkewX != 0 || dm.SkewY != 0 || dm.ScaleX <= 0 || dm.ScaleY <= 0) return false;
        var device = dm.MapRect(s.Bounds);
        float x = MathF.Floor(device.Left) - 1, y = MathF.Floor(device.Top) - 1;
        // The shape in the image's pixels: the same scale, offset by its whole-pixel position.
        var local = dm.PostConcat(SKMatrix.CreateTranslation(-x, -y));
        bool same = s.Cached != null && Math.Abs(local.ScaleX - s.CachedFor.ScaleX) < 1e-4 && Math.Abs(local.ScaleY - s.CachedFor.ScaleY) < 1e-4 &&
                    Math.Abs(local.TransX - s.CachedFor.TransX) < 0.01 && Math.Abs(local.TransY - s.CachedFor.TransY) < 0.01;
        if (!same)
        {
            int w = (int)MathF.Ceiling(device.Right - x) + 2, h = (int)MathF.Ceiling(device.Bottom - y) + 2;
            if (w <= 0 || h <= 0 || w > 8192 || h > 8192) return false;
            if (LayerCache.FrameEvents.Count < 200) LayerCache.FrameEvents.Add($"shape {s.Movie.Name}#{s.Id} {w}x{h}");
            using var surface = SKSurface.Create(new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Premul));
            surface.Canvas.SetMatrix(local);
            using var paint = new SKPaint { IsAntialias = true };
            for (int i = 0; i < s.Parts.Count; i++)
            {
                var part = s.Parts[i];
                if (part.Fill == null || part.Fill.Type != "solid") return false;
                paint.Color = Geo.Color(part.Fill.Color);
                paint.Style = SKPaintStyle.Fill;
                surface.Canvas.DrawPath(part.Path, paint);
                if (paint.Color.Alpha == 255 && s.SharedEdges(i) is { } shared)
                {
                    paint.Style = SKPaintStyle.Stroke;
                    paint.StrokeWidth = 0;
                    surface.Canvas.DrawPath(shared, paint);
                }
            }
            s.Cached?.Dispose();
            s.Cached = surface.Snapshot();
            s.CachedFor = local;
        }
        Canvas.ResetMatrix();
        using var p = new SKPaint();
        if (!cx.IsIdentity) p.ColorFilter = cx.ToFilter();
        Canvas.DrawImage(s.Cached, x, y, p);
        return true;
    }

    /// <summary>
    /// A bitmap fill whose image has a vector replacement (ImageAsset.Vector): after the image, its shapes, placed
    /// by the fill's matrix as the image is, inside the part's outline and the replacement's area (the sand).
    /// </summary>
    void DrawVectorImage(ShapePart part, MovieAsset movie, CxForm cx)
    {
        if (part.Fill is not { Type: "bitmap", Bitmap: int id } fill || movie.Symbol(id) is not ImageAsset { Vector: not null, Image: { } img } image) return;
        Canvas.Save();
        if (image.VectorHeight > img.Height)
        {
            // Continuing below the image: its shape's outline and the replacement's full height.
            using var below = new SKPath();
            below.AddRect(SKRect.Create(img.Width, image.VectorHeight));
            below.Transform(Geo.Matrix(fill.Matrix));
            using var area = part.Path.Op(below, SKPathOp.Union) ?? new SKPath(part.Path);
            Canvas.ClipPath(area, SKClipOperation.Intersect, true);
        }
        else Canvas.ClipPath(part.Path, SKClipOperation.Intersect, true);
        Canvas.Concat(Geo.Matrix(fill.Matrix));
        // Drawn once at the screen's resolution (again only when that changes, a quarter step at a time): the
        // shapes, texture and its blending cost too much to draw every frame, and as an image this is as cheap
        // as the photo it replaces.
        var m = Canvas.TotalMatrix;
        // The exact screen scale (to 1%): drawn pixel for pixel, not resampled (panning moves it, not rescales it).
        float scale = MathF.Round(MathF.Sqrt(MathF.Abs(m.ScaleX * m.ScaleY - m.SkewX * m.SkewY)) * 100) / 100;
        scale = Math.Clamp(scale, 0.25f, 6);
        long now = Environment.TickCount64;
        image.VectorUsed[scale] = now;
        while (image.VectorDone.TryDequeue(out var done))
        {
            image.VectorPending.Remove(done.Scale);
            if (done.Image == null) continue;
            // The scales no longer drawn (for a couple of seconds) let go; those still drawn are kept.
            foreach (var k in image.VectorCache.Keys.Where(k => k != done.Scale && now - image.VectorUsed.GetValueOrDefault(k) > 2000).ToList())
            {
                image.VectorUsed.Remove(k);
                image.VectorCache[k].Dispose();
                image.VectorCache.Remove(k);
            }
            if (image.VectorCache.Remove(done.Scale, out var old)) old.Dispose();
            image.VectorCache[done.Scale] = done.Image;
        }
        // Drawn in the background (a few hundred ms) while the photo, underneath, shows: never a hitch.
        if (!image.VectorCache.TryGetValue(scale, out var cached) && image.VectorPending.Add(scale))
        {
            float at = scale;
            System.Threading.Interlocked.Increment(ref VectorImagesPending);
            System.Threading.Tasks.Task.Run(() =>
            {
                long t0 = System.Diagnostics.Stopwatch.GetTimestamp();
                using var job = Watchdog.Job($"drawing vector picture {image.Movie.Name}#{image.Id} at x{at}");
                SKImage? made = null;
                try { made = RenderVectorImage(image, img, at); }
                catch (Exception e) { Console.WriteLine($"vector image {image.Movie.Name}#{image.Id}: {e.Message}"); }
                image.VectorDone.Enqueue((at, made));
                // The artwork drawn meanwhile (with the photo) may be held in the layer cache: it's redrawn.
                VectorImageReady = true;
                System.Threading.Interlocked.Decrement(ref VectorImagesPending);
                if (Environment.GetEnvironmentVariable("MNOG_VECDBG") == "1")
                    Console.WriteLine($"vector image {image.Movie.Name}#{image.Id} drawn at x{at} in {System.Diagnostics.Stopwatch.GetElapsedTime(t0).TotalMilliseconds:0} ms");
            });
        }
        // A scale still being drawn: the nearest one ready meanwhile.
        cached ??= image.VectorCache.OrderBy(kv => Math.Abs(kv.Key - scale)).Select(kv => kv.Value).FirstOrDefault();
        if (cached != null)
        {
            using var paint = new SKPaint();
            if (!cx.IsIdentity) paint.ColorFilter = cx.ToFilter();
            var full = SKRect.Create(img.Width, Math.Max(img.Height, image.VectorHeight));
            if ((fill.Repeat == true || fill.Spread == "reflect") && image.VectorHeight <= img.Height)
            {
                // A repeating fill (II.4-N's sand strip tiles it from far off the image), or a mirrored one (the skies,
                // continued past their sides with their mirror image): tiled as the photo is.
                var tile = fill.Repeat == true ? SKShaderTileMode.Repeat : SKShaderTileMode.Mirror;
                paint.Shader = cached.ToShader(tile, tile, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None),
                    SKMatrix.CreateScale(full.Width / cached.Width, full.Height / cached.Height));
                Canvas.DrawPaint(paint);
            }
            else if (ClipMasks && paint.ColorFilter != null)
            {
                // (SVG output drops an image's colour filter: the image is tinted itself.)
                using var tinted = Tinted(cached, paint.ColorFilter);
                Canvas.DrawImage(tinted, full, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
            }
            else Canvas.DrawImage(cached, full, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None), paint);
        }
        Canvas.Restore();
    }

    /// <summary>
    /// The dialog boxes' translucent panels (TextSub's and MiniSub's "BackBox", MiniSub's answer row): with
    /// Vectorize bitmaps on, what's behind them is blurred (frosted glass), in dialogs whose live backdrop asks for it
    /// ("frost"), where the scene showing through sharp makes the text hard to read.
    /// </summary>
    public static bool IsFrosted(DisplayObject c) =>
        c is MovieClip { Movie.Name: var m } mc && (m.Equals("TextSub", StringComparison.OrdinalIgnoreCase) || m.Equals("MiniSub", StringComparison.OrdinalIgnoreCase)) &&
        (mc.Name == "BackBox" || m.Equals("MiniSub", StringComparison.OrdinalIgnoreCase) && mc.Parent is MovieClip { LoadedRoot: true } && mc.Depth == 5) &&
        DialogFrosted(mc);

    /// <summary>Whether a dialog holding this object (a movie it's loaded into) has a backdrop that asks for frosting.</summary>
    static bool DialogFrosted(DisplayObject c)
    {
        for (var p = c.Parent; p != null; p = p.Parent)
            if (p is MovieClip { LoadedRoot: true } root && c.Player.BackdropFrosted(root.Movie)) return true;
        return false;
    }

    const float FrostBlur = 5; // stage pixels

    /// <summary>Blurs what's already drawn under an object's bounds.</summary>
    void Frost(DisplayObject c, SKMatrix world)
    {
        var bounds = c.LocalBounds;
        if (bounds.IsEmpty) return;
        var device = Base.PreConcat(world);
        var rect = device.MapRect(bounds);
        float scale = MathF.Sqrt(MathF.Abs(device.ScaleX * device.ScaleY - device.SkewX * device.SkewY));
        float sigma = FrostBlur * scale;
        Canvas.Save();
        Canvas.ResetMatrix();
        Canvas.ClipRect(rect);
        using var blur = SKImageFilter.CreateBlur(sigma, sigma, SKShaderTileMode.Clamp);
        Canvas.SaveLayer(new SKCanvasSaveLayerRec { Bounds = rect, Backdrop = blur });
        Canvas.Restore();
        Canvas.Restore();
    }

    /// <summary>Set when a vector image finished drawing in the background (Player.Render then clears the layer cache).</summary>
    public static volatile bool VectorImageReady;

    /// <summary>Vector images being drawn in the background (the scene crossfade waits for them).</summary>
    public static int VectorImagesPending;

    /// <summary>
    /// A bitmap fill whose image is a live backdrop (Player.Backdrop): the scene it came from, zoomed into the
    /// image's place, drawn into an image at the screen's resolution and blurred there. False when there's none.
    /// </summary>
    /// <summary>A live backdrop's painted shapes (image pixels, mapped by m): those beneath the scene, or those on top.</summary>
    static void DrawBackdropShapes(LiveBackdrop b, SKCanvas canvas, SKMatrix m, bool under, float sigma)
    {
        foreach (var (points, from, to, colors, radial, isUnder) in b.Shapes)
        {
            if (isUnder != under) continue;
            using var path = new SKPath();
            path.AddPoly(points.Select(m.MapPoint).ToArray());
            using var shape = new SKPaint
            {
                IsAntialias = true,
                Shader = radial
                    ? SKShader.CreateRadialGradient(m.MapPoint(from), SKPoint.Distance(m.MapPoint(from), m.MapPoint(to)), colors, null, SKShaderTileMode.Clamp)
                    : SKShader.CreateLinearGradient(m.MapPoint(from), m.MapPoint(to), colors, null, SKShaderTileMode.Clamp),
                MaskFilter = sigma > 0.3f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, sigma) : null,
            };
            canvas.DrawPath(path, shape);
        }
    }

    static readonly byte[] IdentityTable = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();

    bool DrawLiveBackdrop(ShapePart part, MovieAsset movie, SKMatrix world, CxForm cx)
    {
        if (part.Fill is not { Type: "bitmap", Bitmap: int id } fill || movie.Symbol(id) is not ImageAsset { Image: { } img }) return false;
        if (player.BackdropReplaces(movie, id)) return true;
        if (player.Backdrop(movie, id) is not { Root: { } root } b) return false;
        // Image pixels to the screen, and the scene's stage to image pixels.
        var toDevice = Base.PreConcat(world).PreConcat(Geo.Matrix(fill.Matrix));
        // Mirrored ("mirror"): the painting was the scene flipped; x is then the scene point at the image's left edge.
        var sceneToImage = b.Mirror
            ? new SKMatrix(-b.Zoom, 0, b.Zoom * b.X, 0, b.Zoom, -b.Zoom * b.Y, 0, 0, 1)
            : SKMatrix.CreateScale(b.Zoom, b.Zoom).PreConcat(SKMatrix.CreateTranslation(-b.X, -b.Y));
        float scale = MathF.Sqrt(MathF.Abs(toDevice.ScaleX * toDevice.ScaleY - toDevice.SkewX * toDevice.SkewY));
        float sigma = b.Blur * scale;
        // The image's area on screen (within the canvas), drawn a little wider so the blur has scenery at its edges.
        var imageRect = b.Wide ? new SKRect(-100000, 0, img.Width + 100000, img.Height) : SKRect.Create(img.Width, img.Height);
        var area = toDevice.MapRect(imageRect);
        var device = Canvas.DeviceClipBounds;
        area.Intersect(new SKRect(device.Left, device.Top, device.Right, device.Bottom));
        if (area.IsEmpty) return true;
        float pad = MathF.Ceiling(sigma * 3);
        var region = SKRectI.Round(SKRect.Inflate(area, pad, pad));
        if (region.Width <= 0 || region.Height <= 0) return true;
        var info = new SKImageInfo(region.Width, region.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = Canvas.Context is GRRecordingContext gpu ? SKSurface.Create(gpu, false, info) : SKSurface.Create(info);
        if (surface == null) return false;
        surface.Canvas.Clear(SKColors.Black);
        // Lighting drawn beneath the scene (blurred with it, below).
        DrawBackdropShapes(b, surface.Canvas, SKMatrix.CreateTranslation(-region.Left, -region.Top).PreConcat(toDevice), true, 0);
        var sceneBase = SKMatrix.CreateTranslation(-region.Left, -region.Top).PreConcat(toDevice).PreConcat(sceneToImage);
        // In-focus objects are left out of the blurred pass, then drawn sharp on top (below).
        // Hazier objects first, each blurred extra (the whole backdrop's blur comes on top of it).
        foreach (var (o, extra) in b.SofterObjects)
        {
            if (!o.Visible) continue;
            var m = o.DrawMatrix(T);
            for (var p = o.Parent; p != null && p != root; p = p.Parent) m = p.DrawMatrix(T).PreConcat(m);
            float es = extra * scale;
            using var haze = new SKPaint { ImageFilter = SKImageFilter.CreateBlur(es, es) };
            surface.Canvas.SaveLayer(haze);
            o.Render(new RenderContext(surface.Canvas, sceneBase, player, T), m, o.DrawColor(T));
            surface.Canvas.Restore();
        }
        var softerShown = b.SofterObjects.Where(t => t.Obj.Visible).Select(t => t.Obj).ToList();
        foreach (var o in softerShown) o.Visible = false;
        foreach (var o in b.SharpObjects) o.Visible = false;
        var sharpAtShown = b.SharpAtObjects.Where(t => t.Obj.Visible).ToList();
        foreach (var (o, _) in sharpAtShown) o.Visible = false;
        new RenderContext(surface.Canvas, sceneBase, player, T).DrawContainer(root.Children, SKMatrix.Identity, CxForm.Identity);
        foreach (var o in b.SharpObjects) o.Visible = true;
        foreach (var (o, _) in sharpAtShown) o.Visible = true;
        foreach (var o in softerShown) o.Visible = true;
        using var snapshot = surface.Snapshot();
        Canvas.Save();
        if (b.Wide)
        {
            Canvas.ResetMatrix();
            Canvas.ClipRect(area);
        }
        else
        {
            SetMatrix(world);
            Canvas.ClipPath(part.Path, SKClipOperation.Intersect, true);
            if (b.Clip is { } outline)
            {
                // ("clip": its outline, in the image's pixels.)
                using var inImage = new SKPath(outline);
                inImage.Transform(Geo.Matrix(fill.Matrix));
                Canvas.ClipPath(inImage, SKClipOperation.Intersect, true);
            }
            Canvas.ResetMatrix();
        }
        using var paint = new SKPaint { ImageFilter = sigma > 0.3f ? SKImageFilter.CreateBlur(sigma, sigma) : null };
        if (!cx.IsIdentity) paint.ColorFilter = cx.ToFilter();
        if (b.Tone is { } tone)
        {
            using var curve = SKColorFilter.CreateTable(IdentityTable, tone[0], tone[1], tone[2]);
            paint.ColorFilter = paint.ColorFilter is { } first ? SKColorFilter.CreateCompose(first, curve) : curve;
        }
        Canvas.DrawImage(snapshot, region.Left, region.Top, paint);
        foreach (var (top, bottom, color) in b.Light)
        {
            var p0 = toDevice.MapPoint(0, top); var p1 = toDevice.MapPoint(0, bottom);
            using var light = new SKPaint
            {
                BlendMode = SKBlendMode.Plus,
                Shader = SKShader.CreateLinearGradient(p0, p1, [color.WithAlpha(0), color], SKShaderTileMode.Clamp),
            };
            var band = toDevice.MapRect(new SKRect(b.Wide ? -100000 : 0, top, b.Wide ? img.Width + 100000 : img.Width, bottom + (b.Wide ? 100000 : 0)));
            band.Intersect(area);
            Canvas.DrawRect(band, light);
        }
        DrawBackdropShapes(b, Canvas, toDevice, false, sigma);
        foreach (var (fx, fy, fr, color, ring) in b.Flares)
        {
            var c = toDevice.MapPoint(fx, fy);
            float r = fr * scale;
            var clear = color.WithAlpha(0);
            using var flare = new SKPaint
            {
                BlendMode = SKBlendMode.Screen,
                Shader = ring
                    ? SKShader.CreateRadialGradient(c, r, [clear, clear, color, clear], [0, 0.62f, 0.88f, 1], SKShaderTileMode.Clamp)
                    : SKShader.CreateRadialGradient(c, r, [color, color.WithAlpha((byte)(color.Alpha * 0.6f)), clear], [0, 0.5f, 1], SKShaderTileMode.Clamp),
            };
            Canvas.DrawCircle(c, r, flare);
        }
        foreach (var (gx, gy, gr, color) in b.Glows)
        {
            var c = toDevice.MapPoint(gx, gy);
            float r = gr * scale;
            // The lightstone item's own glow (backpack shape 58): white at the centre, yellow, then clear.
            using var glow = new SKPaint
            {
                Shader = SKShader.CreateRadialGradient(c, r,
                    [new SKColor(255, 255, 255, 230), new SKColor(255, 255, 255, 230), new SKColor(255, 255, 102, 179), new SKColor(255, 255, 204, 0)],
                    [0, 0.1725f, 0.3529f, 1], SKShaderTileMode.Clamp),
            };
            Canvas.DrawCircle(c, r, glow);
        }
        if (b.Bloom is var (threshold, radius, strength, tint))
        {
            // The parts brighter than the threshold (what's below goes to black), tinted, blurred and added on.
            float t = threshold / 255f;
            float g(int i) => strength * tint[Math.Min(i, tint.Length - 1)] / Math.Max(0.05f, 1 - t);
            using var bright = SKColorFilter.CreateColorMatrix([
                g(0), 0, 0, 0, -t * g(0),
                0, g(1), 0, 0, -t * g(1),
                0, 0, g(2), 0, -t * g(2),
                0, 0, 0, 1, 0]);
            float bs = radius * scale;
            using var glow = new SKPaint
            {
                BlendMode = SKBlendMode.Plus,
                ImageFilter = SKImageFilter.CreateBlur(sigma + bs, sigma + bs, SKImageFilter.CreateColorFilter(bright)),
            };
            Canvas.DrawImage(snapshot, region.Left, region.Top, glow);
        }
        // Less blurred objects, each at its own blur, beneath the sharp ones.
        foreach (var (o, blurAt) in b.SharpAtObjects)
        {
            if (!o.Visible) continue;
            var m = o.DrawMatrix(T);
            for (var p = o.Parent; p != null && p != root; p = p.Parent) m = p.DrawMatrix(T).PreConcat(m);
            float s = blurAt * scale;
            using var lessBlur = s > 0.3f ? new SKPaint { ImageFilter = SKImageFilter.CreateBlur(s, s) } : null;
            if (lessBlur != null) Canvas.SaveLayer(lessBlur);
            o.Render(new RenderContext(Canvas, toDevice.PreConcat(sceneToImage), player, T), m, cx.Concat(o.DrawColor(T)));
            if (lessBlur != null) Canvas.Restore();
        }
        if (b.SharpObjects.Count > 0)
        {
            var sharpBase = toDevice.PreConcat(sceneToImage);
            var sharp = new RenderContext(Canvas, sharpBase, player, T);
            float sharpSigma = b.SharpBlur * scale;
            using var soft = sharpSigma > 0.3f ? new SKPaint { ImageFilter = SKImageFilter.CreateBlur(sharpSigma, sharpSigma) } : null;
            if (soft != null) Canvas.SaveLayer(soft);
            foreach (var o in b.SharpObjects)
            {
                var m = o.DrawMatrix(T);
                for (var p = o.Parent; p != null && p != root; p = p.Parent) m = p.DrawMatrix(T).PreConcat(m);
                o.Render(sharp, m, cx.Concat(o.DrawColor(T)));
            }
            if (soft != null) Canvas.Restore();
        }
        Canvas.Restore();
        return true;
    }

    /// <summary>(For the test harness: an image's vector replacement drawn on its own.)</summary>
    public static SKImage? RenderVectorImageAlone(ImageAsset image, float scale) =>
        image.Image is { } img && image.Vector != null ? RenderVectorImage(image, img, scale) : null;

    /// <summary>A vector replacement as an image, at the given scale (screen pixels per image pixel).</summary>
    static SKImage? RenderVectorImage(ImageAsset image, SKImage img, float scale)
    {
        float vh = Math.Max(img.Height, image.VectorHeight);
        int w = (int)MathF.Ceiling(img.Width * scale), h = (int)MathF.Ceiling(vh * scale);
        using var surface = SKSurface.Create(new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Premul));
        if (surface == null) return null;
        var c = surface.Canvas;
        c.Clear(SKColors.Transparent);
        c.Scale(scale);
        // Faded as a whole (Mt Ihu into the photo's mist): drawn in a layer, then masked.
        if (image.VectorFade != null) c.SaveLayer(SKRect.Create(img.Width, vh), null);
        var rect = SKRect.Create(img.Width, vh);
        DrawLayer(surface, image.VectorClip, image.Vector!, image.VectorSeal, image.VectorTexture, image.VectorTextureSize, image.VectorTextureSize,
            image.VectorTextureBlend, image.VectorTextureFade, rect, scale);
        // The other layers ("layers"), each drawn apart (its texture only on its own shapes), then laid on.
        foreach (var layer in image.VectorLayers)
        {
            using var own = SKSurface.Create(new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Premul));
            if (own == null) continue;
            own.Canvas.Clear(SKColors.Transparent);
            own.Canvas.Scale(scale);
            DrawLayer(own, layer.Clip, layer.Parts, image.VectorSeal, layer.Texture, layer.TextureSize, layer.TextureHeight > 0 ? layer.TextureHeight : layer.TextureSize,
                layer.TextureBlend, layer.TextureFade, rect, scale);
            using var drawnLayer = own.Snapshot();
            c.Save();
            c.ResetMatrix();
            c.DrawImage(drawnLayer, 0, 0);
            c.Restore();
        }
        if (image.VectorFade is { } whole)
        {
            using var mask = new SKPaint { Shader = whole, BlendMode = SKBlendMode.DstIn };
            c.DrawRect(SKRect.Create(img.Width, vh), mask);
            c.Restore();
        }
        return surface.Snapshot();
    }

    /// <summary>One replacement layer's shapes (inside its clip) and its texture over them, onto the surface (scaled).</summary>
    static void DrawLayer(SKSurface surface, SKPath? clip, List<VectorPart> parts, bool sealEdges, SKPicture? texture, float textureSize, float textureHeight,
                          SKBlendMode textureBlend, SKShader? textureFade, SKRect rect, float scale)
    {
        var c = surface.Canvas;
        c.Save();
        if (clip != null) c.ClipPath(clip, SKClipOperation.Intersect, true);
        // (Dithered: a long, subtle gradient, e.g. dark water, would otherwise step visibly from one level to the next.)
        using var paint = new SKPaint { IsAntialias = true, IsDither = true };
        using var seal = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 0 };
        foreach (var part in parts)
        {
            // (A gradient's own stops give its colours and opacity: the part's colour, its last stop's, mustn't
            // scale it, or one fading to transparent wouldn't be drawn at all.)
            var color = part.Fill != null ? SKColors.Black : part.Color;
            paint.Color = color;
            paint.Shader = part.Fill;
            if (part.Clip != null) { c.Save(); c.ClipPath(part.Clip, SKClipOperation.Intersect, true); }
            if (part.Stroke > 0)
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = part.Stroke;
                paint.StrokeCap = part.Cap;
                paint.StrokeJoin = part.Join;
                paint.StrokeMiter = part.Miter;
                c.DrawPath(part.Path, paint);
                paint.Style = SKPaintStyle.Fill;
                if (part.Clip != null) c.Restore();
                continue;
            }
            if (part.Blur > 0)
            {
                // (Its edge softened: the blur follows the canvas's scale, so it's the same in the image's pixels.)
                using var soft = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, part.Blur, true);
                paint.MaskFilter = soft;
                c.DrawPath(part.Path, paint);
                paint.MaskFilter = null;
                if (part.Clip != null) c.Restore();
                continue;
            }
            c.DrawPath(part.Path, paint);
            if (part.Clip != null) c.Restore();
            if (sealEdges)
            {
                seal.Color = color;
                seal.Shader = part.Fill;
                c.DrawPath(part.Path, seal);
            }
        }
        paint.Shader = null;
        seal.Shader = null;
        if (texture is { } tile && textureSize > 0)
        {
            // (Only where the shapes are drawn: an artwork with transparent parts isn't textured around it.)
            using var drawn = surface.Snapshot();
            // The texture in a layer of its own (blended onto the shapes as a whole), its tiles drawn as the vector
            // shapes they are, faded down the image.
            using var layer = new SKPaint { BlendMode = textureBlend };
            c.SaveLayer(rect, layer);
            // One tile drawn at this scale (its shapes, sharp), then repeated pixel for pixel.
            float size = textureSize, sizeY = textureHeight;
            int px = Math.Max(1, (int)MathF.Round(size * scale)), py = Math.Max(1, (int)MathF.Round(sizeY * scale));
            using (var tileSurface = SKSurface.Create(new SKImageInfo(px, py, SKColorType.Rgba8888, SKAlphaType.Premul)))
            {
                tileSurface.Canvas.Clear(SKColors.Transparent);
                tileSurface.Canvas.Scale(px / size, py / sizeY);
                tileSurface.Canvas.DrawPicture(tile);
                using var tileImage = tileSurface.Snapshot();
                using var shader = tileImage.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, new SKSamplingOptions(SKFilterMode.Nearest),
                    SKMatrix.CreateScale(size / px, sizeY / py));
                using var tex = new SKPaint { Shader = shader };
                c.DrawRect(rect, tex);
            }
            if (textureFade is { } fade)
            {
                using var mask = new SKPaint { Shader = fade, BlendMode = SKBlendMode.DstIn };
                c.DrawRect(rect, mask);
            }
            c.Save();
            c.ResetMatrix();
            using (var within = new SKPaint { BlendMode = SKBlendMode.DstIn }) c.DrawImage(drawn, 0, 0, within);
            c.Restore();
            c.Restore();
        }
        c.Restore();
    }

    void ConfigurePaint(ShapePart part, MovieAsset movie, CxForm cx, SKMatrix world)
    {
        if (!part.ShaderReady)
        {
            part.ShaderReady = true;
            var fill = part.Fill ?? part.Stroke?.Fill;
            if (fill != null && fill.Type != "solid") part.Shader = MakeShader(fill, movie);
            if (fill is { Type: "bitmap", Bitmap: int hid } && movie.Symbol(hid) is ImageAsset { HiRes: not null } hiImg)
            {
                part.HiResShader = MakeShader(fill, movie, hiRes: true);
                part.HiResAlways = hiImg.HiResAlways;
            }
            part.Color = Geo.Color(fill?.Color ?? part.Stroke?.Color);
        }
        _paint.Reset();
        _paint.IsAntialias = true;
        if (part.Shader != null)
        {
            _paint.Shader = (player.VectorImages || part.HiResAlways) && part.HiResShader != null ? part.HiResShader : part.Shader;
            // (SVG output has no colour filters: a gradient's colours are tinted themselves.)
            if (ClipMasks && !cx.IsIdentity && (part.Fill ?? part.Stroke?.Fill) is { Type: not "bitmap" and not "solid" } grad)
            {
                _svgShader?.Dispose();
                _paint.Shader = _svgShader = MakeShader(grad, movie, tint: cx);
            }
            else if (!cx.IsIdentity) _paint.ColorFilter = cx.ToFilter();
        }
        else
        {
            // Textbox transparency Low: the conversations' see-through white boxes (TextSub's and MiniSub's: the text's
            // at 45%, the answers' at 35%) drawn much less see-through (85%, 72%); the faint highlight on the next-line
            // arrow as it is.
            var color = part.Color;
            if (player.LowTextboxTransparency && movie.Name is "TextSub" or "MiniSub")
                color = color.Alpha == 0x73 ? color.WithAlpha(0xD9) : color.Alpha == 0x5A ? color.WithAlpha(0xB8) : color;
            _paint.Color = cx.Apply(color);
        }
        if (part.Stroke is { } st) ConfigureStroke(st, world);
    }

    /// <summary>
    /// Fills meeting along an edge are each antialiased on their own, so the edge is only partly covered and
    /// what's behind shows through as a thin seam (Flash fills a shape's regions together, without one). The lower
    /// of the two fills gets a hairline along the edge in its own paint (ShapeAsset.SharedEdges), covering the edge
    /// pixel fully under the upper one's soft edge; the shape's outer edges are left as they are.
    /// </summary>
    void Seal(SKPath path)
    {
        _paint.Style = SKPaintStyle.Stroke;
        _paint.StrokeWidth = 0;
        if (ClipMasks)
        {
            // (SVG has no hairlines: one device pixel's width in the path's own units.)
            var m = Canvas.TotalMatrix;
            float scale = (float)Math.Sqrt(Math.Abs(m.ScaleX * m.ScaleY - m.SkewX * m.SkewY));
            if (scale > 0) _paint.StrokeWidth = 1 / scale;
        }
        Canvas.DrawPath(path, _paint);
        _paint.Style = SKPaintStyle.Fill;
    }

    void ConfigureStroke(StrokeDoc st, SKMatrix world)
    {
        _paint.Style = SKPaintStyle.Stroke;
        // Flash never draws a stroke thinner than one device pixel; those are drawn as hairlines (width 0),
        // which Skia renders directly instead of outlining the stroke, much faster for detailed artwork.
        var dm = Base.PreConcat(world);
        float scale = (float)Math.Sqrt(Math.Abs(dm.ScaleX * dm.ScaleY - dm.SkewX * dm.SkewY));
        float w = (float)st.Width;
        // (SVG has no hairlines, its stroke widths are in the path's own units: one device pixel's width there.)
        _paint.StrokeWidth = w * scale < 1 ? ClipMasks && scale > 0 ? 1 / scale : 0 : w;
        _paint.StrokeCap = st.Cap switch { "none" => SKStrokeCap.Butt, "square" => SKStrokeCap.Square, _ => SKStrokeCap.Round };
        _paint.StrokeJoin = st.Join switch { "bevel" => SKStrokeJoin.Bevel, "miter" => SKStrokeJoin.Miter, _ => SKStrokeJoin.Round };
        if (st.MiterLimit is double ml) _paint.StrokeMiter = (float)ml;
    }

    static SKShader? MakeShader(FillDoc f, MovieAsset movie, bool hiRes = false, CxForm? tint = null)
    {
        var m = Geo.Matrix(f.Matrix);
        if (f.Type == "bitmap")
        {
            if (f.Bitmap is not int id || movie.Symbol(id) is not ImageAsset img || img.Image == null) return null;
            // "reflect" (a port extension, see Patches.SkyEdges) continues an image with its mirror image.
            var tile = f.Repeat == true ? SKShaderTileMode.Repeat : f.Spread == "reflect" ? SKShaderTileMode.Mirror : SKShaderTileMode.Clamp;
            // Its higher-resolution drawing (with Vectorize bitmaps on): scaled down to the image's size.
            if (hiRes && img.HiRes is { } hi)
                // (Without mipmaps: Skia rebuilt them for this large image every frame, 40 ms at 1080p (IX.4-E's sky1);
                // it's drawn near its own size, where they add nothing.)
                return hi.ToShader(tile, tile, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None),
                    m.PreConcat(SKMatrix.CreateScale((float)img.Image.Width / hi.Width, (float)img.Image.Height / hi.Height)));
            return img.Image.ToShader(tile, tile, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear), m);
        }
        var stops = f.Stops ?? [];
        var colors = stops.Select(s => tint is { } t ? t.Apply(Geo.Color(s.Color)) : Geo.Color(s.Color)).ToArray();
        var pos = stops.Select(s => (float)s.At).ToArray();
        if (colors.Length == 0) return null;
        if (colors.Length == 1) { colors = [colors[0], colors[0]]; pos = [0, 1]; }
        if (tint is { } tc && stops.Count > 1)
        {
            // (A tint is applied to the blended colours, which it may clip, not just the stops: resampled finely,
            // each sample blended from the original stops, then tinted.)
            var raw = stops.Select(st => Geo.Color(st.Color)).ToArray();
            const int n = 48;
            var sp = new List<float>(); var sc = new List<SKColor>();
            for (int k = 0; k <= n; k++)
            {
                float at = pos[0] + (pos[^1] - pos[0]) * k / n;
                int j = 1;
                while (j < pos.Length - 1 && pos[j] < at) j++;
                float span = pos[j] - pos[j - 1], u = span > 0 ? Math.Clamp((at - pos[j - 1]) / span, 0, 1) : 0;
                SKColor a0 = raw[j - 1], a1 = raw[j];
                var mix = new SKColor((byte)(a0.Red + (a1.Red - a0.Red) * u), (byte)(a0.Green + (a1.Green - a0.Green) * u),
                    (byte)(a0.Blue + (a1.Blue - a0.Blue) * u), (byte)(a0.Alpha + (a1.Alpha - a0.Alpha) * u));
                sp.Add(at); sc.Add(tc.Apply(mix));
            }
            colors = [.. sc]; pos = [.. sp];
        }
        var mode = f.Spread switch { "reflect" => SKShaderTileMode.Mirror, "repeat" => SKShaderTileMode.Repeat, _ => SKShaderTileMode.Clamp };
        return f.Type switch
        {
            "linear" => SKShader.CreateLinearGradient(new SKPoint(-1, 0), new SKPoint(1, 0), colors, pos, mode, m),
            "focal" => SKShader.CreateTwoPointConicalGradient(new SKPoint((float)(f.Focal ?? 0), 0), 0, new SKPoint(0, 0), 1, colors, pos, mode, m),
            _ => SKShader.CreateRadialGradient(new SKPoint(0, 0), 1, colors, pos, mode, m),
        };
    }

    // ---- Morph shapes ----

    void DrawMorph(MorphAsset m, float t, SKMatrix world, CxForm cx)
    {
        SetMatrix(world);
        for (int i = 0; i < m.Parts.Count; i++)
        {
            var p = m.Parts[i];
            var path = player.MorphPath(m, i, t);
            _paint.Reset();
            _paint.IsAntialias = true;
            var start = p.FillStart ?? p.StrokeStart?.Fill;
            var end = p.FillEnd ?? p.StrokeEnd?.Fill;
            if (start != null && start.Type != "solid")
            {
                var lerped = LerpFill(start, end ?? start, t);
                bool tintStops = ClipMasks && !cx.IsIdentity && lerped.Type != "bitmap";
                var shader = MakeShader(lerped, m.Movie, tint: tintStops ? cx : null);
                _paint.Shader = shader;
                if (!cx.IsIdentity && !tintStops) _paint.ColorFilter = cx.ToFilter();
            }
            else
            {
                var c0 = Geo.Color(start?.Color ?? p.StrokeStart?.Color);
                var c1 = Geo.Color(end?.Color ?? p.StrokeEnd?.Color);
                _paint.Color = cx.Apply(LerpColor(c0, c1, t));
            }
            if (p.StrokeStart is { } s0)
            {
                var s1 = p.StrokeEnd ?? s0;
                ConfigureStroke(new StrokeDoc { Width = s0.Width + (s1.Width - s0.Width) * t, Cap = s0.Cap, Join = s0.Join, MiterLimit = s0.MiterLimit }, world);
            }
            Canvas.DrawPath(path, _paint);
            _paint.Shader?.Dispose();
        }
        Canvas.ResetMatrix();
    }

    static SKColor LerpColor(SKColor a, SKColor b, float t) => new(
        (byte)(a.Red + (b.Red - a.Red) * t), (byte)(a.Green + (b.Green - a.Green) * t),
        (byte)(a.Blue + (b.Blue - a.Blue) * t), (byte)(a.Alpha + (b.Alpha - a.Alpha) * t));

    static FillDoc LerpFill(FillDoc a, FillDoc b, float t)
    {
        double L(double x, double y) => x + (y - x) * t;
        var r = new FillDoc
        {
            Type = a.Type, Spread = a.Spread, Bitmap = a.Bitmap, Repeat = a.Repeat, Smooth = a.Smooth, Focal = a.Focal,
            Matrix = a.Matrix == null || b.Matrix == null ? a.Matrix : a.Matrix.Zip(b.Matrix, L).ToArray(),
        };
        if (a.Stops != null && b.Stops != null)
            r.Stops = a.Stops.Zip(b.Stops, (x, y) => new GradientStopDoc
            {
                At = L(x.At, y.At),
                Color = Hex(LerpColor(Geo.Color(x.Color), Geo.Color(y.Color), t)),
            }).ToList();
        return r;
    }

    static string Hex(SKColor c) => $"#{c.Red:X2}{c.Green:X2}{c.Blue:X2}{c.Alpha:X2}";

    // ---- Text ----

    public void DrawText(TextAsset t, SKMatrix world, CxForm cx)
    {
        var m = world.PreConcat(t.Matrix);
        foreach (var run in t.Doc.Runs)
        {
            if (t.Movie.Symbol(run.Font) is not FontAsset font) continue;
            _paint.Reset();
            _paint.IsAntialias = true;
            _paint.Color = cx.Apply(Geo.Color(run.Color));
            float scale = (float)run.Size / (float)font.Doc.UnitsPerEm;
            float x = (float)run.X;
            for (int i = 0; i < run.Glyphs.Count; i++)
            {
                int g = run.Glyphs[i];
                if (g >= 0 && g < font.Glyphs.Length)
                {
                    SetMatrix(m.PreConcat(SKMatrix.CreateScaleTranslation(scale, scale, x, (float)run.Y)));
                    Canvas.DrawPath(font.Glyphs[g], _paint);
                }
                x += (float)run.Advances[i];
            }
        }
        Canvas.ResetMatrix();
    }

    static readonly Dictionary<string, SKTypeface> Typefaces = [];

    static SKTypeface Typeface(string? name, bool bold, bool italic)
    {
        string family = name switch
        {
            null or "" or "_sans" => "Arial",
            "_serif" => "Times New Roman",
            "_typewriter" => "Courier New",
            _ => name,
        };
        var key = $"{family}|{bold}|{italic}";
        if (!Typefaces.TryGetValue(key, out var tf))
        {
            var style = new SKFontStyle(bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal, SKFontStyleWidth.Normal,
                italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
            tf = SKTypeface.FromFamilyName(family, style) ?? SKTypeface.Default;
            Typefaces[key] = tf;
        }
        return tf;
    }

    public void DrawEditText(EditTextObject e, SKMatrix world, CxForm cx)
    {
        var doc = e.Field.Doc;
        var bounds = e.Field.Bounds;
        SetMatrix(world);
        Canvas.Save();
        Canvas.ClipRect(bounds);
        if (doc.Border == true)
        {
            _paint.Reset();
            _paint.Color = cx.Apply(SKColors.White);
            Canvas.DrawRect(bounds, _paint);
            _paint.Style = SKPaintStyle.Stroke;
            _paint.Color = cx.Apply(SKColors.Black);
            _paint.StrokeWidth = 0;
            Canvas.DrawRect(bounds, _paint);
        }
        var font = doc.Font is int fid ? e.Field.Movie.Symbol(fid) as FontAsset : null;
        bool outlines = doc.UseOutlines == true && font != null && font.Glyphs.Length > 0;
        float size = (float)doc.Size;
        using var skFont = new SKFont(Typeface(font?.Doc.Name, font?.Doc.Bold == true, font?.Doc.Italic == true), size);
        _paint.Reset();
        _paint.IsAntialias = true;
        _paint.Color = cx.Apply(Geo.Color(doc.Color ?? "#000000FF"));

        string text = e.Text;
        if (doc.Password == true) text = new string('*', text.Length);
        float left = bounds.Left + 2 + (float)doc.LeftMargin;
        float width = bounds.Width - 4 - (float)(doc.LeftMargin + doc.RightMargin);
        float Measure(string s) => outlines ? GlyphWidth(font!, s, size) : skFont.MeasureText(s);
        var lines = Layout(text, doc.WordWrap == true && doc.Multiline == true || doc.WordWrap == true, width, Measure);

        float ascent = outlines && font!.Doc.Ascent is double asc ? (float)(asc / font.Doc.UnitsPerEm * size) : -skFont.Metrics.Ascent;
        float descent = outlines && font!.Doc.Descent is double desc ? (float)(desc / font.Doc.UnitsPerEm * size) : skFont.Metrics.Descent;
        float lineHeight = ascent + descent + (float)doc.Leading;
        float y = bounds.Top + 2 + ascent;
        // Smooth mode, text being typed out (a dialog adds a few letters each game frame): between frames the
        // new letters appear one after another, each fading in, instead of all at once.
        double? revealed = player.Smooth && e.PrevText is { } prev && text.Length > prev.Length && text.StartsWith(prev, StringComparison.Ordinal)
            ? prev.Length + (text.Length - prev.Length) * (double)T : null;
        byte alpha = _paint.Color.Alpha;
        int index = 0;
        foreach (var line in lines)
        {
            float w = Measure(line);
            float x = doc.Align switch
            {
                "right" => left + width - w,
                "center" => left + (width - w) / 2,
                _ => left + (float)doc.Indent,
            };
            if (revealed is double shown)
            {
                foreach (var ch in line)
                {
                    double a = Math.Clamp(shown - index, 0, 1);
                    if (a <= 0) break;
                    _paint.Color = _paint.Color.WithAlpha((byte)Math.Round(alpha * a));
                    string s = ch.ToString();
                    if (outlines) DrawGlyphs(font!, s, x, y, size, world);
                    else Canvas.DrawText(s, x, y, skFont, _paint);
                    x += Measure(s);
                    index++;
                }
                _paint.Color = _paint.Color.WithAlpha(alpha);
                index++; // the space or line break the line ended at
                if (index > shown) break;
            }
            else if (outlines) DrawGlyphs(font!, line, x, y, size, world);
            else Canvas.DrawText(line, x, y, skFont, _paint);
            y += lineHeight;
        }
        if (player.Focus == e && (player.ElapsedMs / 500) % 2 == 0)
        {
            var last = lines.Count > 0 ? lines[^1] : "";
            float cxPos = left + Measure(last);
            float cy = y - lineHeight;
            Canvas.DrawLine(cxPos, cy - ascent, cxPos, cy + descent, _paint);
        }
        Canvas.Restore();
        Canvas.ResetMatrix();
    }

    static float GlyphWidth(FontAsset font, string s, float size)
    {
        float w = 0;
        foreach (var ch in s) w += GlyphAdvance(font, ch, size);
        return w;
    }

    static float GlyphAdvance(FontAsset font, char ch, float size)
    {
        if (!font.ByChar.TryGetValue(ch, out int i)) return ch == ' ' ? size / 4 : 0;
        float scale = size / (float)font.Doc.UnitsPerEm;
        if (font.Doc.Glyphs[i].Advance is double adv) return (float)adv * scale;
        return (font.Glyphs[i].Bounds.Right + 50) * scale;
    }

    void DrawGlyphs(FontAsset font, string s, float x, float y, float size, SKMatrix world)
    {
        float scale = size / (float)font.Doc.UnitsPerEm;
        foreach (var ch in s)
        {
            if (font.ByChar.TryGetValue(ch, out int i))
            {
                Canvas.Save();
                Canvas.Translate(x, y);
                Canvas.Scale(scale);
                Canvas.DrawPath(font.Glyphs[i], _paint);
                Canvas.Restore();
            }
            x += GlyphAdvance(font, ch, size);
        }
    }

    static List<string> Layout(string text, bool wrap, float width, Func<string, float> measure)
    {
        var result = new List<string>();
        foreach (var para in text.Replace("\r\n", "\r").Replace('\n', '\r').Split('\r'))
        {
            if (!wrap || measure(para) <= width) { result.Add(para); continue; }
            var line = "";
            foreach (var word in para.Split(' '))
            {
                var candidate = line.Length == 0 ? word : line + " " + word;
                if (line.Length > 0 && measure(candidate) > width)
                {
                    result.Add(line);
                    line = word;
                }
                else line = candidate;
            }
            result.Add(line);
        }
        return result;
    }

    public void DrawBitmap(SKImage image, SKMatrix world, CxForm cx)
    {
        SetMatrix(world);
        _paint.Reset();
        _paint.IsAntialias = true;
        if (!cx.IsIdentity) _paint.ColorFilter = cx.ToFilter();
        Canvas.DrawImage(image, 0, 0, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear), _paint);
        Canvas.ResetMatrix();
    }

    // ---- Hit testing ----

    /// <param name="fillsOnly">Ignore strokes (for measuring what artwork covers).</param>
    public static bool HitGraphic(GraphicObject g, SKPoint p, SKMatrix world, bool fillsOnly = false)
    {
        if (!world.TryInvert(out var inv)) return false;
        var local = inv.MapPoint(p);
        switch (g.Asset)
        {
            case ShapeAsset s:
                if (!s.Bounds.Contains(local)) return false;
                foreach (var part in s.Parts)
                {
                    if (part.Fill != null)
                    {
                        if (part.Path.Bounds.Contains(local) && part.Path.Contains(local.X, local.Y)) return true;
                    }
                    if (part.Stroke != null && !fillsOnly && part.StrokeOutline.Contains(local.X, local.Y)) return true;
                }
                return false;
            case MorphAsset m:
                for (int i = 0; i < m.Parts.Count; i++)
                    if ((!fillsOnly || m.Parts[i].FillStart != null) && g.Player.MorphPath(m, i, g.Ratio).Contains(local.X, local.Y)) return true;
                return false;
            default:
                return g.LocalBounds.Contains(local);
        }
    }
}

using Mnog.Engine.Assets;
using Mnog.Engine.Script;
using Mnog.Format;
using SkiaSharp;

namespace Mnog.Engine.Display;

public sealed class MovieClip : DisplayObject
{
    public MovieAsset Movie;
    public TimelineAsset Timeline;
    public int CurrentFrame;
    public bool Playing = true;
    public int? Level;

    /// <summary>
    /// Shown but not run (a live backdrop's copy of a scene, Player.LiveBackdrop): its timelines animate, but no
    /// scripts, sounds or clip events; the clips it creates are inert too.
    /// </summary>
    public bool Inert;

    /// <summary>The root clip of a movie loaded by loadMovie (or a level); its sounds stop when it is unloaded.</summary>
    public bool LoadedRoot;

    /// <summary>The root of the loaded movie this clip belongs to.</summary>
    public MovieClip LoadedMovie
    {
        get
        {
            MovieClip c = this;
            while (!c.LoadedRoot && c.Parent is MovieClip p) c = p;
            return c;
        }
    }
    public readonly SortedList<int, DisplayObject> Children = [];
    public int CreatedTick;
    /// <summary>onClipEvent handlers (Flash 5), keyed by event name.</summary>
    public List<(HashSet<string> On, int? Key, Chunk? Script)>? ClipEvents;
    Assets.SoundAsset? _stream;

    readonly List<MovieClip> _pendingInit = [];

    public MovieClip(Player player, MovieAsset movie, TimelineAsset timeline) : base(player, player.Vm.MovieClipPrototype)
    {
        Movie = movie;
        Timeline = timeline;
        CreatedTick = player.TickCount;
    }

    public int FrameCount => Timeline.FrameCount;

    public override string TypeOf => "movieclip";

    public override SKRect LocalBounds
    {
        get
        {
            var r = SKRect.Empty;
            foreach (var c in Children.Values)
            {
                var b = c.BoundsInParent;
                if (b.IsEmpty) continue;
                r = r.IsEmpty ? b : SKRect.Union(r, b);
            }
            return r;
        }
    }

    public override SKRect StretchBounds
    {
        get
        {
            var r = SKRect.Empty;
            foreach (var c in Children.Values)
            {
                // (Parked off the stage by a script: not part of the artwork.)
                if (Math.Abs(c.Matrix.TransX) >= 1500 || Math.Abs(c.Matrix.TransY) >= 1500) continue;
                var b = c.BoundsInParent;
                if (b.IsEmpty) continue;
                r = r.IsEmpty ? b : SKRect.Union(r, b);
            }
            return r.IsEmpty ? LocalBounds : r;
        }
    }

    // ---- Variables and child lookup ----

    public DisplayObject? ChildByName(string name)
    {
        foreach (var c in Children.Values)
            if (c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return c;
        return null;
    }

    public override bool TryGetOwn(string name, out Value v)
    {
        if (base.TryGetOwn(name, out v)) return true;
        if (ChildByName(name) is { } child) { v = Value.From(child); return true; }
        return false;
    }

    public override bool HasOwn(string name) => base.HasOwn(name) || ChildByName(name) != null;

    public override Value GetProperty(int index, int version) => index switch
    {
        4 => CurrentFrame,
        5 => FrameCount,
        12 => FrameCount,
        14 => Player.DropTarget(this),
        // _url: the movie loaded there, as its file name (e.g. the chronicle remembering a sound holder's movie).
        15 => Movie.Name + ".swf",
        _ => base.GetProperty(index, version),
    };

    // ---- Timeline ----

    /// <summary>Builds frame 1 of a freshly created clip (its scripts are queued, not run).</summary>
    public void Construct()
    {
        CurrentFrame = 0;
        if (!Inert) Player.FireClipEvent(this, "load");
        GotoFrame(1);
    }

    /// <summary>
    /// One tick: child clips run first (newest/topmost first), then this clip advances. This is AVM1's
    /// execution order, and it decides whose frame scripts run first in a tick.
    /// </summary>
    public void RunFrame()
    {
        if (Removed) return;
        foreach (var c in Children.Values.Reverse().ToArray())
        {
            if (c.Removed) continue;
            if (c is MovieClip mc && mc.CreatedTick < Player.TickCount) mc.RunFrame();
            else if (c is ButtonObject b) b.RunFrame();
        }
        if (Playing && FrameCount > 1 && Advances())
            GotoFrame(CurrentFrame >= FrameCount ? 1 : CurrentFrame + 1, sequential: true);
    }

    /// <summary>Speed of play, in frames per frame (below 1 for a clip a live backdrop slows down).</summary>
    public float Rate = 1;
    float _rateDue;

    bool Advances()
    {
        if (Rate >= 1) return true;
        _rateDue += Rate;
        if (_rateDue < 1) return false;
        _rateDue -= 1;
        return true;
    }

    public void Play() => Playing = true;

    public void Stop()
    {
        Playing = false;
        StopStream();
    }

    public void NextFrame()
    {
        if (CurrentFrame < FrameCount) GotoFrame(CurrentFrame + 1);
        Stop();
    }

    public void PrevFrame()
    {
        if (CurrentFrame > 1) GotoFrame(CurrentFrame - 1);
        Stop();
    }

    public int? FrameOf(Value frame, int version)
    {
        if (frame.IsString)
        {
            var s = frame.RawString;
            if (Timeline.Labels.TryGetValue(s, out int f)) return f;
            double d = Value.ParseNumber(s, version);
            return double.IsNaN(d) ? null : (int)d;
        }
        double n = frame.ToNumber(version);
        return double.IsNaN(n) ? null : (int)n;
    }

    sealed class Placement
    {
        public int Id;
        public int PlaceFrame;
        public DisplayObject? Existing;
        public SKMatrix? Matrix;
        public CxForm? Cx;
        public float? Ratio;
        public string? Name;
        public int? ClipDepth;
    }

    /// <summary>
    /// Moves the playhead. The display list for the target frame is computed and reconciled with the
    /// current children, keeping instances that persist (Flash semantics for gotos and loops).
    /// </summary>
    public void GotoFrame(int target, bool sequential = false)
    {
        target = Math.Clamp(target, 1, FrameCount);
        if (target == CurrentFrame) return;
        bool rewind = target < CurrentFrame;
        if (!sequential || rewind) StopStream();

        var state = new Dictionary<int, Placement>();
        int from = 1;
        if (!rewind)
        {
            from = CurrentFrame + 1;
            foreach (var c in Children.Values)
                if (c.PlaceFrame > 0)
                    state[c.Depth] = new Placement { Id = c.Asset?.Id ?? 0, PlaceFrame = c.PlaceFrame, Existing = c };
        }
        for (int f = from; f <= target; f++)
        {
            foreach (var op in Timeline.Frame(f).Ops)
            {
                if (op.Remove is int rd) { state.Remove(rd); continue; }
                if (op.Place is not int depth) continue;
                state.TryGetValue(depth, out var p);
                if (op.Id is int id && (op.Move != true || p == null || p.Id != id))
                {
                    var np = new Placement { Id = id, PlaceFrame = f };
                    if (op.Move == true && p != null)
                    {
                        // Replacing the character keeps the old transform unless a new one is given.
                        np.Matrix = p.Matrix ?? p.Existing?.Matrix;
                        np.Cx = p.Cx ?? p.Existing?.ColorTransform;
                        np.Name = p.Name ?? p.Existing?.Name;
                        np.ClipDepth = p.ClipDepth;
                    }
                    p = state[depth] = np;
                }
                if (p == null) continue;
                if (op.Matrix != null) p.Matrix = Geo.Matrix(op.Matrix);
                if (op.Cxform != null) p.Cx = CxForm.From(op.Cxform);
                if (op.Ratio is double r) p.Ratio = (float)r;
                if (op.Name != null) p.Name = op.Name;
                if (op.ClipDepth is int cd) p.ClipDepth = cd;
            }
        }
        CurrentFrame = target;

        // Reconcile timeline-created children with the computed state.
        foreach (var c in Children.Values.ToArray())
        {
            if (c.PlaceFrame == 0) continue;
            if (state.TryGetValue(c.Depth, out var p) &&
                (p.Existing == c || (rewind && p.Existing == null && c.Asset?.Id == p.Id && c.PlaceFrame == p.PlaceFrame)))
            {
                Apply(c, p);
                state.Remove(c.Depth);
            }
            else RemoveChild(c);
        }
        foreach (var (depth, p) in state.OrderBy(kv => kv.Key))
        {
            if (p.Existing != null) continue;
            if (Children.TryGetValue(depth, out var occupant)) RemoveChild(occupant);
            var obj = Create(p.Id, depth);
            if (obj == null) continue;
            obj.PlaceFrame = p.PlaceFrame;
            Apply(obj, p);
            // Keyed by the sprite (0: a loaded movie's own timeline, whatever clip it was loaded into).
            if (Movie.WidescreenStretch.TryGetValue((LoadedRoot || Asset is not SpriteAsset ? 0 : Asset.Id, depth), out var stretch) && (stretch.Id == null || stretch.Id == p.Id))
                (obj.WidescreenScaleX, obj.WidescreenAnchor, obj.WidescreenSplit, obj.WidescreenSplitLeft, obj.WidescreenAnchorX, obj.WidescreenScaleY, obj.WidescreenSplitY, obj.WidescreenSplitAt, obj.WidescreenSplitLeftAt, obj.WidescreenScaleLeft) =
                    (stretch.ScaleX, stretch.Anchor, stretch.SplitAt != null ? stretch.Split ?? 0 : stretch.Split, stretch.SplitLeftAt != null ? stretch.SplitLeft ?? 0 : stretch.SplitLeft,
                     stretch.AnchorX, stretch.ScaleY, stretch.SplitY, stretch.SplitAt, stretch.SplitLeftAt, stretch.ScaleLeft);
            if (obj is MovieClip mc && Timeline.ClipEvents.TryGetValue((p.PlaceFrame, depth), out var ev)) mc.ClipEvents = ev;
        }

        RunFrameContent(target);
        InitPending();
    }

    static void Apply(DisplayObject c, Placement p)
    {
        if (!c.TransformedByScript)
        {
            if (p.Matrix is SKMatrix m) c.Matrix = m;
            if (p.Cx is CxForm cx) c.ColorTransform = cx;
        }
        if (p.Ratio is float r) c.Ratio = r;
        if (p.Name != null && c.PlaceFrame == p.PlaceFrame) c.Name = p.Name;
        if (p.ClipDepth is int cd) c.ClipDepth = cd;
    }

    void RunFrameContent(int frame)
    {
        var f = Timeline.Frame(frame);
        if (Inert)
        {
            // Only a frame that just stops is honoured (an animation that waits on a frame until a script starts
            // it, like a smoke puff): looping on, it would play when it shouldn't.
            if (f.Scripts.Any(IsStopOnly)) Stop();
            // (Following its loops too, a clip's frame that just goes to another, as its scripts would, for a played
            // wallpaper copy: not the movie's own timeline, whose loops wait on loading.)
            else if (FollowLoops && !LoadedRoot && f.Scripts.Select(LoopOnly).FirstOrDefault(l => l != null) is { } loop)
            {
                if (FrameOf(loop.To, 4) is int target && target != frame) GotoFrame(target);
                Playing = loop.Play;
            }
            return;
        }
        foreach (var chunk in f.Scripts) Player.QueueAction(this, chunk);
        if (f.Sounds.Count > 0)
        {
            var source = LoadedMovie;
            foreach (var s in f.Sounds) Player.Audio.Start(Movie, s, source);
        }
        if (Timeline.Stream is { } st && frame == st.StartFrame && Playing)
        {
            _stream ??= Assets.SoundAsset.FromFile(Movie, st.File, -1);
            Player.Audio.StartStream(this, _stream);
        }
    }

    /// <summary>Inert clips follow their frame scripts that just go to another frame (see RunFrameContent).</summary>
    [ThreadStatic] public static bool FollowLoops;

    static readonly Dictionary<Chunk, (bool Play, Value To)?> _loopOnly = new(ReferenceEqualityComparer.Instance);
    static readonly System.Text.RegularExpressions.Regex LoopLine = new(@"^(gotoAndPlay|gotoAndStop)\(\s*(?:""([^""]*)""|(\d+))\s*\);$");

    /// <summary>A frame script that's only "gotoAndPlay(n);" or "gotoAndStop(n);" (or a label): whether it plays, and where.</summary>
    (bool Play, Value To)? LoopOnly(Chunk chunk)
    {
        lock (_loopOnly)
        {
            if (_loopOnly.TryGetValue(chunk, out var known)) return known;
            (bool Play, Value To)? loop = null;
            int slash = chunk.Name.IndexOf('/');
            if (slash > 0)
            {
                var file = Path.Combine(Movie.Dir, chunk.Name[(slash + 1)..]);
                if (File.Exists(file))
                {
                    var lines = File.ReadAllLines(file).Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith('#')).ToList();
                    if (lines.Count == 1 && LoopLine.Match(lines[0]) is { Success: true } m)
                        loop = (m.Groups[1].Value == "gotoAndPlay", m.Groups[2].Success ? Value.From(m.Groups[2].Value) : Value.From(double.Parse(m.Groups[3].Value)));
                }
            }
            return _loopOnly[chunk] = loop;
        }
    }

    static readonly Dictionary<Chunk, bool> _stopOnly = new(ReferenceEqualityComparer.Instance);

    /// <summary>A frame script that's only "stop();" (read from its file, by the chunk's name).</summary>
    bool IsStopOnly(Chunk chunk)
    {
        lock (_stopOnly)
        {
            if (_stopOnly.TryGetValue(chunk, out var known)) return known;
            bool stop = false;
            int slash = chunk.Name.IndexOf('/');
            if (slash > 0)
            {
                var file = Path.Combine(Movie.Dir, chunk.Name[(slash + 1)..]);
                if (File.Exists(file))
                {
                    var lines = File.ReadAllLines(file).Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith('#')).ToList();
                    stop = lines is ["stop();"];
                }
            }
            return _stopOnly[chunk] = stop;
        }
    }

    void StopStream()
    {
        if (_stream != null) Player.Audio.StopStream(this);
    }

    void InitPending()
    {
        while (_pendingInit.Count > 0)
        {
            var list = _pendingInit.ToArray();
            _pendingInit.Clear();
            foreach (var mc in list)
                if (!mc.Removed) mc.Construct();
        }
    }

    /// <summary>Instantiates a library symbol as a child at the given depth.</summary>
    public DisplayObject? Create(int id, int depth, string? name = null)
    {
        var asset = Movie.Symbol(id);
        DisplayObject? obj = asset switch
        {
            SpriteAsset s => new MovieClip(Player, Movie, s.Timeline) { Inert = Inert },
            ButtonAsset b => new ButtonObject(Player, b),
            EditTextAsset e => new EditTextObject(Player, e),
            ShapeAsset or MorphAsset or TextAsset => new GraphicObject(Player, asset),
            _ => null,
        };
        if (obj == null) return null;
        obj.Asset = asset;
        obj.Parent = this;
        obj.Depth = depth;
        obj.Name = name ?? (obj is MovieClip or ButtonObject or EditTextObject ? Player.NextInstanceName() : "");
        Children[depth] = obj;
        if (obj is MovieClip mc) _pendingInit.Add(mc);
        return obj;
    }

    public void AddChild(DisplayObject obj, int depth)
    {
        if (Children.TryGetValue(depth, out var old)) RemoveChild(old);
        obj.Parent = this;
        obj.Depth = depth;
        Children[depth] = obj;
    }

    public void RemoveChild(DisplayObject c)
    {
        if (Children.TryGetValue(c.Depth, out var at) && at == c) Children.Remove(c.Depth);
        c.OnRemoved();
    }

    public override void OnRemoved()
    {
        if (Removed) return;
        Player.FireClipEvent(this, "unload");
        base.OnRemoved();
        StopStream();
        if (LoadedRoot) Player.Audio.StopSource(this);
        Player.OnClipRemoved(this);
        foreach (var c in Children.Values) c.OnRemoved();
    }

    /// <summary>duplicateMovieClip: copies the symbol, transform and clip events at a new depth.</summary>
    public MovieClip? Duplicate(string newName, int depth)
    {
        if (Parent == null || Asset == null) return null;
        var copy = Parent.Create(Asset.Id, depth, newName) as MovieClip;
        if (copy == null) return null;
        copy.Matrix = Matrix;
        copy.ColorTransform = ColorTransform;
        copy.Visible = Visible;
        copy.ClipEvents = ClipEvents;
        copy.TransformedByScript = TransformedByScript;
        Parent.InitPending();
        return copy;
    }

    public MovieClip? Attach(string exportName, string newName, int depth)
    {
        var asset = Movie.Export(exportName);
        if (asset == null) return null;
        var mc = Create(asset.Id, depth, newName) as MovieClip;
        InitPending();
        return mc;
    }

    public override void Render(Mnog.Engine.Render.RenderContext ctx, SKMatrix world, CxForm cx) => ctx.DrawContainer(Children, world, cx);

    public override bool HitTest(SKPoint p, SKMatrix world)
    {
        foreach (var c in Children.Values)
            if (c.Visible && c.ClipDepth == 0 && c.HitTest(p, world.PreConcat(c.Matrix))) return true;
        return false;
    }
}

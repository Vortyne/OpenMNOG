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
        // (A loop back to a frame that moves objects between depths (OpDoc.From): it carries on from what's shown, as
        // the next frame would, rather than starting the display list over.)
        if (rewind && target > 1 && Timeline.Frame(target).Ops.Any(o => o.From != null))
        {
            rewind = false;
            CurrentFrame = target - 1;
        }
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
            foreach (var op in Timeline.Frame(f).Ops) Step(state, op, f);
        CurrentFrame = target;

        // Objects moved to another depth (OpDoc.From): moved there, before what's left at their new depth goes.
        var moved = state.Where(kv => kv.Value.Existing is { } e && e.Depth != kv.Key && Children.GetValueOrDefault(e.Depth) == e).ToList();
        foreach (var (_, p) in moved) Children.Remove(p.Existing!.Depth);
        foreach (var (depth, p) in moved)
        {
            if (Children.TryGetValue(depth, out var occupant)) RemoveChild(occupant);
            p.Existing!.Depth = depth;
            Children[depth] = p.Existing;
        }

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

    /// <summary>One timeline op applied to the display list being worked out (GotoFrame).</summary>
    static void Step(Dictionary<int, Placement> state, OpDoc op, int f)
    {
        if (op.Remove is int rd) { state.Remove(rd); return; }
        if (op.Place is not int depth) return;
        if (op.From is int fromDepth)
        {
            state.Remove(depth);
            if (state.Remove(fromDepth, out var carried)) state[depth] = carried;
        }
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
        if (p == null) return;
        if (op.Matrix != null) p.Matrix = Geo.Matrix(op.Matrix);
        if (op.Cxform != null) p.Cx = CxForm.From(op.Cxform);
        if (op.Ratio is double r) p.Ratio = (float)r;
        if (op.Name != null) p.Name = op.Name;
        if (op.ClipDepth is int cd) p.ClipDepth = cd;
    }

    /// <summary>What a timeline places at a frame, worked out from its start.</summary>
    static Dictionary<int, Placement> StateAt(TimelineAsset timeline, int frame)
    {
        var state = new Dictionary<int, Placement>();
        for (int f = 1; f <= frame; f++)
            foreach (var op in timeline.Frame(f).Ops) Step(state, op, f);
        return state;
    }

    /// <summary>
    /// The settings' picture tweaks switched while this clip is shown: it and its children moved to their movies
    /// loaded again (map: the old to the new), so their shapes are drawn as the tweaks have them now, and what the
    /// timeline now places at the current frame shown (a colour transform changed, another symbol placed) without
    /// leaving the frame.
    /// </summary>
    public void Retweak(IReadOnlyDictionary<MovieAsset, MovieAsset> map)
    {
        foreach (var c in Children.Values.ToArray())
            if (c is MovieClip mc) mc.Retweak(map);
            else if (c is GraphicObject && c.Asset is { } a && map.TryGetValue(a.Movie, out var fresh) && fresh.Symbol(a.Id) is ShapeAsset or MorphAsset or TextAsset)
                c.Asset = fresh.Symbol(a.Id);
        if (!map.TryGetValue(Movie, out var m)) return;
        var old = Timeline;
        // (A movie loaded into a clip, a scene into the player's holder: it plays its movie's own timeline, whatever
        // sprite of another movie the clip was.)
        if (ReferenceEquals(Timeline, Movie.Timeline)) Timeline = m.Timeline;
        else if (Asset is SpriteAsset sprite && sprite.Movie == Movie)
        {
            // (Its sprite gone from the movie as loaded again (one a tweak added, now off): its parent moves it to the
            // sprite in its place (Rebase), which takes the new movie; it doesn't keep the old one.)
            if (m.Symbol(sprite.Id) is not SpriteAsset again) { Movie = m; return; }
            Asset = again;
            Timeline = again.Timeline;
        }
        else return;
        Movie = m;
        Rebase(old);
    }

    /// <summary>
    /// Its timeline changed (Retweak) from old: what the new one places at the current frame shown. A clip placed in
    /// place of another sprite is kept and moved to that sprite (as a patch's copy of a character, redrawn: where its
    /// scripts have walked it, its frame and its variables kept); anything else placed in place of another is made
    /// anew; a colour transform changed is set.
    /// </summary>
    void Rebase(TimelineAsset old)
    {
        // (Pictures it placed that are still from another load of its movie: this load's.)
        foreach (var c in Children.Values)
            if (c is GraphicObject && c.PlaceFrame != 0 && c.Asset is { } a && a.Movie != Movie &&
                Movie.Symbol(a.Id) is ShapeAsset or MorphAsset or TextAsset)
                c.Asset = Movie.Symbol(a.Id);
        var before = StateAt(old, CurrentFrame);
        foreach (var (depth, p) in StateAt(Timeline, CurrentFrame))
        {
            // (Only what the timeline put there, as it put it: not a clip a script attached.)
            if (!Children.TryGetValue(depth, out var c) || c.PlaceFrame == 0 || !before.TryGetValue(depth, out var q) || c.Asset?.Id != q.Id) continue;
            if (p.Id != q.Id && c is MovieClip clip && Movie.Symbol(p.Id) is SpriteAsset sprite)
            {
                var was = clip.Timeline;
                clip.Asset = sprite;
                clip.Timeline = sprite.Timeline;
                // (The sprite is this movie's, as loaded now: the clip's movie with it, so what it places from here on
                // is the current tweaks' (a clip left on an older load kept drawing that load's pictures).)
                clip.Movie = Movie;
                clip.CurrentFrame = Math.Min(clip.CurrentFrame, clip.FrameCount);
                clip.PlaceFrame = p.PlaceFrame;
                clip.Rebase(was);
            }
            else if (p.Id != q.Id)
            {
                RemoveChild(c);
                if (Create(p.Id, depth) is not { } obj) continue;
                obj.PlaceFrame = p.PlaceFrame;
                Apply(obj, p);
                if (obj is MovieClip made && Timeline.ClipEvents.TryGetValue((p.PlaceFrame, depth), out var ev)) made.ClipEvents = ev;
            }
            if (p.Cx != q.Cx && !c.TransformedByScript && Children.GetValueOrDefault(depth) is { } now) now.ColorTransform = p.Cx ?? CxForm.Identity;
        }
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

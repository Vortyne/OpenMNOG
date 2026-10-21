using System.Diagnostics;
using Mnog.Engine.Assets;
using Mnog.Engine.Audio;
using Mnog.Engine.Display;
using Mnog.Engine.Script;
using Mnog.Format;
using SkiaSharp;

namespace Mnog.Engine;

/// <summary>Services the window/OS layer provides to the player.</summary>
public interface IHost
{
    bool Fullscreen { get; set; }
    void SetClipboard(string text);
    void ShowCursor(bool visible);
    void SetHandCursor(bool hand);
    void OpenUrl(string url);
    void Quit();
}

/// <summary>Runs a converted game: levels, frame ticks, the action queue, input and loading.</summary>
public sealed class Player
{
    public readonly string GameDir;
    public readonly GameManifest Manifest;
    public readonly Interpreter Vm;
    public readonly AudioMixer Audio = new();
    public readonly SharedObjectStore Storage;
    public IHost? Host;
    public readonly Random Random = new();
    public readonly SortedDictionary<int, MovieClip> Levels = [];
    public int TickCount { get; private set; }
    /// <summary>The level 0 movie sets the pace (the patch swaps in a 30 fps build of it).</summary>
    public double FrameRate => SceneFrameRate() ?? (Levels.TryGetValue(0, out var l0) && l0.Movie.Doc.FrameRate > 0 ? l0.Movie.Doc.FrameRate : Manifest.FrameRate);

    readonly Dictionary<string, double?> _sceneRates = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, List<(int From, int To, bool On)>?> _sceneSmooth = new(StringComparer.OrdinalIgnoreCase);

    readonly Dictionary<string, bool> _sceneWide = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>A cutscene or minigame allowed to show its artwork past the original stage (assets/fixes "widescreen").</summary>
    bool SceneShowsWide(DisplayObject scene)
    {
        if (scene is not MovieClip { } clip) return false;
        if (!_sceneWide.TryGetValue(clip.Movie.Name, out var on))
            _sceneWide[clip.Movie.Name] = on = File.Exists(Path.Combine(clip.Movie.Dir, "widescreen.txt"));
        return on;
    }

    readonly Dictionary<string, bool> _sceneStage = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, (float? Left, float? Right, SKColor? Backdrop)> _sceneView = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>A scene's fixed view (assets/fixes "view"): how far it shows past the stage, and a colour behind it.</summary>
    (float? Left, float? Right, SKColor? Backdrop) SceneView(DisplayObject? scene)
    {
        if (scene is not MovieClip { } clip) return default;
        if (!_sceneView.TryGetValue(clip.Movie.Name, out var view))
        {
            var file = Path.Combine(clip.Movie.Dir, "view.json");
            view = default;
            if (File.Exists(file) && System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(file)) is System.Text.Json.Nodes.JsonObject o)
                view = ((float?)o["Left"]?.GetValue<double>(), (float?)o["Right"]?.GetValue<double>(),
                        o["Backdrop"]?.GetValue<string>() is { } c ? Assets.Geo.Color(c) : null);
            _sceneView[clip.Movie.Name] = view;
        }
        return view;
    }

    /// <summary>A scene kept to the original stage width, as cutscenes are (assets/fixes "stageWidth").</summary>
    bool SceneKeepsStage(DisplayObject scene)
    {
        if (scene is not MovieClip { } clip) return false;
        if (!_sceneStage.TryGetValue(clip.Movie.Name, out var on))
            _sceneStage[clip.Movie.Name] = on = File.Exists(Path.Combine(clip.Movie.Dir, "stage-width.txt"));
        return on;
    }

    /// <summary>
    /// Whether Smooth blends the current scene's timeline animation too (assets/fixes "smoothMotion"), at the
    /// movie's current frame: smooth-motion.txt may limit it to ranges of frames ("a-b") or leave some out ("!a-b").
    /// </summary>
    bool SceneSmoothMotion()
    {
        if (GetLevel(1)?.ChildByName("holder") is not MovieClip { Removed: false } holder) return false;
        var name = holder.Movie.Name;
        if (!_sceneSmooth.TryGetValue(name, out var ranges))
        {
            var file = Path.Combine(holder.Movie.Dir, "smooth-motion.txt");
            ranges = null;
            if (File.Exists(file))
            {
                ranges = [];
                foreach (var line in File.ReadAllLines(file))
                    if (line.TrimStart('!').Split('-') is [var a, var b] && int.TryParse(a, out int from) && int.TryParse(b, out int to))
                        ranges.Add((from, to, !line.StartsWith('!')));
            }
            _sceneSmooth[name] = ranges;
        }
        int frame = holder.CurrentFrame;
        // Left out by hand (blending looks wrong there), even in Full.
        if (ranges != null && ranges.Any(r => !r.On && frame >= r.From && frame <= r.To)) return false;
        if (SmoothAll) return true;
        if (ranges == null) return false;
        return !ranges.Any(r => r.On) || ranges.Any(r => r.On && frame >= r.From && frame <= r.To);
    }

    /// <summary>A frame rate set for the current scene (assets/fixes "frameRate", the movie's frame-rate.txt).</summary>
    double? SceneFrameRate()
    {
        if (GetLevel(1)?.ChildByName("holder") is not MovieClip { Removed: false } holder) return null;
        var name = holder.Movie.Name;
        if (!_sceneRates.TryGetValue(name, out var rate))
        {
            var file = Path.Combine(holder.Movie.Dir, "frame-rate.txt");
            _sceneRates[name] = rate = File.Exists(file) && double.TryParse(File.ReadAllText(file), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var r) && r > 0 ? r : null;
        }
        return rate;
    }

    /// <summary>Widescreen: fill the window's width with the scenery beyond the original stage edges.</summary>
    public bool Widescreen;

    (int W, int H) _windowSize;

    /// <summary>Horizontal range of the stage the window shows (stage coordinates); wider than the stage in widescreen.</summary>
    public float ViewLeft => Widescreen && _windowSize.W > 0 ? -Viewport(_windowSize.W, _windowSize.H).X / Viewport(_windowSize.W, _windowSize.H).Scale : 0;
    public float ViewRight => StageWidth - ViewLeft;
    public int StageWidth => Manifest.Width;
    public int StageHeight => Manifest.Height;
    readonly Stopwatch _clock = Stopwatch.StartNew();
    public long ElapsedMs => _clock.ElapsedMilliseconds;

    readonly Dictionary<string, string> _movieDirs = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, MovieAsset> _movies = new(StringComparer.OrdinalIgnoreCase);
    readonly Queue<(MovieClip Clip, Chunk Chunk)> _actions = new();
    readonly List<System.Action> _pendingLoads = [];
    readonly HashSet<MovieClip> _eventClips = [];
    int _instanceCounter;

    /// <summary>The project's version, from version_number.txt beside the game folder ("" without one).</summary>
    public readonly string ProjectVersion;

    public Player(string gameDir)
    {
        GameDir = gameDir;
        var versionFile = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(gameDir)) ?? gameDir, "version_number.txt");
        ProjectVersion = File.Exists(versionFile) ? File.ReadAllText(versionFile).Trim() : "";
        Manifest = Json.Load<GameManifest>(Path.Combine(gameDir, "game.json"));
        foreach (var d in Directory.GetDirectories(Path.Combine(gameDir, "movies")))
            _movieDirs[Path.GetFileName(d)] = d;
        Vm = new Interpreter(this);
        // (Documents\My Games\OpenMNOG, or as OpenMNOG.ini or MNOG_SAVEDIR says: see SaveLocation.)
        SaveLocation.Resolve(gameDir);
        Storage = new SharedObjectStore(SaveLocation.SharedObjects);
        Audio.LoadGroups(Path.Combine(GameDir, "sound-groups.json"));
        // The menus in the chosen language (read once a tick: the save's "language").
        Lang.Load(gameDir);
        Lang.Current = () => _language;
        Tweaks.IsOn = key => _tweaks.Contains("|" + key + "|", StringComparison.Ordinal);
        Audio.Hq = new Mnog.Engine.Audio.HqSounds(GameDir);
        Log($"save: {Path.GetFullPath(Storage.Dir)}");
        Log($"save folder from {SaveLocation.Source}");
    }

    public MovieClip Level0Root => Levels.GetValueOrDefault(0) ?? Levels.Values.First();

    public void Log(string msg)
    {
        var line = $"[{TickCount,6}] {msg}";
        Console.WriteLine(line);
        RecentLog.Add(line);
        if (RecentLog.Count > 20) RecentLog.RemoveAt(0);
    }

    /// <summary>The latest log lines (shown by the debug overlay).</summary>
    public readonly List<string> RecentLog = [];

    /// <summary>Debug overlay: diagnostic info drawn over the bottom bar (toggled with Backspace in the settings menu).</summary>
    public bool Debug;
    DebugOverlay? _overlay;

    public (int W, int H) WindowSize => _windowSize;

    /// <summary>What the mouse is over, for the debug overlay.</summary>
    public string HoverDescription =>
        _pressed != null ? $"pressing button {_pressed.SlashPath} (#{_pressed.Button.Id})"
        : _hover != null ? $"over button {_hover.SlashPath} (#{_hover.Button.Id} in {_hover.Button.Movie.Name})"
        : "no button under mouse";

    readonly HashSet<string> _logged = [];

    /// <summary>For warnings that recur every frame or scene (e.g. targets that legitimately don't exist yet).</summary>
    /// <summary>Describes a clip for diagnostics: its path, movie, frame and child names.</summary>
    static string Where(DisplayObject o) => o is MovieClip mc
        ? $"{mc.SlashPath} in {mc.Movie.Name} frame {mc.CurrentFrame}{(mc.Removed ? " (removed)" : "")}, children: {string.Join(",", mc.Children.Values.Select(c => c.Name).Where(n => n.Length > 0).Take(12))}"
        : o.SlashPath;

    public void LogOnce(string msg)
    {
        if (_logged.Add(msg)) Log(msg);
    }

    public string NextInstanceName() => "instance" + ++_instanceCounter;

    public MovieClip? GetLevel(int n) => Levels.GetValueOrDefault(n);

    public MovieAsset? GetMovie(string name)
    {
        lock (_movies) if (_movies.TryGetValue(name, out var known)) return known;
        if (!_movieDirs.TryGetValue(name, out var dir)) return null;
        var sw = Stopwatch.StartNew();
        MovieAsset? m = null;
        // Loaded ahead in the background (PreloadNeighbours): taken (waited for, if it's still loading).
        if (_preloads.TryRemove(name, out var ahead))
            using (Watchdog.Wait($"{name}, being loaded ahead in the background"))
                try { m = ahead.Result; } catch (AggregateException) { }
        if (m == null)
            using (Watchdog.Wait($"{name}, loading"))
                m = MovieAsset.Load(name, dir);
        foreach (var e in m.Errors) Log($"{name}: {e}");
        if (sw.ElapsedMilliseconds > 100) Log($"loaded {name} in {sw.ElapsedMilliseconds} ms");
        lock (_movies) return _movies[name] = m;
    }

    readonly System.Collections.Concurrent.ConcurrentDictionary<string, System.Threading.Tasks.Task<MovieAsset>> _preloads = new(StringComparer.OrdinalIgnoreCase);
    string? _preloadedFor;

    /// <summary>
    /// When the scene changes: the movies its scripts name (the scenes its exits lead to, its sky, sound and music
    /// movies) loaded ahead in the background, their pictures and sounds decoded, so walking on doesn't stall while
    /// they load (GetMovie takes them).
    /// </summary>
    void PreloadNeighbours()
    {
        if (CurrentSceneName is not { } scene || scene == _preloadedFor || !_movieDirs.TryGetValue(scene, out var dir)) return;
        _preloadedFor = scene;
        bool vector = VectorImages;
        System.Threading.Tasks.Task.Run(() =>
        {
            var scripts = Path.Combine(dir, "scripts");
            if (!Directory.Exists(scripts)) return;
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in Directory.EnumerateFiles(scripts, "*.as"))
                foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(file), @"""([^""/\\]+)\.swf""", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    names.Add(m.Groups[1].Value);
            foreach (var name in names)
            {
                bool loaded;
                lock (_movies) loaded = _movies.ContainsKey(name);
                if (loaded || !_movieDirs.TryGetValue(name, out var movieDir)) continue;
                _preloads.GetOrAdd(name, key => System.Threading.Tasks.Task.Run(() =>
                {
                    using var job = Watchdog.Job($"loading {name} ahead");
                    var movie = MovieAsset.Load(name, movieDir);
                    foreach (var symbol in movie.Symbols.Values)
                        if (symbol is ImageAsset image) image.Preload(vector);
                        else if (symbol is SoundAsset sound)
                        {
                            _ = sound.Samples;
                            // (Its high-quality replacement too, while that's on, so it doesn't load as it plays.)
                            if (Audio.UseHq) _ = Audio.Hq?.Asset(sound, ready: true)?.Samples;
                        }
                    return movie;
                }));
            }
        });
    }

    public void Start()
    {
        LoadLevel(Manifest.EntryMovie, 0);
        RunQueue();
        // The navigation tables a warp (and continuing a game with no saved sounds) works from: built in the
        // background now, as reading every script takes seconds, which done on the first warp froze the game.
        _navigationReady = System.Threading.Tasks.Task.Run(() =>
        {
            using var job = Watchdog.Job("working out the scenes' navigation");
            SceneState("");
        });
    }

    System.Threading.Tasks.Task? _navigationReady;
    readonly object _navigationLock = new();

    // ---- Frame loop ----

    /// <summary>Smooth mode: draw between game frames, interpolating scripted movement (panning) and fades.</summary>
    public bool Smooth;
    /// <summary>Smoothing "Full": every scene's timeline animation is blended too, not only the scenes marked for it.</summary>
    public bool SmoothAll;
    /// <summary>Misc > Vector sand: images with a vector replacement are drawn with it (RenderContext.DrawVectorImage).</summary>
    public bool VectorImages;

    /// <summary>An episode paused for stepping through it a frame at a time (the < > / keys); steps still to run.</summary>
    public bool FramePaused;
    int _stepFrames;

    public void Tick()
    {
        // (A new language: the menus' labels redrawn, not kept from the layer cache's pictures of them.)
        if (SaveValue("language") is { Length: > 0 } language && language != _language) { _language = language; _layerCache.Clear(); }
        // (The Ruru setting has three: Original "0", Variant 1 "1", Variant 2 "2"; their tweaks are "ruru-classic",
        // "ruru-accurate".)
        string ruru = SaveValue("ruru") switch { "1" => "ruru-classic|", "2" => "ruru-accurate|", _ => "" };
        if ("|" + string.Concat(TweakKeys.Where(k => SaveValue(k) == "1").Select(k => k + "|")) + ruru is var tweaks && tweaks != _tweaks) TweaksChanged(tweaks);
        PreloadNeighbours();
        if (_pendingMessage is { } note) { _pendingMessage = null; ShowMessage(note); }
        // Paused: the game held (nothing runs) but for the steps asked for; unpaused when the episode's left.
        if (FramePaused)
        {
            if (Cutscene == null) FramePaused = false;
            else if (_stepFrames > 0) _stepFrames--;
            else return;
        }
        TickCount++;
        if (!_volumesApplied) { _volumesApplied = true; ApplyVolumes(); }
        UpdateMusicButton();
        // The view's edges, for the game's panning (Patches.PanSettings measures its edge zone from them).
        Vm.SetVariableAt(Level0Root, "ViewLeft", Value.From((double)(Widescreen ? ViewLeft : 0)));
        Vm.SetVariableAt(Level0Root, "ViewRight", Value.From((double)(Widescreen ? ViewRight : StageWidth)));
        if (Debug) (_overlay ??= new DebugOverlay(this)).TickRan();
        if (Smooth) Snapshot();
        foreach (var clip in _eventClips.ToArray()) FireClipEvent(clip, "enterFrame");
        foreach (var level in Levels.Values.Reverse().ToArray()) level.RunFrame();
        RunQueue();
        ProcessLoads();
        TickBackdrops();
        ApplyVectorMoves();
        if (SkipLoadScreen) KeepSceneLit();
        WatchSceneChange();
        UpdateDrag();
        UpdateHover();
        if (Smooth) DecideInterpolation();
    }

    readonly Mnog.Engine.Render.LayerCache _layerCache = new();
    public Dictionary<string, int> LayerCacheStats => _layerCache.Stats;
    /// <summary>Draw unchanging artwork from images (Render.LayerCache); MNOG_NOCACHE turns it off.</summary>
    public bool UseLayerCache = !Mnog.Engine.Render.LayerCache.Disabled;

    // ---- Save slots ----

    /// <summary>The save slot menu, while open (the bottom bar's Change Save Slot button, or the title screen's).</summary>
    public SaveSlotMenu? SlotMenu;

    public void CloseSaveSlotMenu() => SlotMenu = null;

    // ---- Live backdrops (Vectorize bitmaps) ----

    readonly Dictionary<MovieAsset, List<LiveBackdrop>> _backdrops = [];

    List<LiveBackdrop> Backdrops(MovieAsset movie)
    {
        if (!_backdrops.TryGetValue(movie, out var list)) _backdrops[movie] = list = LiveBackdrop.Load(movie.Dir);
        return list;
    }

    /// <summary>The live backdrop replacing a movie's image (see LiveBackdrop), with its scene copy made; null if none.</summary>
    public LiveBackdrop? Backdrop(MovieAsset movie, int image)
    {
        var level1 = GetLevel(1);
        var b = Backdrops(movie).FirstOrDefault(x => x.Image == image &&
            x.If.All(c => (level1 != null ? Vm.GetVariableAt(level1, c.Key).ToStr(6) : "") == c.Value));
        if (b == null) return null;
        b.Root ??= b.Build(this);
        if (b.Root == null) return null;
        b.LastDrawn = TickCount;
        return b;
    }

    /// <summary>Whether a shape draws a live backdrop (a bitmap fill whose image has one).</summary>
    public bool HasBackdrop(ShapeAsset shape)
    {
        var list = Backdrops(shape.Movie);
        return list.Count > 0 && shape.Parts.Any(p => p.Fill is { Type: "bitmap", Bitmap: int id } && list.Any(b => b.Image == id));
    }

    /// <summary>Whether a movie's live backdrop asks for its text box to be frosted ("frost").</summary>
    public bool BackdropFrosted(MovieAsset movie) => Backdrops(movie).Any(b => b.Frost);

    /// <summary>An image the live backdrop of its movie supersedes (not drawn while it's shown).</summary>
    public bool BackdropReplaces(MovieAsset movie, int image) => Backdrops(movie).Any(b => b.Replaces.Contains(image));

    /// <summary>Animates the backdrops being shown; drops the scene copies of ones no longer drawn.</summary>
    // ---- Objects moved to fit vectorized art ----

    readonly Dictionary<string, Dictionary<string, List<(int From, int To, float X, float Y)>>?> _vectorMoves = new(StringComparer.OrdinalIgnoreCase);
    readonly System.Runtime.CompilerServices.ConditionalWeakTable<DisplayObject, System.Runtime.CompilerServices.StrongBox<(float X, float Y)>> _vectorMoved = new();

    /// <summary>
    /// A scene's objects that sit on a vectorized image moved to fit it, while Vectorize bitmaps is on (vector-moves.json
    /// beside the movie, in their parent's coordinates); back where they were when it's off. Each entry is a path
    /// ("Middle-Ground/#7"; "#N" in a button is its state child at depth N) and either [dx, dy] or, for a clip whose
    /// frames show different things, {"1-24": [dx, dy], "25-97": [dx, dy]} by its current frame.
    /// </summary>
    void ApplyVectorMoves()
    {
        // (The game's scene: the title screen's beach, kept loaded behind it, isn't one of these.)
        if (GetLevel(1)?.ChildByName("holder") is not MovieClip { Removed: false } scene) return;
        var name = scene.Movie.Name;
        if (!_vectorMoves.TryGetValue(name, out var moves))
        {
            moves = null;
            var file = Path.Combine(scene.Movie.Dir, "vector-moves.json");
            if (File.Exists(file) && System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(file)) is System.Text.Json.Nodes.JsonObject o)
            {
                moves = [];
                static (float, float) Pair(System.Text.Json.Nodes.JsonArray a) => ((float)a[0]!.GetValue<double>(), (float)a[1]!.GetValue<double>());
                foreach (var (k, v) in o)
                {
                    var list = new List<(int, int, float, float)>();
                    if (v is System.Text.Json.Nodes.JsonArray { Count: 2 } a) { var (x, y) = Pair(a); list.Add((0, int.MaxValue, x, y)); }
                    else if (v is System.Text.Json.Nodes.JsonObject byFrame)
                        foreach (var (range, d) in byFrame)
                            if (d is System.Text.Json.Nodes.JsonArray { Count: 2 } da && range.Split('-') is [var f, var t] && int.TryParse(f, out int from) && int.TryParse(t, out int to))
                            { var (x, y) = Pair(da); list.Add((from, to, x, y)); }
                    moves[k] = list;
                }
            }
            _vectorMoves[name] = moves;
        }
        if (moves == null) return;
        foreach (var (path, ranges) in moves)
        {
            DisplayObject? o = scene;
            foreach (var part in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                bool byDepth = part.StartsWith('#') && int.TryParse(part.AsSpan(1), out _);
                int depth = byDepth ? int.Parse(part.AsSpan(1)) : 0;
                o = o switch
                {
                    MovieClip mc => byDepth ? mc.Children.GetValueOrDefault(depth) : mc.ChildByName(part),
                    ButtonObject b when byDepth => b.StateChildren.FirstOrDefault(c => c.Depth == depth),
                    _ => null,
                };
            }
            if (o == null) continue;
            int frame = (o as MovieClip)?.CurrentFrame ?? 1;
            var want = (0f, 0f);
            if (VectorImages)
                foreach (var (from, to, x, y) in ranges)
                    if (frame >= from && frame <= to) { want = (x, y); break; }
            var applied = _vectorMoved.GetOrCreateValue(o);
            if (applied.Value == want) continue;
            o.Matrix = o.Matrix.PostConcat(SKMatrix.CreateTranslation(want.Item1 - applied.Value.X, want.Item2 - applied.Value.Y));
            o.TransformedByScript = true;
            applied.Value = want;
            _layerCache.Clear();
        }
    }

    void TickBackdrops()
    {
        foreach (var b in _backdrops.Values.SelectMany(l => l))
        {
            if (b.Root == null) continue;
            if (TickCount - b.LastDrawn > 36) { b.Root = null; continue; }
            if (!b.Still) b.Root.RunFrame();
        }
    }

    // ---- Audio settings ----

    /// <summary>The Audio settings menu (see AudioMenu), while open.</summary>
    public AudioMenu? AudioSettings;

    /// <summary>The Help (the bottom bar's ? button), while it's open.</summary>
    public HelpMenu? Help;
    /// <summary>The Help as it was last left (its page, scroll, open area and hints shown), for opening it again.</summary>
    HelpMenu? _help;

    public void CloseHelp() { Help = null; Host?.SetHandCursor(false); }

    /// <summary>A volume setting (percent, 100 unless set).</summary>
    public int Volume(string key) => int.TryParse(SaveValue(key), out int v) ? Math.Clamp(v, 0, 100) : 100;

    public void SetVolume(string key, int percent)
    {
        if (SaveData() is not { } data || Volume(key) == percent && SaveValue(key) != "") return;
        data.SetOwn(key, Value.From(percent.ToString()));
        Storage.Flush("mata-nui-online-game", Vm);
        ApplyVolumes();
    }

    /// <summary>Hands the volume settings to the mixer.</summary>
    void ApplyVolumes()
    {
        Audio.Volume = Volume("volmaster") / 100f;
        Audio.GroupVolume[Mnog.Engine.Audio.AudioMixer.Music] = Volume("volmusic") / 100f;
        Audio.GroupVolume[Mnog.Engine.Audio.AudioMixer.Sfx] = Volume("volsfx") / 100f;
        Audio.UseHq = HqAudio;
        // (High quality on: the long songs start decoding now, so they're ready by the time they're needed.)
        if (Audio.UseHq && Audio.Hq != null && GetMovie("player") is { } owner) Audio.Hq.WarmUp(owner);
    }

    /// <summary>The Audio menu's High quality option (saved as "hqaudio", off by default): the high-quality
    /// replacements in game/audio/hq play instead of the game's own sounds (from the next sound started).</summary>
    public bool HqAudio => SaveValue("hqaudio") == "1";

    public void SetHqAudio(bool on)
    {
        if (SaveData() is not { } data) return;
        data.SetOwn("hqaudio", Value.From(on ? "1" : "0"));
        Storage.Flush("mata-nui-online-game", Vm);
        ApplyVolumes();
    }

    /// <summary>A saved setting changed (the test harness's "setting KEY VALUE"), as the settings menu changes it.</summary>
    public void SetSetting(string key, string value)
    {
        if (SaveData() is not { } data) return;
        data.SetOwn(key, Value.From(value));
        Storage.Flush("mata-nui-online-game", Vm);
    }

    bool _volumesApplied;

    /// <summary>Closes the Audio menu; back reopens the settings dialog it came from (the title screen's own copy while
    /// the title is up: the overlay's would be beneath it).</summary>
    public void CloseAudioMenu(bool back)
    {
        AudioSettings = null;
        if (!back) return;
        if (TitleShowing && GetLevel(TitleLevel) is MovieClip title)
            RunScript("this.attachMovie(\"matanui_dialog_settings\", \"matanui_dialog_settings\", 1);", title);
        else if (GetLevel(10001)?.ChildByName("matanui_overlay") is MovieClip overlay)
            RunScript("this.attachMovie(\"matanui_dialog_settings\", \"matanui_dialog_settings\", 1);", overlay);
    }

    /// <summary>The prompt after the last episode (see EndingPrompt), while it's up.</summary>
    public EndingPrompt? Ending;

    public void CloseEndingPrompt() => Ending = null;

    /// <summary>
    /// The ending prompt's Chronicle button: the Chronicle opened as the backpack opens it, marked (EndingChronicle) so
    /// its close button, which does nothing once the game is won, brings the prompt back.
    /// </summary>
    public void OpenEndingChronicle()
    {
        Ending = null;
        if (GetLevel(1) is not { } root) return;
        foreach (var (name, value) in new[] { ("EndingChronicle", "1"), ("BookOpen", "1"), ("BookMovieName", "book.swf"), ("BeginLoadMovie", "1") })
            Vm.SetVariableAt(root, name, Script.Value.From(value));
    }

    /// <summary>The title screen setting (on unless turned off).</summary>
    public bool TitleEnabled => SaveValue("title") != "0";

    /// <summary>Leaves the game for the title screen: the player and everything it loaded are unloaded.</summary>
    public void ReturnToTitle() => RunScript("_level0.__matanui.toTitle();", Level0Root);

    /// <summary>The conversations' font (GillSans, the TextSub movie's glyph outlines) at a size, for the menus' longer
    /// text (the Help's pages, the settings' descriptions); the menus' font if it's missing.</summary>
    public TextFont DialogFont(float size)
    {
        if (!_dialogFonts.TryGetValue(size, out var f))
            _dialogFonts[size] = f = GetMovie("TextSub")?.Symbol(5) is Mnog.Engine.Assets.FontAsset glyphs ? new TextFont(glyphs, size)
                : new TextFont(new SKFont(MenuTypeface, size * 0.86f));
        return f;
    }
    readonly Dictionary<float, TextFont> _dialogFonts = [];

    SKTypeface? _menuTypeface;

    /// <summary>The settings menus' font (TradeMarker Light, copied into the game's fonts folder by the converter).</summary>
    public SKTypeface MenuTypeface => _menuTypeface ??=
        (Directory.Exists(Path.Combine(GameDir, "fonts")) ? Directory.GetFiles(Path.Combine(GameDir, "fonts"), "*.ttf").FirstOrDefault() : null) is { } file
            ? SKTypeface.FromFile(file) ?? SKTypeface.Default : SKTypeface.Default;

    AsObject? SaveData() =>
        Storage.GetLocal("mata-nui-online-game", Vm).TryGetOwn("data", out var d) ? d.Object as AsObject : null;

    string _language = "eng";

    /// <summary>The settings' Tweaks that are on, as "|colors|matoran|" (read once a tick from the save; Tweaks.IsOn is
    /// asked from the background loads too).</summary>
    volatile string _tweaks = "|";
    static readonly string[] TweakKeys = ["matoran", "colors", "clones", "pakari"];

    void TweaksChanged(string tweaks)
    {
        string was = _tweaks;
        _tweaks = tweaks;
        bool Switched(string key) => was.Contains("|" + key + "|") != tweaks.Contains("|" + key + "|");
        // Tohunga or Matoran: the text already loaded (the clips' variables) said the new way too.
        bool matoran = tweaks.Contains("|matoran|");
        if (Switched("matoran"))
        {
            void Retext(MovieClip clip)
            {
                clip.MapStrings(s => Tweaks.Swap(s, matoran));
                foreach (var c in clip.Children.Values)
                    if (c is MovieClip mc) Retext(mc);
            }
            foreach (var level in Levels.Values) Retext(level);
        }
        if (!Tweaks.Pictures.Any(Switched)) return;
        // A picture tweak switched: the movies it changes that are on screen loaded again and what's shown moved to
        // them (MovieClip.Retweak); the others loaded again as they're next needed.
        var shown = new HashSet<MovieAsset>();
        void Shown(MovieClip clip)
        {
            shown.Add(clip.Movie);
            foreach (var c in clip.Children.Values)
                if (c is MovieClip mc) Shown(mc);
        }
        foreach (var level in Levels.Values) Shown(level);
        var map = new Dictionary<MovieAsset, MovieAsset>();
        lock (_movies)
            foreach (var (name, movie) in _movies.ToList())
            {
                if (!Tweaks.HasPictures(movie.Dir)) continue;
                if (!shown.Contains(movie)) { _movies.Remove(name); continue; }
                using (Watchdog.Wait($"{name}, loading again for the tweaks"))
                    map[movie] = _movies[name] = MovieAsset.Load(name, movie.Dir);
                foreach (var e in map[movie].Errors) Log($"{name}: {e}");
            }
        foreach (var level in Levels.Values) level.Retweak(map);
        _preloads.Clear();
        _preloadedFor = null;
        _layerCache.Clear();
    }

    string SaveValue(string key) => SaveData() is { } data && data.TryGetOwn(key, out var v) && !v.IsUndefined ? v.ToStr(6) : "";

    /// <summary>The slot the game saves to (the patched game's "slot" setting).</summary>
    public int CurrentSaveSlot => int.TryParse(SaveValue("slot"), out int n) && n is >= 1 and <= SaveSlotMenu.Slots ? n : 1;

    /// <summary>A slot's saved state (slot 1 is the original save's "state").</summary>
    public string SaveSlotState(int n) => SaveValue(n == 1 ? "state" : "state" + n);

    /// <summary>Where a slot's picture is kept: beside the save data.</summary>
    public string SlotPictureFile(int n) => Path.Combine(Path.GetDirectoryName(Path.GetFullPath(Storage.Dir))!, "pictures", $"slot{n}.png");

    /// <summary>Makes a slot the one the game saves to; in game, loads it (an empty one starts a new game).</summary>
    public void UseSaveSlot(int n, bool load)
    {
        Ending = null;
        RunScript($"_level0.__matanui.useSlot(\"{n}\");" +
                  (load ? "_level0.__matanui.reload();" : "if (_level20000.refreshSlot != undefined) { _level20000.refreshSlot(); }"), Level0Root);
    }

    /// <summary>Starts the slot being played over (the original's Start New Game): its save is cleared and the game reloaded.</summary>
    public void RestartSaveSlot(int n)
    {
        Ending = null;
        Storage.BackupNow("mata-nui-online-game");
        RunScript("_level0.__matanui.save.state.clear(); _level0.__matanui.reload();", Level0Root);
        try { File.Delete(SlotPictureFile(n)); } catch (IOException) { }
    }

    public void DeleteSaveSlot(int n, bool fromTitle)
    {
        Storage.BackupNow("mata-nui-online-game");
        RunScript($"_level0.__matanui.deleteSlot(\"{n}\");" +
                  (fromTitle ? "if (_level20000.refreshSlot != undefined) { _level20000.refreshSlot(); }" : ""), Level0Root);
        try { File.Delete(SlotPictureFile(n)); } catch (IOException) { }
    }

    long _pictureAt;
    bool _picturing;

    /// <summary>A small picture of the view for the slot just saved (the game in play, without menus or panels).</summary>
    void TakeSlotPicture(SKCanvas canvas, int width, int height, float t)
    {
        // Not while something covers the scene; tried again on the next frame.
        if (TitleShowing || _transitionImage != null || SlotMenu != null || Warp != null || Help != null ||
            GetLevel(1) is not { Movie.Name: "player", CurrentFrame: >= 19 and <= 23 }) return;
        _pictureAt = 0;
        int w = 256, h = Math.Max(1, (int)Math.Round(256.0 * height / width));
        var info = new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        if (surface == null) return;
        bool cached = UseLayerCache;
        _picturing = true;
        UseLayerCache = false;
        try { Render(surface.Canvas, w, h, t); }
        finally
        {
            _picturing = false;
            UseLayerCache = cached;
            _windowSize = (width, height);
        }
        try
        {
            var file = SlotPictureFile(CurrentSaveSlot);
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            // The scene only, above the bottom bar (from y 425 of the stage).
            var (scale, _, oy) = Viewport(w, h);
            using var image = surface.Snapshot(SKRectI.Create(0, 0, w, Math.Clamp((int)Math.Round(oy + 425 * scale), 1, h)));
            using var png = image.Encode(SKEncodedImageFormat.Png, 90);
            File.WriteAllBytes(file, png.ToArray());
        }
        catch (IOException e) { Log($"slot picture not saved: {e.Message}"); }
    }

    /// <summary>The "Loading screen: Off" setting (see Patches.LoadScreenSkip).</summary>
    public bool SkipLoadScreen;

    // ---- Scene crossfade (loading screen off) ----
    // When the player starts loading the next scene (its frame 24), the view is captured; it stays over the
    // view while the scene loads and then fades out over TransitionMs, so the old scene crossfades into the new.

    const double TransitionMs = 200;
    bool _captureRequested, _capturing;
    SKImage? _transitionImage;
    long _transitionFadeStart = -1, _transitionHoldStart = -1;
    int _lastPlayerFrame;

    /// <summary>Called each tick: notices the player starting a scene change.</summary>
    void WatchSceneChange()
    {
        int frame = GetLevel(1) is { Movie.Name: "player" } p ? p.CurrentFrame : 0;
        if (SkipLoadScreen && frame == 24 && _lastPlayerFrame != 24) { _captureRequested = true; }
        // The game saves its state as it enters frame 19 (SaveState); a picture of the slot is taken a moment later.
        if (frame == 19 && _lastPlayerFrame != 19) _pictureAt = TickCount + 4;
        _lastPlayerFrame = frame;
    }

    void CaptureTransition(SKCanvas canvas, int width, int height, float t)
    {
        _captureRequested = false;
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = canvas.Context is GRRecordingContext gpu ? SKSurface.Create(gpu, true, info) : SKSurface.Create(info);
        if (surface == null) return;
        _capturing = true;
        try { Render(surface.Canvas, width, height, t); }
        finally { _capturing = false; }
        _transitionImage?.Dispose();
        _transitionImage = surface.Snapshot();
        _transitionFadeStart = -1;
        _transitionHoldStart = -1;
    }

    void DrawTransition(SKCanvas canvas, int width, int height)
    {
        if (_transitionImage == null) return;
        // Held while the player's loading frames run, then faded out.
        bool loading = GetLevel(1) is { Movie.Name: "player", CurrentFrame: >= 24 and <= 43 };
        // The new scene's vector sand is still being drawn (in the background): the old view stays a little longer,
        // so the scene fades in with it (half a second at most).
        if (!loading && Volatile.Read(ref Mnog.Engine.Render.RenderContext.VectorImagesPending) > 0)
        {
            if (_transitionHoldStart < 0) _transitionHoldStart = ElapsedMs;
            if (ElapsedMs - _transitionHoldStart < 500) loading = true;
        }
        double alpha = 1;
        if (!loading)
        {
            if (_transitionFadeStart < 0) { _transitionFadeStart = ElapsedMs; }
            alpha = 1 - (ElapsedMs - _transitionFadeStart) / TransitionMs;
        }
        if (alpha <= 0)
        {
            _transitionImage.Dispose();
            _transitionImage = null;
            return;
        }
        canvas.Save();
        canvas.ResetMatrix();
        using var paint = new SKPaint { Color = SKColors.White.WithAlpha((byte)Math.Round(alpha * 255)) };
        canvas.DrawImage(_transitionImage, new SKRect(0, 0, width, height), new SKSamplingOptions(SKFilterMode.Linear), paint);
        canvas.Restore();
    }

    /// <summary>
    /// While the player's loading sequence runs (its frames 25-43), the scene and backpack keep their colours
    /// (the timeline would fade them to black) and the globe (depth 11) stays hidden.
    /// </summary>
    void KeepSceneLit()
    {
        if (GetLevel(1) is not { Movie.Name: "player" } player || player.CurrentFrame is < 25 or > 43) return;
        foreach (var name in new[] { "holder", "Backpack" })
            if (player.ChildByName(name) is { } o) o.ColorTransform = o.PrevColor = CxForm.Identity;
        if (player.Children.TryGetValue(11, out var globe)) globe.Visible = false;
    }

    IEnumerable<DisplayObject> AllObjects()
    {
        var stack = new Stack<DisplayObject>(Levels.Values);
        while (stack.Count > 0)
        {
            var o = stack.Pop();
            yield return o;
            if (o is MovieClip mc)
                foreach (var c in mc.Children.Values) stack.Push(c);
        }
    }

    void Snapshot()
    {
        foreach (var o in AllObjects())
        {
            o.PrevMatrix = o.Matrix;
            o.PrevColor = o.ColorTransform;
            // (Whether a clip had nothing on its frame: one then moved by a script and appearing there isn't slid in.)
            o.PrevEmpty = o is MovieClip { Children.Count: 0 };
            if (o is EditTextObject et) et.PrevText = et.Text;
            o.Snapshotted = true;
        }
    }

    /// <summary>
    /// Picks what to interpolate after a game frame: movement made by scripts (such as panning) when it is
    /// a plain slide, and pure color changes (fades). Timeline animation is never interpolated, so it keeps
    /// its original frame rate; neither is the dragged cursor clip, which follows the mouse directly.
    /// </summary>
    void DecideInterpolation()
    {
        const float maxSlide = 150;
        // A scene slowed down (SceneFrameRate) or marked for it (SceneSmoothMotion) has its timeline animation blended
        // between frames too (positions, scaling and colour), so it stays smooth.
        bool slowed = SceneFrameRate() != null || SceneSmoothMotion();
        foreach (var o in AllObjects())
        {
            var a = o.PrevMatrix;
            var b = o.Matrix;
            bool moved = a.TransX != b.TransX || a.TransY != b.TransY;
            bool sameShape = a.ScaleX == b.ScaleX && a.ScaleY == b.ScaleY && a.SkewX == b.SkewX && a.SkewY == b.SkewY;
            // A turn: the new transform is the old one rotated by a small angle (scripts setting _rotation).
            float angle = 0;
            if (!sameShape)
            {
                angle = (float)(Math.Atan2(b.SkewY, b.ScaleX) - Math.Atan2(a.SkewY, a.ScaleX));
                if (angle > Math.PI) angle -= (float)(2 * Math.PI);
                if (angle < -Math.PI) angle += (float)(2 * Math.PI);
                var turned = SKMatrix.CreateRotation(angle).PreConcat(new SKMatrix(a.ScaleX, a.SkewX, 0, a.SkewY, a.ScaleY, 0, 0, 0, 1));
                const float eps = 1e-3f;
                bool isTurn = Math.Abs(angle) <= 0.35f &&
                    Math.Abs(turned.ScaleX - b.ScaleX) < eps && Math.Abs(turned.SkewX - b.SkewX) < eps &&
                    Math.Abs(turned.SkewY - b.SkewY) < eps && Math.Abs(turned.ScaleY - b.ScaleY) < eps;
                if (!isTurn) angle = 0;
            }
            bool turnedOnly = angle != 0;
            // A flip (mirrored, or turned more than 60 degrees from one frame to the next: Episode 3-3's speed-line
            // backgrounds, flipped about every frame to make the lines flicker) is a cut: blending it would squash the
            // shape through nothing on the way. Shown at once, in Full too.
            float detA = a.ScaleX * a.ScaleY - a.SkewX * a.SkewY, detB = b.ScaleX * b.ScaleY - b.SkewX * b.SkewY;
            float lenA = MathF.Sqrt(a.ScaleX * a.ScaleX + a.SkewY * a.SkewY), lenB = MathF.Sqrt(b.ScaleX * b.ScaleX + b.SkewY * b.SkewY);
            bool flipped = detA * detB < 0 || lenA > 0 && lenB > 0 && (a.ScaleX * b.ScaleX + a.SkewY * b.SkewY) / (lenA * lenB) < 0.5f;
            // (Not one that drew nothing until now: a clip moved into place and started as it appears, like the Ta-Koro
            // guard's turn, would otherwise be seen sliding in from wherever it waited.)
            o.LerpMatrix = o.Snapshotted && !o.PrevEmpty && (moved || turnedOnly) && (sameShape || turnedOnly) && o.TransformedByScript && _drag?.Clip != o &&
                           Math.Abs(b.TransX - a.TransX) <= maxSlide && Math.Abs(b.TransY - a.TransY) <= maxSlide;
            o.LerpAngle = o.LerpMatrix ? angle : 0;
            o.LerpColor = o.Snapshotted && !o.ColorTransform.Equals(o.PrevColor) && !moved && sameShape;
            o.LerpFull = false;
            // A script resizing something a little each frame (the Ta-Koro cable car's rope stretching as the view
            // pans): blended too, so it keeps pace with what's sliding smoothly around it.
            if (!o.LerpMatrix && !o.PrevEmpty && !sameShape && !turnedOnly && !flipped && o.Snapshotted && o.TransformedByScript && _drag?.Clip != o &&
                // (Skews compared loosely: a mirrored one (the XVIII.3-S bridge's right rope) carries a rounding
                // leftover there. A change counts as little if small either way: a shrinking rope's steps grow
                // relative to its size.)
                Math.Abs(a.SkewX - b.SkewX) < 1e-4f && Math.Abs(a.SkewY - b.SkewY) < 1e-4f && a.ScaleX != 0 && a.ScaleY != 0 &&
                (Math.Abs(b.ScaleX / a.ScaleX - 1) < 0.15f || Math.Abs(b.ScaleX - a.ScaleX) < 0.1f) &&
                (Math.Abs(b.ScaleY / a.ScaleY - 1) < 0.15f || Math.Abs(b.ScaleY - a.ScaleY) < 0.1f) &&
                Math.Abs(b.TransX - a.TransX) <= maxSlide && Math.Abs(b.TransY - a.TransY) <= maxSlide)
            {
                o.LerpMatrix = true;
                o.LerpFull = true;
            }
            // A looping animation jumping back to its start (moving or growing the other way to the frame before)
            // is a reset: shown at once, not slid back.
            static float N(float v, float eps) => Math.Abs(v) < eps ? 0 : v;
            var step = (X: N(b.TransX - a.TransX, 0.05f), Y: N(b.TransY - a.TransY, 0.05f), S: N(b.ScaleX - a.ScaleX + b.ScaleY - a.ScaleY, 0.005f));
            var last = o.LastStep;
            bool reset = step.X * last.X + step.Y * last.Y < 0 || step.S * last.S < 0;
            // After a reset the motion starts over: the next step is compared with nothing.
            o.LastStep = reset ? default : step;
            // (Not one that drew nothing until now, as above: the Ta-Koro guards' turn, moved to the walking guard as
            // it appears when clicked, would slide in from where it waited.)
            if (slowed && o.Snapshotted && !o.PrevEmpty && !o.LerpMatrix && !reset && !flipped && _drag?.Clip != o && (moved || !sameShape) &&
                Math.Abs(b.TransX - a.TransX) <= maxSlide && Math.Abs(b.TransY - a.TransY) <= maxSlide)
            {
                o.LerpMatrix = true;
                o.LerpFull = !sameShape;
                o.LerpAngle = 0;
            }
            if (slowed && o.Snapshotted && !o.ColorTransform.Equals(o.PrevColor)) o.LerpColor = true;
            o.Snapshotted = false;
        }
    }

    public void QueueAction(MovieClip clip, Chunk chunk) => _actions.Enqueue((clip, chunk));

    void RunQueue()
    {
        int guard = 0;
        while (_actions.Count > 0 && guard++ < 100_000)
        {
            var (clip, chunk) = _actions.Dequeue();
            if (!clip.Removed) Vm.Run(chunk, clip);
        }
    }

    public void FireClipEvent(MovieClip clip, string name)
    {
        if (clip.ClipEvents == null || clip.Inert) return;
        if (name == "load") _eventClips.Add(clip);
        foreach (var (on, _, script) in clip.ClipEvents)
            if (script != null && on.Contains(name)) QueueAction(clip, script);
    }

    public void OnClipRemoved(MovieClip clip)
    {
        _eventClips.Remove(clip);
        if (_drag?.Clip == clip) _drag = null;
    }

    // ---- Loading ----

    static string MovieName(string url)
    {
        var u = url;
        int q = u.IndexOfAny(['?', '#']);
        if (q >= 0) u = u[..q];
        return Path.GetFileNameWithoutExtension(u.Replace('\\', '/').Split('/')[^1]);
    }

    void LoadLevel(string name, int level)
    {
        if (Environment.GetEnvironmentVariable("MNOG_LOADDBG") != null) Console.WriteLine($"[load] level {level} <- {name}");
        var movie = GetMovie(name);
        if (movie == null) { Log($"loadMovie: movie not found: {name}"); return; }
        if (level == 0)
        {
            // Replacing level 0 unloads everything.
            foreach (var l in Levels.Values) l.OnRemoved();
            Levels.Clear();
        }
        else if (Levels.TryGetValue(level, out var old)) old.OnRemoved();
        var clip = new MovieClip(this, movie, movie.Timeline) { Level = level, Name = $"_level{level}", Depth = level, LoadedRoot = true };
        Levels[level] = clip;
        clip.Construct();
    }

    /// <summary>loadMovie(url, target): target is "_levelN", a path, or a clip. Applied after the current scripts.</summary>
    public void LoadMovie(string url, Value target, DisplayObject relativeTo)
    {
        string name = MovieName(url);
        string ext = Path.GetExtension(url.Split('?')[0]).ToLowerInvariant();
        if (target.IsString && target.RawString.StartsWith("_level", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(target.RawString.AsSpan(6), out int lv))
        {
            _pendingLoads.Add(() => LoadLevel(name, lv));
            return;
        }
        var t = Vm.ResolveTargetFrom(relativeTo, target);
        if (Environment.GetEnvironmentVariable("MNOG_LOADDBG") != null) Console.WriteLine($"[load] {target.ToStr(6)} from {Where(relativeTo)} -> {(t as MovieClip)?.SlashPath} <- {url}");
        if (t is not MovieClip clip)
        {
            LogOnce($"loadMovie: target not found: {target.ToStr(6)} (from {Where(relativeTo)})");
            return;
        }
        _pendingLoads.Add(() =>
        {
            if (clip.Removed) return;
            if (ext is ".jpg" or ".jpeg" or ".png" or ".gif") { LoadImageInto(clip, url); return; }
            var movie = GetMovie(name);
            if (movie == null) { Log($"loadMovie: movie not found: {url}"); return; }
            ReplaceClip(clip, new MovieClip(this, movie, movie.Timeline));
        });
    }

    void ReplaceClip(MovieClip old, MovieClip clip)
    {
        clip.LoadedRoot = true;
        clip.Name = old.Name;
        clip.Matrix = old.Matrix;
        clip.ColorTransform = old.ColorTransform;
        clip.Visible = old.Visible;
        clip.PlaceFrame = old.PlaceFrame;
        clip.TransformedByScript = old.TransformedByScript;
        clip.Asset = old.Asset;
        // Smooth mode: a fade running across the load continues from where the old clip was.
        clip.PrevMatrix = old.PrevMatrix;
        clip.PrevColor = old.PrevColor;
        clip.Snapshotted = old.Snapshotted;
        if (old.Level is int lv)
        {
            clip.Level = lv;
            clip.Depth = lv;
            old.OnRemoved();
            Levels[lv] = clip;
        }
        else if (old.Parent is { } parent)
        {
            parent.RemoveChild(old);
            parent.AddChild(clip, old.Depth);
        }
        clip.Construct();
    }

    void LoadImageInto(MovieClip clip, string url)
    {
        var path = FilePath(url);
        if (path == null) { Log($"loadMovie: image not found: {url}"); return; }
        var image = SKImage.FromEncodedData(path);
        if (image == null) return;
        var holder = new MovieClip(this, clip.Movie, new TimelineAsset(clip.Movie, new TimelineDoc { FrameCount = 1 }, ""));
        ReplaceClip(clip, holder);
        holder.AddChild(new BitmapObject(this, image), 1);
    }

    /// <summary>Finds a data file (case-insensitively) under files/.</summary>
    public string? FilePath(string url)
    {
        var rel = url.Split('?')[0].Replace('\\', '/');
        var dir = Path.Combine(GameDir, "files");
        foreach (var part in rel.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            var match = Directory.EnumerateFileSystemEntries(dir)
                .FirstOrDefault(e => Path.GetFileName(e).Equals(part, StringComparison.OrdinalIgnoreCase));
            if (match == null) return null;
            dir = match;
        }
        return File.Exists(dir) ? dir : null;
    }

    public void LoadVariables(string url, Value target, DisplayObject relativeTo)
    {
        var path = FilePath(url);
        // French ("_fre"): a file not translated yet is read in English.
        if (path == null && System.Text.RegularExpressions.Regex.IsMatch(url, @"_fre\.txt$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            path = FilePath(System.Text.RegularExpressions.Regex.Replace(url, @"_fre\.txt$", "_eng.txt", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
        if (path == null) { Log($"loadVariables: file not found: {url}"); return; }
        DisplayObject? t;
        if (target.IsString && target.RawString.StartsWith("_level", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(target.RawString.AsSpan(6), out int lv))
            t = GetLevel(lv);
        else t = Vm.ResolveTargetFrom(relativeTo, target);
        if (t == null) { LogOnce($"loadVariables: target not found: {target.ToStr(6)} (from {Where(relativeTo)})"); return; }
        // A button's variables live in the clip it's on (its scripts run there); the backpack loads its item
        // names this way, targeting its slot buttons.
        if (t is ButtonObject { Parent: MovieClip buttonHost }) t = buttonHost;
        _pendingLoads.Add(() =>
        {
            var text = File.ReadAllText(path, Ansi.Encoding);
            foreach (var pair in text.Split('&'))
            {
                int eq = pair.IndexOf('=');
                if (eq <= 0) continue;
                var k = Unescape(pair[..eq]).Trim('\r', '\n');
                var v = Tweaks.Matoran(Unescape(pair[(eq + 1)..]));
                t.Set(k, v, Vm);
            }
            if (t is MovieClip mc) FireClipEvent(mc, "data");
        });
    }

    static string Unescape(string s)
    {
        s = s.Replace('+', ' ');
        // Flash decodes %XX as bytes in the system code page.
        var bytes = new List<byte>();
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '%' && i + 2 < s.Length && Uri.IsHexDigit(s[i + 1]) && Uri.IsHexDigit(s[i + 2]))
            {
                bytes.Add(Convert.ToByte(s.Substring(i + 1, 2), 16));
                i += 2;
            }
            else bytes.AddRange(Ansi.Encoding.GetBytes(s[i].ToString()));
        }
        return Ansi.Encoding.GetString(bytes.ToArray());
    }

    void ProcessLoads()
    {
        while (_pendingLoads.Count > 0)
        {
            var loads = _pendingLoads.ToArray();
            _pendingLoads.Clear();
            foreach (var l in loads) l();
            RunQueue();
        }
    }

    public void GetUrl(string url, string window, DisplayObject relativeTo)
    {
        if (url.StartsWith("FSCommand:", StringComparison.OrdinalIgnoreCase))
        {
            FsCommand(url[10..], window);
            return;
        }
        if (window.StartsWith("_level", StringComparison.OrdinalIgnoreCase))
        {
            LoadMovie(url, window, relativeTo);
            return;
        }
        if (url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)) return;
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
        {
            Host?.OpenUrl(url);
            return;
        }
        if (FilePath(url) is { } local && Uri.TryCreate(Path.GetFullPath(local), UriKind.Absolute, out var uri)) Host?.OpenUrl(uri.AbsoluteUri);
        else Log($"getURL: ignored {url}");
    }

    public void FsCommand(string cmd, string arg)
    {
        switch (cmd.ToLowerInvariant())
        {
            case "quit": Host?.Quit(); break;
            case "fullscreen": if (Host != null) Host.Fullscreen = arg.Equals("true", StringComparison.OrdinalIgnoreCase); break;
            case "widescreen": Widescreen = arg is "1" or "true"; break;
            case "wallpaper": if (int.TryParse(arg, out int wallSlot)) Wallpaper = new WallpaperMenu(this, wallSlot); break;
            case "music": if (int.TryParse(arg, out int musicSlot) && MusicMenu.Tracks(this, musicSlot).Count > 0) Music = new MusicMenu(this, musicSlot); break;
            case "vectorsand": if (VectorImages != (arg is "1" or "true")) { VectorImages = arg is "1" or "true"; _layerCache.Clear(); } break;
            case "textboxalpha": if (LowTextboxTransparency != (arg == "0")) { LowTextboxTransparency = arg == "0"; _layerCache.Clear(); } break;
            // Smoothing: 0 off, 1 partial (scripted motion, and the scenes marked for it), 2 full (all animation).
            case "smooth": Smooth = arg is "1" or "2" or "true"; SmoothAll = arg == "2"; break;
            case "debug": Debug = arg is "1" or "true"; break;
            case "warp": Warp = new WarpMenu(this); break;
            case "gameend": Ending = new EndingPrompt(this); break;
            case "audiomenu": AudioSettings = new AudioMenu(this); break;
            // (The settings dialog, if open, closed: these menus replace it.)
            case "help": CloseSettingsDialog(); Help = _help ??= new HelpMenu(this); Help.Reopen(); break;
            case "saveslots": CloseSettingsDialog(); SlotMenu = new SaveSlotMenu(this, arg == "title" ? SaveSlotMenu.Mode.Title : SaveSlotMenu.Mode.Game); break;
            // The bar's (and the title screen's) Settings button: the settings dialog opened, or closed if it (or one of
            // its pages) is open.
            case "settings":
                if (!CloseSettingsDialog())
                {
                    if (TitleShowing && GetLevel(TitleLevel) is { } title) RunScript("this.attachMovie(\"matanui_dialog_settings\", \"matanui_dialog_settings\", 1);", title);
                    else if (GetLevel(10001)?.ChildByName("matanui_overlay") is MovieClip bar) RunScript("this.attachMovie(\"matanui_dialog_settings\", \"matanui_dialog_settings\", 1);", bar);
                }
                break;
            case "scenesounds": FillSceneSounds(); break;
            case "message": ShowMessage(arg); break;
            case "loadscreen": SkipLoadScreen = arg == "0"; break;
            case "fadeout":
                // fadeout LEVEL: fade the sounds started by movies on that level (the title screen's music and waves).
                if (int.TryParse(arg, out int fadeLevel) && GetLevel(fadeLevel) is { } fading)
                    Audio.FadeOut(src => src is MovieClip mc && mc.Root == fading, 1.4);
                break;
            default: Log($"fscommand {cmd} {arg}"); break;
        }
    }

    // ---- Mouse, buttons and dragging ----

    public SKPoint MouseStage { get; private set; }
    public bool MouseDown { get; private set; }
    ButtonObject? _hover, _pressed;

    sealed record DragState(MovieClip Clip, bool LockCenter, SKRect? Bounds, SKPoint Offset);
    DragState? _drag;

    public void StartDrag(MovieClip clip, bool lockCenter, SKRect? bounds)
    {
        var offset = SKPoint.Empty;
        if (!lockCenter && clip.Parent != null && clip.Parent.WorldMatrix.TryInvert(out var inv))
        {
            var m = inv.MapPoint(MouseStage);
            offset = new SKPoint((float)clip.X - m.X, (float)clip.Y - m.Y);
        }
        _drag = new DragState(clip, lockCenter, bounds, offset);
        UpdateDrag();
    }

    public void StopDrag()
    {
        if (_drag != null) _dropTargets[_drag.Clip] = FindDropTarget(_drag.Clip);
        _drag = null;
    }

    readonly Dictionary<MovieClip, string> _dropTargets = [];

    public string DropTarget(MovieClip clip) =>
        _drag?.Clip == clip ? FindDropTarget(clip) : _dropTargets.GetValueOrDefault(clip, "");

    string FindDropTarget(MovieClip dragged)
    {
        foreach (var level in Levels.Values.Reverse())
            if (FindClipAt(level, MouseStage, dragged) is { } hit) return hit.SlashPath;
        return "";
    }

    MovieClip? FindClipAt(MovieClip clip, SKPoint p, MovieClip exclude)
    {
        if (clip == exclude || !clip.Visible) return null;
        foreach (var c in clip.Children.Values.Reverse())
        {
            if (c == exclude || !c.Visible) continue;
            if (c is MovieClip mc && FindClipAt(mc, p, exclude) is { } inner) return inner;
            if (c is not MovieClip && c.HitTest(p, c.WorldMatrix)) return clip.Parent == null ? null : clip;
        }
        return null;
    }

    void UpdateDrag()
    {
        if (_drag is not { } d || d.Clip.Removed) return;
        if (d.Clip.Parent is not { } parent || !parent.WorldMatrix.TryInvert(out var inv))
        {
            // Dragging a level root: stage coordinates.
            inv = SKMatrix.Identity;
        }
        var p = inv.MapPoint(MouseStage);
        float x = p.X + d.Offset.X, y = p.Y + d.Offset.Y;
        if (d.Bounds is SKRect b)
        {
            x = Math.Clamp(x, Math.Min(b.Left, b.Right), Math.Max(b.Left, b.Right));
            y = Math.Clamp(y, Math.Min(b.Top, b.Bottom), Math.Max(b.Top, b.Bottom));
        }
        d.Clip.X = x;
        d.Clip.Y = y;
    }

    /// <summary>Top-most button under a stage point.</summary>
    /// <summary>Whether the mouse is over a button (with the widescreen edge extension of ButtonAt).</summary>
    bool MouseOver(ButtonObject b)
    {
        var p = MouseStage;
        if (b.HitTest(p, b.WorldMatrix)) return true;
        foreach (var edge in EdgePoints(p))
            if (b.HitTest(edge, b.WorldMatrix)) return true;
        return false;
    }

    /// <summary>Widescreen, mouse beyond the stage's side: points just inside that edge, nearest first.</summary>
    IEnumerable<SKPoint> EdgePoints(SKPoint p)
    {
        if (!Widescreen || (p.X >= 0 && p.X < StageWidth)) yield break;
        foreach (var inset in new[] { 0.5f, 2, 4, 8, 12 })
            yield return new SKPoint(p.X < 0 ? inset : StageWidth - inset, p.Y);
    }

    ButtonObject? ButtonAt(SKPoint p)
    {
        // Under one of the engine's menus, nothing (the game ticking on mustn't hover, or point at, what's beneath).
        if (EngineMenuOpen) return null;
        // Over an open dialog (settings and its pages, the quit dialog...): only its own buttons. Its box covers what's
        // beneath, in the scene, which mustn't be pointed at or clicked through it.
        if (OpenDialog() is { } dialog && dialog.WorldMatrix.MapRect(dialog.LocalBounds).Contains(p))
            return ButtonIn(dialog, p, dialog.WorldMatrix);
        // On the bottom bar (from stage y 425), only it and what's drawn over it: what's hidden under it (dialog
        // answers still scrolling up from below) can't be clicked through it.
        int barLevel = Levels.FirstOrDefault(kv => kv.Value.Movie.Name == "overlay").Key;
        bool onBar = p.Y >= 425 && Levels.ContainsKey(barLevel) && Levels[barLevel].Movie.Name == "overlay";
        foreach (var (n, level) in Levels.Reverse())
            if ((!onBar || n >= barLevel) && ButtonIn(level, p, level.Matrix) is { } b) return b;
        // Widescreen: beyond the stage's sides, buttons at its edge (such as "turn around") reach out to the window
        // edge, as the original's would have reached the window edge.
        foreach (var edge in EdgePoints(p))
            foreach (var (n, level) in Levels.Reverse())
                if ((!onBar || n >= barLevel) && ButtonIn(level, edge, level.Matrix) is { } b) return b;
        return null;
    }

    /// <summary>The dialog's red "continue" arrow (TextSub's button 13), when it's showing.</summary>
    /// <summary>One of the engine's own menus is open (the Help, save slots, audio settings, warp, wallpaper, music, the
    /// ending): it takes the mouse, and sets the cursor.</summary>
    bool EngineMenuOpen => SlotMenu != null || Ending != null || AudioSettings != null || Help != null || Warp != null ||
        Wallpaper != null || Music != null;

    /// <summary>Whether a menu covers the game: the engine's own (the Help, save slots, audio settings, warp, wallpaper,
    /// the ending) or one of the overlay's dialogs (settings and its pages, timestamp, progress, quit).</summary>
    public bool MenuOpen => SlotMenu != null || Ending != null || AudioSettings != null || Help != null || Warp != null ||
        Wallpaper != null || Music != null || GetLevel(10001)?.ChildByName("matanui_overlay") is MovieClip overlay &&
        overlay.Children.TryGetValue(16384 + 1, out var dialog) && dialog is MovieClip { Removed: false };

    /// <summary>
    /// Left Shift: turns around, as clicking the scene's turn-around button does (the edge areas that show the
    /// turn cursor), when the scene has one showing and no menu or dialog is open.
    /// </summary>
    public void TurnAround()
    {
        if (MenuOpen || TitleShowing) return;
        if (GetLevel(1)?.ChildByName("holder") is not MovieClip holder) return;
        static ButtonObject? Find(MovieClip clip)
        {
            if (!clip.Visible || clip.Removed) return null;
            foreach (var c in clip.Children.Values)
            {
                if (!c.Visible) continue;
                if (c is ButtonObject { Asset: ButtonAsset { TurnsAround: true } } b) return b;
                if (c is MovieClip mc && Find(mc) is { } found) return found;
            }
            return null;
        }
        if (Find(holder) is not { } button) return;
        Log($"turn around: {button.SlashPath}");
        ButtonEvent(button, "release");
        RunQueue();
    }

    ButtonObject? DialogArrow()
    {
        static ButtonObject? Find(MovieClip clip)
        {
            if (!clip.Visible || clip.Removed) return null;
            foreach (var c in clip.Children.Values)
            {
                if (!c.Visible) continue;
                if (c is ButtonObject { Asset.Id: 13 } b && b.Asset.Movie.Name.Equals("TextSub", StringComparison.OrdinalIgnoreCase)) return b;
                if (c is MovieClip mc && Find(mc) is { } found) return found;
            }
            return null;
        }
        // Cutscenes have their own red arrow (each its own button, its own script), most in the same corner as the
        // dialog's, some further along the bottom beside the cutscene's own text (6-6, 6-7, 7-2): a small visible
        // button in the cutscene's lower right, the one nearest the corner.
        static void InCorner(MovieClip clip, ref ButtonObject? best, ref float bestX)
        {
            if (!clip.Visible || clip.Removed) return;
            foreach (var c in clip.Children.Values)
            {
                if (!c.Visible) continue;
                if (c is ButtonObject b && b.WorldMatrix.MapRect(b.LocalBounds) is { Width: > 0 and < 80, Height: > 0 and < 80 } r &&
                    r.MidX is > 440 and < 790 && r.MidY is > 360 and < 440 && r.MidX > bestX) { best = b; bestX = r.MidX; }
                if (c is MovieClip mc) InCorner(mc, ref best, ref bestX);
            }
        }
        if (GetLevel(1) is not { } level1) return null;
        if (Find(level1) is { } dialog) return dialog;
        if (level1.ChildByName("holder") is not MovieClip holder || !holder.Movie.Name.StartsWith("Episode_", StringComparison.OrdinalIgnoreCase)) return null;
        ButtonObject? arrow = null;
        float x = 0;
        InCorner(holder, ref arrow, ref x);
        return arrow;
    }

    static ButtonObject? ButtonIn(MovieClip clip, SKPoint p, SKMatrix world)
    {
        if (!clip.Visible) return null;
        foreach (var c in clip.Children.Values.Reverse())
        {
            if (!c.Visible) continue;
            var w = world.PreConcat(c.Matrix);
            // The title screen's backdrop scene is only scenery.
            if (c is MovieClip { Name: "holder", Parent: MovieClip { Level: TitleLevel } }) continue;
            if (c is MovieClip mc && ButtonIn(mc, p, w) is { } b) return b;
            if (c is ButtonObject { Disabled: false } btn && btn.HitTest(p, w)) return btn;
        }
        return null;
    }

    public void OnMouseMove(SKPoint stage)
    {
        MouseStage = stage;
        // (The engine's menus: the pointer cursor over what can be clicked in them, nothing beneath them counting.)
        if (SlotMenu != null) { SlotMenu.MouseMove(stage); Host?.SetHandCursor(SlotMenu.OverClickable(stage)); return; }
        if (Ending != null) { Ending.MouseMove(stage); Host?.SetHandCursor(Ending.OverClickable(stage)); return; }
        if (Wallpaper != null) { Wallpaper.MouseMove(stage); Host?.SetHandCursor(Wallpaper.OverClickable(stage)); return; }
        if (Music != null) { Music.MouseMove(stage); Host?.SetHandCursor(Music.OverClickable(stage)); return; }
        if (AudioSettings != null) { AudioSettings.MouseMove(stage); Host?.SetHandCursor(AudioSettings.OverClickable(stage)); return; }
        if (Help != null) { Help.MouseMove(stage); return; }
        if (Warp != null) { Warp.MouseMove(stage); Host?.SetHandCursor(Warp.OverClickable(stage)); return; }
        UpdateDrag();
        UpdateHover();
        Host?.SetHandCursor(_hover != null || OverSkip);
        foreach (var clip in _eventClips.ToArray()) FireClipEvent(clip, "mouseMove");
        RunQueue();
    }

    readonly List<ButtonObject> _removedHover = [];

    void UpdateHover()
    {
        if (_removedHover.Count > 0)
        {
            // The button under the mouse was removed (e.g. clicking it loaded a new scene). In Flash the new
            // content arrived after a delay, so the mouse had usually left the button and its rollOut ran; run it
            // now so what rollOver set up, like the game's custom cursor, is undone. Not when only its own clip went
            // to another frame (its clip still there): Flash runs no rollOut then, and XV.3A's switches, which go to
            // their "on" animation as they're clicked, were sent back "off" by it.
            var removed = _removedHover.ToArray();
            _removedHover.Clear();
            foreach (var b in removed)
                if (b.Parent is MovieClip { Removed: true } host)
                    foreach (var a in b.Button.Actions)
                        if (a.Script != null && a.On.Contains("rollOut")) Vm.Run(a.Script, host);
            RunQueue();
        }
        if (_pressed != null)
        {
            bool over = !_pressed.Removed && MouseOver(_pressed);
            var wanted = over ? ButtonState.Down : ButtonState.Over;
            if (_pressed.State != wanted)
            {
                ButtonEvent(_pressed, over ? "dragOver" : "dragOut");
                _pressed.SetState(wanted);
            }
            return;
        }
        var hit = ButtonAt(MouseStage);
        if (hit == _hover) return;
        if (_hover != null && !_hover.Removed)
        {
            _hover.SetState(ButtonState.Up);
            ButtonEvent(_hover, "rollOut");
        }
        _hover = hit;
        if (_hover != null)
        {
            _hover.SetState(ButtonState.Over);
            ButtonEvent(_hover, "rollOver");
        }
        // (An engine menu sets the cursor itself, for what's clickable in it.)
        if (!EngineMenuOpen) Host?.SetHandCursor(_hover != null || OverSkip);
    }

    public void OnMouseDown()
    {
        if (SlotMenu != null) { SlotMenu.MouseDown(MouseStage); return; }
        if (Ending != null) { Ending.MouseDown(MouseStage); return; }
        if (Wallpaper != null) { Wallpaper.MouseDown(MouseStage); return; }
        if (Music != null) { Music.MouseDown(MouseStage); return; }
        if (AudioSettings != null) { AudioSettings.MouseDown(MouseStage); return; }
        if (Help != null) { Help.MouseDown(MouseStage); return; }
        if (Warp != null) { Warp.MouseDown(MouseStage); return; }
        if (OverSkip) { SkipMovie(); return; }
        MouseDown = true;
        UpdateHover();
        if (_hover != null)
        {
            _pressed = _hover;
            _pressed.SetState(ButtonState.Down);
            ButtonEvent(_pressed, "press");
        }
        else FocusTextAt(MouseStage);
        foreach (var clip in _eventClips.ToArray()) FireClipEvent(clip, "mouseDown");
        RunQueue();
    }

    public void OnMouseUp()
    {
        AudioSettings?.MouseUp();
        Help?.MouseUp();
        if (Warp != null || SlotMenu != null || Ending != null || AudioSettings != null || Help != null || Wallpaper != null || Music != null) return;
        MouseDown = false;
        if (_pressed != null)
        {
            var b = _pressed;
            _pressed = null;
            bool over = !b.Removed && MouseOver(b);
            if (over)
            {
                b.SetState(ButtonState.Over);
                ButtonEvent(b, "release");
            }
            else
            {
                b.SetState(ButtonState.Up);
                ButtonEvent(b, "releaseOutside");
                _hover = null;
            }
        }
        foreach (var clip in _eventClips.ToArray()) FireClipEvent(clip, "mouseUp");
        RunQueue();
        ProcessLoads();
        UpdateHover();
    }

    void ButtonEvent(ButtonObject b, string ev)
    {
        if (b.Button.Sounds.TryGetValue(ev, out var snd)) Audio.Start(b.Button.Movie, snd, (b.Parent as MovieClip)?.LoadedMovie);
        var host = b.Parent;
        if (host == null || host.Removed) return;
        foreach (var a in b.Button.Actions)
            if (a.Script != null && a.On.Contains(ev)) Vm.Run(a.Script, host);
        RunQueue();
    }

    public void OnButtonRemoved(ButtonObject b)
    {
        if (_hover == b) { _hover = null; _removedHover.Add(b); Host?.SetHandCursor(false); }
        if (_pressed == b) _pressed = null;
    }

    // ---- Keyboard and text input ----

    public readonly HashSet<int> KeysDown = [];
    public int LastKeyCode, LastKeyAscii;
    public EditTextObject? Focus { get; private set; }

    public void SetFocus(Value target)
    {
        Focus = null;
        if (target.IsNullish) return;
        if (Vm.ResolveTargetFrom(Level0Root, target) is EditTextObject t) Focus = t;
        else
        {
            // Selection.setFocus("input") names a text field by its variable.
            var name = target.ToStr(6);
            foreach (var level in Levels.Values)
                if (FindField(level, name) is { } f) { Focus = f; return; }
        }
    }

    static EditTextObject? FindField(MovieClip clip, string variable)
    {
        foreach (var c in clip.Children.Values)
        {
            if (c is EditTextObject e && (e.Name.Equals(variable, StringComparison.OrdinalIgnoreCase) ||
                                          string.Equals(e.Field.Doc.Variable, variable, StringComparison.OrdinalIgnoreCase)))
                return e;
            if (c is MovieClip mc && FindField(mc, variable) is { } inner) return inner;
        }
        return null;
    }

    void FocusTextAt(SKPoint p)
    {
        Focus = null;
        foreach (var level in Levels.Values.Reverse())
            if (TextAt(level, p) is { } t) { Focus = t; return; }
    }

    static EditTextObject? TextAt(MovieClip clip, SKPoint p)
    {
        if (!clip.Visible) return null;
        foreach (var c in clip.Children.Values.Reverse())
        {
            if (!c.Visible) continue;
            if (c is MovieClip mc && TextAt(mc, p) is { } t) return t;
            if (c is EditTextObject e && e.Editable && e.HitTest(p, e.WorldMatrix)) return e;
        }
        return null;
    }

    public void OnTextRemoved(EditTextObject t)
    {
        if (Focus == t) Focus = null;
    }

    /// <summary>Flash key codes for button keyPress events (special keys use small numbers).</summary>
    public void OnKeyDown(int flashKeyCode, int buttonKeyCode)
    {
        if (SlotMenu != null) { SlotMenu.KeyDown(flashKeyCode); return; }
        if (Ending != null) return;
        if (Wallpaper != null) { Wallpaper.KeyDown(flashKeyCode); return; }
        if (Music != null) { Music.KeyDown(flashKeyCode); return; }
        if (AudioSettings != null) { AudioSettings.KeyDown(flashKeyCode); return; }
        if (Help != null) { Help.KeyDown(flashKeyCode); return; }
        if (Warp != null) { Warp.KeyDown(flashKeyCode); return; }
        if (flashKeyCode == 27) { ToggleQuitDialog(); return; }
        // V toggles Vectorize bitmaps for this session (for comparing with the original art; the setting isn't changed).
        if (flashKeyCode == 86 && !(Focus?.Editable ?? false))
        {
            FsCommand("vectorsand", VectorImages ? "0" : "1");
            ShowMessage(Lang.T(VectorImages ? "Vectorize bitmaps: on" : "Vectorize bitmaps: off"), bar: true);
            return;
        }
        // Episodes frame by frame: < (,) pauses and steps back a frame, > (.) pauses and steps on one, / or Pause|Break
        // pauses and plays on.
        if (flashKeyCode is 188 or 190 or 191 or 19 && !(Focus?.Editable ?? false) && Cutscene is { } episode)
        {
            if (flashKeyCode is 191 or 19)
            {
                if (FramePaused) { FramePaused = false; ShowMessage(Lang.T("Playing on")); }
                else
                {
                    Audio.StopSource(episode);
                    FramePaused = true;
                    ShowMessage(Lang.F("Paused at frame {0}. < > to step, / to play.", episode.CurrentFrame));
                }
                return;
            }
            if (!FramePaused) Audio.StopSource(episode);
            FramePaused = true;
            if (flashKeyCode == 190) _stepFrames++;
            else if (episode.CurrentFrame > 1)
            {
                // Back: the episode's timeline only (its clips' own animations aren't rewound).
                episode.GotoFrame(episode.CurrentFrame - 1);
                episode.Playing = true;
            }
            ShowMessage(Lang.F("Paused at frame {0}. < > to step, / to play.", episode.CurrentFrame + _stepFrames));
            return;
        }
        // Space continues a dialog, as clicking its red arrow does (when one is showing).
        if (flashKeyCode == 32 && !(Focus?.Editable ?? false) && DialogArrow() is { } arrow)
        {
            ButtonEvent(arrow, "release");
            RunQueue();
            return;
        }
        KeysDown.Add(flashKeyCode);
        LastKeyCode = flashKeyCode;
        if (Focus != null && Focus.Editable)
        {
            if (flashKeyCode == 8 && Focus.Text.Length > 0) Focus.Text = Focus.Text[..^1];
            else if (flashKeyCode == 13 && Focus.Field.Doc.Multiline == true) Focus.Text += "\r";
        }
        if (buttonKeyCode != 0)
            foreach (var level in Levels.Values.ToArray()) KeyPress(level, buttonKeyCode);
        foreach (var clip in _eventClips.ToArray()) FireClipEvent(clip, "keyDown");
        RunQueue();
    }

    public void OnKeyUp(int flashKeyCode)
    {
        KeysDown.Remove(flashKeyCode);
        foreach (var clip in _eventClips.ToArray()) FireClipEvent(clip, "keyUp");
        RunQueue();
    }

    public void OnTextInput(string text)
    {
        if (SlotMenu != null || Ending != null || AudioSettings != null || Help != null) return;
        if (Warp != null) { Warp.Text(text); return; }
        if (text.Length > 0) LastKeyAscii = text[0];
        if (Focus != null && Focus.Editable) Focus.Text += text;
        foreach (var ch in text)
            foreach (var level in Levels.Values.ToArray()) KeyPress(level, ch);
        RunQueue();
    }

    public void OnMouseWheel(float dy)
    {
        if (Help != null) Help.Wheel(dy);
        else Warp?.Wheel(dy);
    }

    void KeyPress(MovieClip clip, int code)
    {
        foreach (var c in clip.Children.Values.ToArray())
        {
            if (c is MovieClip mc) KeyPress(mc, code);
            else if (c is ButtonObject b)
                foreach (var a in b.Button.Actions)
                    if (a.Key == code && a.Script != null) Vm.Run(a.Script, clip);
        }
    }

    /// <summary>The title screen's level (movies/title, loaded by the settings patch's start()).</summary>
    public const int TitleLevel = 20000;

    /// <summary>Whether the title screen is up (its level keeps an empty clip once it's gone).</summary>
    bool TitleShowing => GetLevel(TitleLevel) is { Removed: false, Movie.Name: "title" };

    // ---- Startup ----
    // The window stays black until the first view (the title screen, or the first scene with the title off) is
    // ready: its vector sand drawn (in the background) and a couple of frames rendered. Meanwhile the game is
    // frozen (SdlHost doesn't tick it or play its sound), so the title's music and fade start with the picture.

    bool _started;
    int _startupFrames;
    long _startupContentAt = -1;
    const double StartupWaitMs = 1500;

    /// <summary>The first view is ready and showing (the host shows its window then).</summary>
    public bool Started => _started;

    /// <summary>Shows the game from the start (headless test runs, which only draw when asked).</summary>
    public void SkipStartupHold() => _started = true;

    /// <summary>The first view is up but not yet shown: the host doesn't tick the game or play its sound.</summary>
    public bool StartupFrozen => !_started && _startupContentAt >= 0;

    bool StartupContent => TitleShowing || GetLevel(1) is { Movie.Name: "player" } p && p.ChildByName("holder") is MovieClip { Removed: false, FrameCount: > 1 };

    /// <summary>Called after each render while starting: true while the view should stay hidden.</summary>
    bool StartupHidden()
    {
        if (_started) return false;
        if (_startupContentAt < 0)
        {
            if (!StartupContent) return true;
            _startupContentAt = ElapsedMs;
        }
        _startupFrames++;
        if (_startupFrames >= 3 && Volatile.Read(ref Mnog.Engine.Render.RenderContext.VectorImagesPending) == 0 ||
            ElapsedMs - _startupContentAt > StartupWaitMs)
        {
            _started = true;
            Log($"startup: shown after {ElapsedMs - _startupContentAt:0} ms of preparing");
            return false;
        }
        return true;
    }

    /// <summary>
    /// stopAllSounds, except the title screen's sounds: the game's own background sounds call it as they start,
    /// which would cut the title music off instead of letting it fade out as the game begins.
    /// </summary>
    public void StopAllSounds() => Audio.StopAll(src => !(src is MovieClip mc && mc.Root.Level == TitleLevel));

    // ---- Escape: quit dialog ----

    /// <summary>
    /// Escape closes the open overlay dialog (settings, timestamp, progress, new game, quit), or else opens the
    /// "Quit game?" dialog the settings patch adds. Dialogs are attached at depth 1 of the overlay clip.
    /// </summary>
    void ToggleQuitDialog()
    {
        // On the title screen (above the overlay) its own copies of the dialogs are used: Esc closes the new
        // game confirmation if it's up, otherwise opens or closes "Quit game?".
        // While a new game is fading in from the title, the game's own dialog is used (it's soon uncovered).
        if (TitleShowing && GetLevel(TitleLevel) is { } title && Vm.GetVariableAt(title, "leaving").ToStr(6) != "1")
        {
            foreach (var name in new[] { "confirm", "quit" })
                if (title.ChildByName(name) is MovieClip shown)
                {
                    Interpreter.RemoveMovieClip(shown);
                    return;
                }
            RunScript("this.attachMovie(\"title_dialog_quit\", \"quit\", 2);", title);
            return;
        }
        if (GetLevel(10001)?.ChildByName("matanui_overlay") is not MovieClip overlay) return;
        // attachMovie depths are stored offset by 16384, as in Flash.
        if (overlay.Children.TryGetValue(16384 + 1, out var open) && open is MovieClip dialog)
        {
            Interpreter.RemoveMovieClip(dialog);
            return;
        }
        RunScript("this.attachMovie(\"patch_dialog_quit\", \"matanui_dialog_quit\", 1);", overlay);
    }

    /// <summary>Runs a line of ActionScript on a clip (for engine features that open the game's own UI).</summary>
    void RunScript(string source, MovieClip clip)
    {
        var chunk = Compiler.Compile(Mnog.Script.Parser.Parse("#version 6\n" + source + "\n"), "engine");
        Vm.Run(chunk, clip);
        RunQueue();
    }

    // ---- Warp menu (debug) ----

    /// <summary>The warp menu, while open (= in the settings dialog).</summary>
    public WarpMenu? Warp;

    public void CloseWarpMenu() => Warp = null;

    /// <summary>Name of the movie in the scene holder (the current scene or cutscene).</summary>
    public string? CurrentSceneName => (GetLevel(1)?.ChildByName("holder") as MovieClip)?.Movie.Name;

    static readonly System.Text.RegularExpressions.Regex SceneName = new(@"^[IVXL]+\.\d", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    static bool IsMovie(string name) => name.StartsWith("Episode", StringComparison.OrdinalIgnoreCase);
    static bool IsMinigame(string name) => name.StartsWith("Game_", StringComparison.OrdinalIgnoreCase);

    public static string WarpKind(string name) => IsMovie(name) ? "cutscene" : IsMinigame(name) ? "minigame" : "scene";

    /// <summary>Scenes (area numeral first), then cutscenes and minigames.</summary>
    public IEnumerable<string> WarpTargets()
    {
        static int Roman(string r)
        {
            int total = 0, prev = 0;
            foreach (var c in r.ToUpperInvariant().Reverse())
            {
                int v = c switch { 'I' => 1, 'V' => 5, 'X' => 10, 'L' => 50, _ => 0 };
                total += v < prev ? -v : v;
                prev = Math.Max(prev, v);
            }
            return total;
        }
        static string Key(string n)
        {
            // Pad numbers so "IV.10-N" sorts after "IV.9-N", and areas sort by their numeral.
            string area = n.Split('.')[0];
            string rest = System.Text.RegularExpressions.Regex.Replace(n[area.Length..], @"\d+", m => m.Value.PadLeft(4, '0'));
            int group = IsMovie(n) ? 1 : IsMinigame(n) ? 2 : 0;
            return $"{group}{(group == 0 ? Roman(area).ToString("D3") : "")}{(group == 0 ? rest : System.Text.RegularExpressions.Regex.Replace(n, @"\d+", m => m.Value.PadLeft(4, '0')))}";
        }
        var dir = Path.Combine(GameDir, "movies");
        return Directory.EnumerateDirectories(dir).Select(Path.GetFileName).OfType<string>()
            .Where(n => SceneName.IsMatch(n) || IsMovie(n) || IsMinigame(n))
            .OrderBy(Key, StringComparer.OrdinalIgnoreCase);
    }

    Dictionary<string, List<(string Name, string Value)>>? _navigation;

    /// <summary>
    /// The variables a navigation script sets to reach a scene (scroll position, sounds, music...): the
    /// assignments in the same block as its MovieName = "NAME.swf". The block setting the most variables wins;
    /// on a tie, ordinary scenes' buttons are preferred over cutscenes', whose blocks may carry story flags.
    /// </summary>
    public List<(string Name, string Value)> NavigationVars(string scene)
    {
        lock (_navigationLock)
        if (_navigation == null)
        {
            _navigation = new(StringComparer.OrdinalIgnoreCase);
            var assign = new System.Text.RegularExpressions.Regex(@"^(\s*)(\w+) = ""([^""]*)"";\s*$");
            static string MovieOf(string script) => Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(script))!);
            // Only moves made in scenes, cutscenes and minigames: the player's start-up block for the first
            // scene resets the whole game state.
            var files = Directory.EnumerateFiles(Path.Combine(GameDir, "movies"), "*.as", SearchOption.AllDirectories)
                .Where(f => MovieOf(f) is var m && (SceneName.IsMatch(m) || IsMovie(m) || IsMinigame(m)))
                .OrderBy(f => SceneName.IsMatch(Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(f))!)) && Path.GetFileName(f).StartsWith("button") ? 0 : 1);
            foreach (var file in files)
            {
                var lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    var m = assign.Match(lines[i]);
                    if (!m.Success || !m.Groups[2].Value.Equals("MovieName", StringComparison.OrdinalIgnoreCase)) continue;
                    var target = Path.GetFileNameWithoutExtension(m.Groups[3].Value);
                    if (target.Length == 0) continue;
                    string indent = m.Groups[1].Value;
                    int start = i, end = i;
                    // The enclosing block: up to the brace opening it and the one closing it (or an "} else {").
                    while (start > 0 && !(lines[start - 1].TrimEnd().EndsWith('{') && lines[start - 1].Length - lines[start - 1].TrimStart().Length < indent.Length)) start--;
                    while (end + 1 < lines.Length && !(lines[end + 1].TrimStart().StartsWith('}') && lines[end + 1].Length - lines[end + 1].TrimStart().Length < indent.Length)) end++;
                    var vars = new List<(string, string)>();
                    for (int k = start; k <= end; k++)
                        if (assign.Match(lines[k]) is { Success: true } a && a.Groups[1].Value == indent)
                            vars.Add((a.Groups[2].Value, a.Groups[3].Value));
                    // The fullest block wins (some only set a variable or two before loading).
                    if (!_navigation.TryGetValue(target, out var known) || vars.Count > known.Count) _navigation[target] = vars;
                    _routes.Add((Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(file))!), target, vars));
                }
            }
        }
        if (_navigation.TryGetValue(scene, out var found)) return found;
        return [("MovieName", scene + ".swf"), ("BeginLoadMovie", "1")];
    }

    /// <summary>Every navigation block: the movie whose script it is, the scene it goes to, what it sets.</summary>
    readonly List<(string From, string To, List<(string Name, string Value)> Vars)> _routes = [];

    /// <summary>
    /// What stays in effect from scene to scene unless a move changes it: the panning mode, and the music,
    /// ambience, sound effects and their volume (with the flag that has the player reload each).
    /// </summary>
    static readonly (string Name, string? ChangeFlag)[] Carried =
    [
        ("ShiftOff", null), ("HorizontalShift", null), ("VerticalShift", null),
        ("Music_Movie", "Music_ChangeMusic"), ("Sound_Background", "Sound_ChangeBackgroundMovie"),
        ("Sound_SFXMovie", "Sound_ChangeSFXMovie"), ("Sound_BG_Volume", "Sound_ChangeBackgroundVolume"),
    ];

    Dictionary<string, Dictionary<string, string>>? _sceneState;

    /// <summary>A scene's area numeral ("IV" for IV.5-S); empty for cutscenes and minigames.</summary>
    static string Area(string movie) => SceneName.IsMatch(movie) ? movie.Split('.')[0].ToUpperInvariant() : "";

    /// <summary>
    /// A scene's panning and sounds, which the game sets only when moving changes them. A warp skips the
    /// moves that led there, so they're worked out from all routes: a value a route into the scene sets, or
    /// else the one in effect in the scene it comes from (followed back until a route sets it).
    /// </summary>
    Dictionary<string, string> SceneState(string scene)
    {
        lock (_navigationLock)
        if (_sceneState == null)
        {
            NavigationVars(scene);
            var state = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> Of(string movie) => state.TryGetValue(movie, out var s) ? s : state[movie] = new(StringComparer.OrdinalIgnoreCase);
            foreach (var (name, _) in Carried)
            {
                // Values routes set directly (the most common one when they differ), then inherited ones,
                // spreading along the routes that leave the value alone until nothing more is learned.
                foreach (var g in _routes.SelectMany(r => r.Vars.Where(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Select(v => (r.To, v.Value)))
                             .GroupBy(x => x.To, StringComparer.OrdinalIgnoreCase))
                    Of(g.Key)[name] = g.GroupBy(x => x.Value).OrderByDescending(v => v.Count()).First().Key;
                // One step per pass (nearer scenes decide first), from the same area's scenes where it has any
                // (a scene in another region leading in may have other sounds); other areas only when that
                // learns nothing more.
                var leaving = _routes.Where(r => !r.Vars.Any(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    .GroupBy(r => r.To, StringComparer.OrdinalIgnoreCase).ToList();
                for (bool anyArea = false; ;)
                {
                    var learned = new List<(string Scene, string Value)>();
                    foreach (var g in leaving)
                    {
                        if (Of(g.Key).ContainsKey(name)) continue;
                        var near = g.Where(r => Area(r.From) == Area(g.Key)).ToList();
                        var from = near.Count > 0 && !anyArea ? near : g.ToList();
                        var inherited = from.Select(r => Of(r.From).GetValueOrDefault(name)).OfType<string>().ToList();
                        if (inherited.Count > 0)
                            learned.Add((g.Key, inherited.GroupBy(v => v).OrderByDescending(v => v.Count()).First().Key));
                    }
                    if (learned.Count == 0)
                    {
                        if (anyArea) break;
                        anyArea = true;
                        continue;
                    }
                    foreach (var (s, v) in learned) Of(s)[name] = v;
                    anyArea = false;
                }
            }
            _sceneState = state;
        }
        return _sceneState.TryGetValue(scene, out var found) ? found : [];
    }

    /// <summary>
    /// A continued game whose save holds no music, ambience or sound effects (see Patches.RestoredSounds): the
    /// empty ones are set to the scene's, as SceneState works them out (silence when nothing is known).
    /// </summary>
    void FillSceneSounds()
    {
        if (GetLevel(1) is not { } level1) return;
        string scene = Path.GetFileNameWithoutExtension(Vm.GetVariableAt(level1, "MovieName").ToStr(6));
        var state = SceneState(scene);
        var filled = new List<string>();
        foreach (var name in new[] { "Music_Movie", "Sound_Background", "Sound_SFXMovie" })
        {
            if (Vm.GetVariableAt(level1, name).ToStr(6).Length > 0) continue;
            var value = state.GetValueOrDefault(name) ?? "blank.swf";
            Vm.SetVariableAt(level1, name, Value.From(value));
            filled.Add($"{name}={value}");
        }
        Log($"continue: no saved sounds for {scene}, filled in {string.Join(", ", filled)}");
    }

    /// <summary>Goes to a scene the way the game's navigation does.</summary>
    public void WarpTo(string scene)
    {
        var level1 = GetLevel(1);
        if (level1 == null) return;
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var vars = NavigationVars(scene);
        // Defaults for what the navigation block may leave out; a warp always loads.
        Vm.SetVariableAt(level1, "BookOpen", Value.From("0"));
        foreach (var (name, value) in vars)
            Vm.SetVariableAt(level1, name, Value.From(name.Equals("MovieName", StringComparison.OrdinalIgnoreCase) ? scene + ".swf" : value));
        // The scene's panning and sounds, as they'd be on arriving there in play (what the navigation block
        // leaves out is worked out from the other routes); the sounds are reloaded.
        var state = SceneState(scene);
        foreach (var (name, flag) in Carried)
        {
            string? value = vars.LastOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Value ?? state.GetValueOrDefault(name);
            // A route that sets which way the scene pans means it pans: panning isn't off there.
            if (name == "ShiftOff" && !vars.Any(v => v.Name.Equals("ShiftOff", StringComparison.OrdinalIgnoreCase)) &&
                vars.Any(v => v.Name.Equals("HorizontalShift", StringComparison.OrdinalIgnoreCase) || v.Name.Equals("VerticalShift", StringComparison.OrdinalIgnoreCase)))
                value = "0";
            // Movies no route leads to (the episodes, replayed from the book) play with the sound holders
            // emptied, as the book does; nothing from the previous scene keeps playing.
            if (value == null && name is "Music_Movie" or "Sound_Background" or "Sound_SFXMovie") value = "blank.swf";
            if (value == null) continue;
            Vm.SetVariableAt(level1, name, Value.From(value));
            if (flag != null) Vm.SetVariableAt(level1, flag, Value.From("1"));
        }
        Vm.SetVariableAt(level1, "BeginLoadMovie", Value.From("1"));
        Log($"warp: {scene} in {timer.ElapsedMilliseconds} ms ({string.Join(", ", vars.Select(v => v.Name + "=" + v.Value))}; {string.Join(", ", state.Select(v => v.Key + "=" + v.Value))})");
    }

    // ---- Skip button (cutscenes) ----

    /// <summary>The Skip button's place on the bottom bar's empty left end (stage pixels).</summary>
    static readonly SKRect SkipRect = new(10, 436, 74, 462);

    MovieClip? Cutscene => GetLevel(1)?.ChildByName("holder") is MovieClip { Removed: false } h && IsMovie(h.Movie.Name) ? h : null;

    bool SkipVisible => Warp == null && Cutscene != null && SkipTarget(Cutscene) != null;

    bool OverSkip => SkipVisible && SkipRect.Contains(MouseStage);

    readonly Dictionary<Assets.MovieAsset, int[]> _exits = [];

    /// <summary>
    /// Frames of a cutscene's timeline that hand control back to the game (their script sets BeginLoadMovie or
    /// BookMovieName). A movie can hold several segments, each ending in its own exit.
    /// </summary>
    int[] Exits(Assets.MovieAsset movie)
    {
        if (_exits.TryGetValue(movie, out var exits)) return exits;
        var list = new List<int>();
        var frames = movie.Doc.Timeline.Frames;
        for (int i = 0; i < frames.Count; i++)
            foreach (var script in frames[i].Scripts ?? [])
            {
                var path = Path.Combine(GameDir, "movies", movie.Name, script);
                if (File.Exists(path) && File.ReadAllText(path) is var text &&
                    (text.Contains("BeginLoadMovie", StringComparison.OrdinalIgnoreCase) || text.Contains("BookMovieName", StringComparison.OrdinalIgnoreCase)))
                    list.Add(i + 1);
            }
        return _exits[movie] = [.. list];
    }

    int? SkipTarget(MovieClip clip)
    {
        foreach (var f in Exits(clip.Movie))
            if (f >= clip.CurrentFrame) return f;
        return null;
    }

    /// <summary>Jumps the cutscene to its next exit, which goes on to wherever the movie leads.</summary>
    public void SkipMovie()
    {
        if (Cutscene is not { } clip) return;
        int start = clip.CurrentFrame;
        // A cutscene can branch on the way (Episode_3-1 picks its destination's arrival at frame 77): the
        // branching frames before the exit are run first, so the skip ends at the exit of the branch taken.
        for (int guard = 0; guard < 40; guard++)
        {
            if (SkipTarget(clip) is not int exit) return;
            int? branch = Branches(clip.Movie).Where(b => b >= clip.CurrentFrame && b < exit).Cast<int?>().FirstOrDefault();
            Audio.StopSource(clip);
            if (branch is int b)
            {
                clip.GotoFrame(b);
                clip.Play();
                RunQueue();
                // Not branched (its condition didn't apply): on from the frame after it.
                if (clip.CurrentFrame == b && b + 1 <= clip.FrameCount) { clip.GotoFrame(b + 1); clip.Play(); RunQueue(); }
                // Pulled back to it, or before it: a loop waiting for something to finish (Episode_6-7's frames 1115-1116
                // wait for an animation to set ExitLoop; 2247-2270 loop until frame 2606 is loaded): on to where the
                // loop leads once done (the furthest jump on from any of its frames).
                if (clip.CurrentFrame <= b && Enumerable.Range(clip.CurrentFrame, b - clip.CurrentFrame + 1)
                        .Select(f => ForwardJump(clip.Movie, f)).Where(t => t > b).Max() is int done)
                {
                    clip.GotoFrame(done);
                    clip.Play();
                    RunQueue();
                }
                if (clip.CurrentFrame <= b) return;
                continue;
            }
            Log($"skip: {clip.Movie.Name} frame {start} -> {exit}");
            clip.GotoFrame(exit);
            clip.Play();
            RunQueue();
            // An exit that only leaves in some cases (Episode_4-2's frame 327 leaves only in play, not from the
            // chronicle, which goes on to a second part): not left, so on to the next one.
            if (Cutscene == clip && GetLevel(1) is { } player && Vm.GetVariableAt(player, "BeginLoadMovie").ToStr(6) != "1" &&
                exit < clip.FrameCount && Exits(clip.Movie).Any(e => e > exit))
            {
                clip.GotoFrame(exit + 1);
                clip.Play();
                RunQueue();
                continue;
            }
            return;
        }
    }

    /// <summary>The furthest frame on that a frame's script can jump its timeline to (null if none).</summary>
    int? ForwardJump(Assets.MovieAsset movie, int frame)
    {
        int? best = null;
        foreach (var script in movie.Doc.Timeline.Frames[frame - 1].Scripts ?? [])
        {
            var path = Path.Combine(GameDir, "movies", movie.Name, script);
            if (!File.Exists(path)) continue;
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(path), @"goto(?:AndPlay|AndStop)\s*\(\s*(\d+)\s*\)"))
                if (int.Parse(m.Groups[1].Value) is int to && to > frame && to <= movie.Doc.Timeline.Frames.Count) best = Math.Max(best ?? 0, to);
        }
        return best;
    }

    readonly Dictionary<Assets.MovieAsset, int[]> _branches = [];

    /// <summary>
    /// Frames of a cutscene whose script jumps elsewhere in its own timeline (gotoAndPlay/gotoAndStop), past its
    /// loader: not a character's animation (in a tellTarget block, e.g. "Maku" to "Talk"), nor the waits for
    /// frames to load (if (frameLoaded(n)) on to the next frame, else back a frame).
    /// </summary>
    int[] Branches(Assets.MovieAsset movie)
    {
        if (_branches.TryGetValue(movie, out var known)) return known;
        var exits = Exits(movie);
        var list = new List<int>();
        var frames = movie.Doc.Timeline.Frames;
        var tellTarget = new System.Text.RegularExpressions.Regex(@"tellTarget\s*\([^)]*\)\s*\{[^{}]*\}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        var loader = new System.Text.RegularExpressions.Regex(@"if\s*\(\s*frameLoaded\(\d+\)\s*\)\s*\{\s*gotoAndPlay\(\d+\);\s*\}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        var jump = new System.Text.RegularExpressions.Regex(@"goto(?:AndPlay|AndStop)\s*\(\s*(\d*)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        for (int i = 4; i < frames.Count; i++)
            foreach (var script in frames[i].Scripts ?? [])
            {
                var path = Path.Combine(GameDir, "movies", movie.Name, script);
                if (exits.Contains(i + 1) || !File.Exists(path)) continue;
                var text = loader.Replace(tellTarget.Replace(File.ReadAllText(path), ""), "");
                // (A loader's loop back to its check, the frame before.)
                if (jump.Matches(text).Any(m => !(int.TryParse(m.Groups[1].Value, out int to) && to == i)))
                    list.Add(i + 1);
            }
        return _branches[movie] = [.. list];
    }

    readonly SKPaint _skipBack = new() { Color = new SKColor(0x33, 0x33, 0x33), IsAntialias = true };
    readonly SKPaint _skipBorder = new() { Color = new SKColor(0x99, 0x99, 0x99), Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true };
    readonly SKPaint _skipText = new() { Color = new SKColor(0xFF, 0x99, 0x00), IsAntialias = true };
    readonly SKPaint _skipTextLit = new() { Color = SKColors.White, IsAntialias = true };
    readonly SKFont _skipFont = new(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold) ?? SKTypeface.Default, 13);

    /// <summary>The "Skip" label the settings patch adds to the overlay, in the dialogs' font (normal, lit).</summary>
    (Assets.TextAsset Normal, Assets.TextAsset Lit)? SkipLabel()
    {
        if (GetMovie("overlay") is not { } overlay || overlay.Doc.Exports is not { } exports) return null;
        return exports.TryGetValue("patch_skip_text", out var a) && exports.TryGetValue("patch_skip_text_lit", out var b) &&
               overlay.Symbol(a) is Assets.TextAsset normal && overlay.Symbol(b) is Assets.TextAsset lit
            ? (normal, lit) : null;
    }

    /// <summary>The title screen: which save slot Continue and New Game use, in a box joined below its Save Slot button.</summary>
    /// <summary>
    /// The title screen's corner items, Settings and Save Slot (bottom right) and the version (bottom left), kept in
    /// the view's corners: with widescreen on, the wider view's rather than the 4:3 stage's (where the movie places
    /// them, 16 from its right edge and 14 from its left).
    /// </summary>
    void PlaceTitleCorners()
    {
        if (!TitleShowing || GetLevel(TitleLevel)?.ChildByName("ui") is not MovieClip ui) return;
        void At(string name, float x)
        {
            if (ui.ChildByName(name) is not { } c || Math.Abs(c.X - x) < 0.01) return;
            // (Moved outright, not slid there between frames.)
            c.X = x;
            c.PrevMatrix.TransX = x;
        }
        float right = ViewRight - (StageWidth - 677);
        At("slotButton", right);
        At("settingsButton", right);
        At("version", ViewLeft + 14);
        // Continue, New Game and Exit are hidden while the settings dialog or one of its pages (Audio, Timestamp,
        // Progress...) is open (where their descriptions go).
        bool settings = AudioSettings != null || GetLevel(TitleLevel) is MovieClip title && title.Children.Values.Any(c =>
            c is MovieClip { Removed: false } && c.Name.StartsWith("matanui_dialog_", StringComparison.Ordinal));
        // (Continue only for a save slot that's been started: the title's canContinue.)
        bool canContinue = Vm.GetVariableAt(ui, "canContinue").ToBool(6);
        foreach (var name in new[] { "continueButton", "newGameButton", "exitButton" })
            if (ui.ChildByName(name) is { } b) b.Visible = !settings && (name != "continueButton" || canContinue);
    }

    /// <summary>The dialog open over the game (the overlay's, at depth 16385: settings and its pages, quit, timestamp,
    /// progress) or the title screen (its own copies), if any.</summary>
    /// <summary>Closes the settings dialog or one of its pages (Graphics, Audio...), if open; false if none was.</summary>
    bool CloseSettingsDialog()
    {
        bool closed = AudioSettings != null;
        AudioSettings = null;
        if (OpenDialog() is { Name: var name } dialog && name.StartsWith("matanui_dialog_settings", StringComparison.Ordinal)
            && dialog.Parent is MovieClip parent)
        {
            parent.RemoveChild(dialog);
            closed = true;
        }
        return closed;
    }

    MovieClip? OpenDialog() =>
        TitleShowing ? GetLevel(TitleLevel)?.Children.Values.LastOrDefault(c => c is MovieClip { Removed: false, Visible: true } && c.Name.StartsWith("matanui_dialog_", StringComparison.Ordinal)) as MovieClip
        : GetLevel(10001)?.ChildByName("matanui_overlay") is MovieClip overlay && overlay.Children.TryGetValue(16384 + 1, out var d)
            && d is MovieClip { Removed: false, Visible: true } m ? m : null;

    /// <summary>The settings dialog open over the game (the overlay's) or the title screen (its own copy), if any.</summary>
    MovieClip? SettingsDialog =>
        TitleShowing ? GetLevel(TitleLevel)?.ChildByName("matanui_dialog_settings") as MovieClip
        : GetLevel(10001)?.ChildByName("matanui_overlay") is MovieClip overlay && overlay.Children.TryGetValue(16384 + 1, out var d)
            && d is MovieClip { Removed: false, Name: "matanui_dialog_settings" } m ? m : null;

    /// <summary>What each settings option does (by its button's name, after "_button_"), shown below the dialog while
    /// the mouse is over it. The main menu's categories (Language, Graphics...) have none.</summary>
    static readonly Dictionary<string, string> SettingsHints = new()
    {
        ["english"] = "Play the game in English: its dialog, text and the voices' subtitles.",
        ["deutsche"] = "Play the game in German (Deutsch): its dialog, text and the voices' subtitles.",
        ["francais"] = "Play the game in French (Français). Text not yet translated is in English.",
        ["fullscreen"] = "On: the game fills the whole screen.\nOff: the game plays in a window.\nF11 or Alt + Enter also switch between them.",
        ["widescreen"] = "On: full widescreen is used.\nOff: game's original aspect ratio is used.",
        ["smooth"] = "Off: animation moves in steps, as the original did.\nPartial: motion is smoothed where it looks best.\nFull: everything is smoothed.",
        ["textboxalpha"] = "Normal: conversation text boxes are see-through, as the original's.\nLow: they're much less see-through, the scene behind them only faintly showing.",
        ["vectorsand"] = "On: the sand, sky and other photographic pictures are redrawn as smooth vector art that stays sharp at any size.\nOff: the original pictures.",
        ["loadscreen"] = "On: the fade and the loading animation between scenes, as in the original.\nSlow: loading shows for at least 3 seconds.\nOff: go straight to the next scene.",
        ["textspeed"] = "How fast conversations write out their text.\nNormal: the original's speed.\nFast: twice as fast.\nInstant: all of it at once.",
        ["panspeed"] = "View panning speed at the edge of the scene.\nSlowest: half the original's speed.\nSlow: three quarters of it.\nNormal: the original's speed.\nFast: fifty percent faster than original.",
        ["panzone"] = "How near the edge the mouse pans the view.\nSmall: right by the edge.\nMedium: a little further in.\nNormal: as far in as the original.\nOff: only the arrow keys and W A S D move it.",
        ["title"] = "On: the title screen shows when the game starts.\nOff: the game goes straight to where you left off.",
        ["timestamp"] = "The date the game believes it is, which decides the parts of the island that are open, as on the original site.",
        ["progress"] = "Your progress as a code: copy it to keep it or move it to another computer, or enter a code to load it.",
        ["matoran"] = "Tohunga: the villagers are called Tohunga, as in the original.\nMatoran: they're called Matoran, the name later Bionicle stories gave them.",
        ["colors"] = "On: some characters' colors are corrected where the original shows them wrongly.\nOff: the original's colors.",
        ["clones"] = "On: a few villagers drawn as copies of others are redrawn to look like themselves.\nOff: as in the original.",
        ["pakari"] = "On: the Pakari masks the original drew wrongly are redrawn as the real mask looks.\nOff: as in the original.",
        ["ruru"] = "Original: each Ruru's mask as the original drew it.\nVariant 1 and Variant 2: every Ruru wears the same one of the original's two Ruru designs.",
    };

    void DrawSettingsHint(SKCanvas canvas)
    {
        if (_hover is not { } button || SettingsDialog is not { } dialog) return;
        DisplayObject? up = button;
        while (up != null && up != dialog) up = up.Parent;
        if (up == null) return;
        int at = button.Name.IndexOf("_button_", StringComparison.Ordinal);
        if (at < 0 || button.Name.StartsWith("matanui_dialog_settings_", StringComparison.Ordinal)) return;
        if (!SettingsHints.TryGetValue(button.Name[(at + 8)..], out var hint)) return;
        hint = Lang.T(hint);
        // Below the dialog's box (its first child, the panel).
        var box = dialog.Children.Values.FirstOrDefault(c => c.Name.EndsWith("_box", StringComparison.Ordinal)) ?? dialog;
        var r = box.WorldMatrix.MapRect(box.LocalBounds);
        MenuGlyphs.DrawHint(canvas, r, hint, MenuTypeface, DialogFont(13.5f), StageHeight);
        DrawTweakPreview(canvas, r, button.Name[(at + 8)..]);
    }

    /// <summary>The settings whose difference is shown in pictures (game/files/previews): each picture and its name.</summary>
    static readonly Dictionary<string, (string Image, string Label)[]> TweakPreviews = new()
    {
        ["pakari"] = [("pakari-original.png", "Original"), ("pakari-accurate.png", "Accurate")],
        ["ruru"] = [("ruru-variant-1.png", "Variant 1"), ("ruru-variant-2.png", "Variant 2")],
    };

    readonly Dictionary<string, SKImage?> _previewImages = [];

    SKImage? PreviewImage(string file)
    {
        if (!_previewImages.TryGetValue(file, out var image))
        {
            var path = Path.Combine(GameDir, "files", "previews", file);
            _previewImages[file] = image = File.Exists(path) ? SKImage.FromEncodedData(path) : null;
        }
        return image;
    }

    /// <summary>A setting's pictures (the mask each way it draws it), side by side in a box right of the dialog's, each
    /// named below it, in the hint box's look.</summary>
    void DrawTweakPreview(SKCanvas canvas, SKRect dialog, string key)
    {
        if (!TweakPreviews.TryGetValue(key, out var items)) return;
        var images = items.Select(i => PreviewImage(i.Image)).ToArray();
        if (images.Any(i => i == null)) return;
        const float Pad = 9, ImageHeight = 96, Gap = 12, LabelHeight = 18;
        var widths = images.Select(i => ImageHeight * i!.Width / i.Height).ToArray();
        float width = Pad * 2 + widths.Sum() + Gap * (items.Length - 1);
        // (Level with the dialog's bottom, by the rows it shows.)
        float height = Pad * 2 + ImageHeight + LabelHeight;
        var box = new SKRect(dialog.Right + 8, dialog.Bottom - height, dialog.Right + 8 + width, dialog.Bottom);
        using var paint = new SKPaint { IsAntialias = true, Color = new SKColor(0x33, 0x33, 0x33, 0xF4) };
        canvas.DrawRect(box, paint);
        paint.Style = SKPaintStyle.Stroke; paint.StrokeWidth = 2; paint.Color = new SKColor(0x99, 0x99, 0x99);
        canvas.DrawRect(box, paint);
        paint.Style = SKPaintStyle.Fill; paint.Color = new SKColor(0xFF, 0x99, 0x00);
        using var font = new SKFont(MenuTypeface, 12.5f);
        var sampling = new SKSamplingOptions(SKCubicResampler.Mitchell);
        float x = box.Left + Pad;
        for (int k = 0; k < items.Length; k++)
        {
            canvas.DrawImage(images[k]!, new SKRect(x, box.Top + Pad, x + widths[k], box.Top + Pad + ImageHeight), sampling);
            string label = Lang.T(items[k].Label);
            canvas.DrawText(label, x + widths[k] / 2 - font.MeasureText(label) / 2, box.Top + Pad + ImageHeight + 14, font, paint);
            x += widths[k] + Gap;
        }
    }

    void DrawTitleSlot(SKCanvas canvas)
    {
        // New Game's confirmation (the settings menu's dialog, placed at 385, 213) says which slot it empties,
        // on the blank line between its two lines of text.
        if (GetLevel(TitleLevel)?.ChildByName("confirm") is { Visible: true })
        {
            using var warnFont = new SKFont(MenuTypeface, 15.85f);
            using var warn = new SKPaint { Color = new SKColor(0xFF, 0x99, 0x00), IsAntialias = true };
            canvas.DrawText(Lang.F("Save slot {0} will be erased.", CurrentSaveSlot), 189, 216, warnFont, warn);
        }
        if (GetLevel(TitleLevel)?.ChildByName("ui") is not { Visible: true } ui) return;
        float alpha = ui.DrawColor(1).AM;
        if (alpha <= 0) return;
        byte a = (byte)Math.Round(Math.Clamp(alpha, 0, 1) * 255);
        // A box joined to the bottom of the Save Slot button (centred at y 430, 154 by 21: the title's buttons at 70%;
        // in the corner, see PlaceTitleCorners), in the buttons' look.
        float slotX = (ui as MovieClip)?.ChildByName("slotButton") is { } slot ? (float)slot.X : 677;
        var box = new SKRect(slotX - 77, 430 + 10.5f, slotX + 77, 430 + 10.5f + 16);
        using var fill = new SKPaint { Color = new SKColor(0x33, 0x33, 0x33, (byte)(a * 0x90 / 255)), IsAntialias = true };
        using var border = new SKPaint { Color = new SKColor(0x99, 0x99, 0x99, a), Style = SKPaintStyle.Stroke, StrokeWidth = 1.4f, IsAntialias = true };
        canvas.DrawRect(box, fill);
        canvas.DrawRect(box, border);
        using var font = new SKFont(MenuTypeface, 10);
        using var paint = new SKPaint { Color = new SKColor(0xFF, 0x99, 0x00, a), IsAntialias = true };
        string label = Lang.F("Slot {0}", CurrentSaveSlot);
        canvas.DrawText(label, box.MidX - font.MeasureText(label) / 2, box.MidY + 3.5f, font, paint);
    }

    // ---- Chronicle wallpapers ----

    /// <summary>The wallpaper menu (the chronicle's "mac" button): a preview, the size, and Save.</summary>
    public WallpaperMenu? Wallpaper;

    /// <summary>The chronicle's music window (its music button: the entry's soundtrack), while open.</summary>
    public MusicMenu? Music;

    /// <summary>Graphics > Textbox transparency Low: conversation text boxes less see-through (RenderContext.ConfigurePaint).</summary>
    public bool LowTextboxTransparency;

    readonly Dictionary<int, bool> _hasMusic = [];

    /// <summary>The chronicle's music button (the book's button 70) greyed and not pressable for the chosen entry
    /// when it has no music yet.</summary>
    void UpdateMusicButton()
    {
        if (GetLevel(1)?.ChildByName("holder") is not MovieClip { Movie.Name: "book" } book) return;
        if (book.Children.Values.FirstOrDefault(c => c is ButtonObject { Asset.Id: 70 }) is not ButtonObject button) return;
        int slot = (int)Vm.GetVariableAt(book, "CurrentTextSlot").ToNumber(6);
        if (!_hasMusic.TryGetValue(slot, out bool has)) _hasMusic[slot] = has = MusicMenu.Tracks(this, slot).Count > 0;
        if (button.Disabled == !has) return;
        button.Disabled = !has;
        button.ColorTransform = has ? CxForm.Identity : new CxForm(0.4f, 0.4f, 0.4f, 0.75f, 0, 0, 0, 0);
        button.TransformedByScript = true;
        button.TransformedByScript = true;
        if (!has && _hover == button) { _hover = null; Host?.SetHandCursor(false); }
    }

    /// <summary>The chronicle's entry names (its list's TextSlotN), for naming what's saved from it.</summary>
    public string ChronicleTitle(int slot)
    {
        string title = GetLevel(1)?.ChildByName("holder") is MovieClip bookClip ? Vm.GetVariableAt(bookClip, $"TextSelect/:TextSlot{slot}").ToStr(6) : "";
        return string.IsNullOrWhiteSpace(title) ? $"Chronicle {slot}" : title;
    }

    /// <summary>
    /// One of a chronicle slot's wallpapers (the book's wallpapers.json, {"slot": [entries]}): an entry is "default" (the
    /// frame its live picture shows, with its touches), a frame number, or {"frame": n or "default", "zoom": z, "x": dx,
    /// "y": dy, "letterbox": true, "stretch": {"path": [sx, sy]}}: the camera zoomed (times its own fit) and moved (stage
    /// pixels), the episode's own black letterbox kept, objects stretched about their own origin, "behind": another
    /// frame of the episode drawn first, beneath it (e.g. a title over the scenery of a later frame), "crop": false to
    /// keep the fit across the stage where the art leaves the top or bottom empty (e.g. art on a black backdrop),
    /// "place": objects put at [x, y] in their parent (where the episode's scripts move them), "clone": objects copied,
    /// moved by each [dx, dy] (e.g. a pattern continued). "behind" may be {"frame": n, "x": dx, "y": dy, "hide": [paths]}
    /// (moved, parts left out), {"scene": name, "frame": n, "scale": s, ...} for another movie's art (its own default
    /// panning, scaled about the stage's top left), or a list of these, the first lowest. "hide": objects left out. "fill": [{"y0", "y1", "colors"}],
    /// bands of colour (top to bottom) across the whole width, between the frames behind and the shot (e.g. ground).
    /// "show": objects kept that the letterbox's removal took for it (a big black shape that's art, e.g. a branch).
    /// "scene": another episode's frame instead (one not in the chronicle, e.g. "Episode_6-1"; "frame" is its frame).
    /// </summary>
    public sealed record WallpaperShot(int? Frame, float Zoom = 1, float X = 0, float Y = 0, bool Letterbox = false,
                                        Dictionary<string, (float Sx, float Sy)>? Stretch = null, bool? Played = null,
                                        Dictionary<string, int>? Hold = null, List<WallpaperLayer>? Behind = null, bool Crop = true,
                                        Dictionary<string, (float X, float Y)>? Place = null,
                                        Dictionary<string, List<(float Dx, float Dy)>>? Copies = null, List<string>? Hide = null,
                                        List<(float Y0, float Y1, SKColor[] Colors)>? Fills = null, string? Scene = null,
                                        List<string>? Show = null);

    /// <summary>A wallpaper's frame or other scene drawn behind it (see WallpaperShot's "behind").</summary>
    public sealed record WallpaperLayer(int? Frame, string? Scene, float X, float Y, float Scale, List<string>? Hide);

    /// <summary>A chronicle slot's wallpapers (just its picture's frame when the slot isn't listed).</summary>
    public List<WallpaperShot> WallpaperFrames(int slot)
    {
        var list = new List<WallpaperShot>();
        if (GetMovie("book") is { } book && Path.Combine(book.Dir, "wallpapers.json") is var file && File.Exists(file) &&
            System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(file)) is System.Text.Json.Nodes.JsonObject all &&
            all[slot.ToString()] is System.Text.Json.Nodes.JsonArray entries)
            foreach (var e in entries)
            {
                static float Num(System.Text.Json.Nodes.JsonNode? n) => n == null ? 0 : (float)n.GetValue<double>();
                static WallpaperLayer? Layer(System.Text.Json.Nodes.JsonNode? n) => n switch
                {
                    System.Text.Json.Nodes.JsonObject l => new WallpaperLayer(FrameOf(l["frame"]), l["scene"]?.GetValue<string>(), Num(l["x"]), Num(l["y"]),
                        l["scale"] is { } sc ? Num(sc) : 1, l["hide"] is System.Text.Json.Nodes.JsonArray hl ? hl.Select(h => h!.GetValue<string>()).ToList() : null),
                    System.Text.Json.Nodes.JsonValue => FrameOf(n) is int f ? new WallpaperLayer(f, null, 0, 0, 1, null) : null,
                    _ => null,
                };
                static List<WallpaperLayer>? Layers(System.Text.Json.Nodes.JsonNode? n) =>
                    n is System.Text.Json.Nodes.JsonArray a ? a.Select(Layer).OfType<WallpaperLayer>().ToList() : Layer(n) is { } one ? [one] : null;
                static int? FrameOf(System.Text.Json.Nodes.JsonNode? n) => n is System.Text.Json.Nodes.JsonValue v && v.TryGetValue<int>(out int f) ? f : null;
                if (e is System.Text.Json.Nodes.JsonObject o)
                {
                    Dictionary<string, (float, float)>? stretch = null;
                    if (o["stretch"] is System.Text.Json.Nodes.JsonObject st)
                    {
                        stretch = [];
                        foreach (var (k, v) in st)
                            if (v is System.Text.Json.Nodes.JsonArray { Count: 2 } xy) stretch[k] = ((float)xy[0]!.GetValue<double>(), (float)xy[1]!.GetValue<double>());
                    }
                    list.Add(new WallpaperShot(FrameOf(o["frame"]), (float)(o["zoom"]?.GetValue<double>() ?? 1), (float)(o["x"]?.GetValue<double>() ?? 0),
                        (float)(o["y"]?.GetValue<double>() ?? 0), o["letterbox"]?.GetValue<bool>() == true, stretch,
                        o["played"] is { } pl ? pl.GetValue<bool>() : null,
                        o["hold"] is System.Text.Json.Nodes.JsonObject ho ? ho.ToDictionary(kv => kv.Key, kv => kv.Value!.GetValue<int>()) : null,
                        Layers(o["behind"]), o["crop"]?.GetValue<bool>() != false,
                        o["place"] is System.Text.Json.Nodes.JsonObject po
                            ? po.Where(kv => kv.Value is System.Text.Json.Nodes.JsonArray { Count: 2 }).ToDictionary(kv => kv.Key, kv => (Num(kv.Value![0]), Num(kv.Value![1]))) : null,
                        o["clone"] is System.Text.Json.Nodes.JsonObject co
                            ? co.ToDictionary(kv => kv.Key, kv => (kv.Value as System.Text.Json.Nodes.JsonArray ?? []).OfType<System.Text.Json.Nodes.JsonArray>().Select(xy => (Num(xy[0]), Num(xy[1]))).ToList()) : null,
                        o["hide"] is System.Text.Json.Nodes.JsonArray hd ? hd.Select(h => h!.GetValue<string>()).ToList() : null,
                        o["fill"] is System.Text.Json.Nodes.JsonArray fl ? fl.OfType<System.Text.Json.Nodes.JsonObject>().Select(f => (Num(f["y0"]), Num(f["y1"]),
                            (f["colors"] as System.Text.Json.Nodes.JsonArray ?? []).Select(c => SKColor.Parse(c!.GetValue<string>())).ToArray())).ToList() : null,
                        o["scene"]?.GetValue<string>(),
                        o["show"] is System.Text.Json.Nodes.JsonArray sh ? sh.Select(h => h!.GetValue<string>()).ToList() : null));
                }
                else list.Add(new WallpaperShot(FrameOf(e)));
            }
        if (list.Count == 0) list.Add(new WallpaperShot(null));
        return list;
    }

    /// <summary>
    /// A desktop wallpaper of a chronicle slot's episode, width x height (see WallpaperShot), drawn with Vectorize
    /// bitmaps, filling the size (the episode's letterbox left out, its art going on beneath it; where the art doesn't
    /// reach the top or bottom, zoomed in until those empty strips are cropped off). Null if the slot has no picture.
    /// </summary>
    public SKImage? RenderWallpaper(int slot, int width, int height, WallpaperShot? want = null) => RenderWallpaper(slot, width, height, want, null);

    /// <summary>The same, and drawn once more into <paramref name="svg"/> as an SVG of that size (its shapes as vector
    /// paths; images and blurs as embedded pictures).</summary>
    public SKImage? RenderWallpaper(int slot, int width, int height, WallpaperShot? want, Stream? svg)
    {
        want ??= new WallpaperShot(null);
        if (GetMovie("book") is not { } book) return null;
        int image = slot == 1 ? 1 : 2 * slot + 2;
        // A copy of its live picture's settings (its own scene copy, apart from the one the chronicle shows).
        if (LiveBackdrop.Load(book.Dir).FirstOrDefault(b => b.Image == image) is not { } shot) return null;
        // (Another episode's: a plain copy of that scene, at the frame given.)
        if (want.Scene is { } otherScene) shot = new LiveBackdrop { Image = 0, Scene = otherScene, Frame = want.Frame ?? 1 };
        else if (want.Frame is int chosen)
        {
            // Another frame than the picture's: as the episode has it there (the picture's own touches, holding or
            // hiding its clips for that moment, left out).
            shot.Frame = chosen;
            shot.Hold.Clear();
            shot.Hide.Clear();
            shot.Move.Clear();
        }
        shot.Unbar = !want.Letterbox;
        // ("played": reached by playing the episode up to it, its clips animating on the way, e.g. wings mid-beat.)
        if (want.Played is bool played) shot.Played = played;
        shot.Cues = true;
        // ("hold": clips held at a frame, e.g. wings a script sets flapping, at their widest.)
        foreach (var (path, f) in want.Hold ?? []) shot.Hold[path] = f;
        // ("hide": objects left out, e.g. a caption.)
        shot.Hide.AddRange(want.Hide ?? []);
        if (shot.Build(this) is not { } root) return null;
        foreach (var path in want.Show ?? [])
            if (LiveBackdrop.FindIn(root, path) is { } shown) shown.Visible = true;
        var behind = new List<MovieClip>();
        foreach (var layer in want.Behind ?? [])
        {
            LiveBackdrop? under;
            // (A frame of the same episode: of the wallpaper's own other episode, when it has one.)
            if ((layer.Scene ?? want.Scene) is { } scene) under = new LiveBackdrop { Image = 0, Scene = scene, Frame = layer.Frame ?? 1, Played = shot.Played, Cues = true };
            else if ((under = LiveBackdrop.Load(book.Dir).FirstOrDefault(b => b.Image == image)) != null)
            {
                under.Frame = layer.Frame ?? under.Frame;
                under.Hold.Clear();
                under.Hide.Clear();
                under.Move.Clear();
                under.Played = shot.Played;
                under.Cues = true;
            }
            if (under == null) continue;
            // ("hide": its parts left out, where they'd show around the frame drawn over them.)
            under.Hide.AddRange(layer.Hide ?? []);
            under.Unbar = shot.Unbar;
            if (under.Build(this) is not { } built) continue;
            if (layer.X != 0 || layer.Y != 0 || layer.Scale != 1)
                foreach (var c in built.Children.Values)
                {
                    c.Matrix = c.Matrix.PostConcat(SKMatrix.CreateScale(layer.Scale, layer.Scale)).PostConcat(SKMatrix.CreateTranslation(layer.X, layer.Y));
                    c.TransformedByScript = true;
                }
            behind.Add(built);
        }
        // (Captions, text fields and buttons left out: a wallpaper is the picture, not the dialogue or its controls.)
        static void HideText(MovieClip clip)
        {
            foreach (var c in clip.Children.Values)
                if (c is Mnog.Engine.Display.EditTextObject or Mnog.Engine.Display.ButtonObject) c.Visible = false;
                else if (c is MovieClip mc) HideText(mc);
        }
        HideText(root);
        foreach (var under in behind) HideText(under);
        void DrawRoots(Mnog.Engine.Render.RenderContext rc, SKCanvas canvas, SKMatrix matrix)
        {
            foreach (var under in behind) rc.DrawContainer(under.Children, SKMatrix.Identity, CxForm.Identity);
            foreach (var (y0, y1, colors) in want.Fills ?? [])
            {
                if (colors.Length == 0) continue;
                canvas.Save();
                canvas.SetMatrix(matrix);
                using var paint = new SKPaint { IsAntialias = true };
                paint.Shader = colors.Length == 1 ? SKShader.CreateColor(colors[0])
                    : SKShader.CreateLinearGradient(new SKPoint(0, y0), new SKPoint(0, y1), colors, SKShaderTileMode.Clamp);
                canvas.DrawRect(SKRect.Create(-4000, y0, 8000, y1 - y0), paint);
                canvas.Restore();
            }
            rc.DrawContainer(root.Children, SKMatrix.Identity, CxForm.Identity);
        }
        foreach (var (path, (px, py)) in want.Place ?? [])
            if (LiveBackdrop.FindIn(root, path) is { } placed)
            {
                placed.Matrix = placed.Matrix with { TransX = px, TransY = py };
                placed.TransformedByScript = true;
            }
        foreach (var (path, offsets) in want.Copies ?? [])
            if (LiveBackdrop.FindIn(root, path) is Mnog.Engine.Display.GraphicObject { Parent: MovieClip parent } src)
                foreach (var (dx, dy) in offsets)
                {
                    int depth = parent.Children.Count == 0 ? 1 : parent.Children.Keys.Max() + 1;
                    parent.AddChild(new Mnog.Engine.Display.GraphicObject(this, src.Asset!)
                        { Matrix = src.Matrix.PostConcat(SKMatrix.CreateTranslation(dx, dy)), ColorTransform = src.ColorTransform, TransformedByScript = true }, depth);
                }
        foreach (var (path, (sx, sy)) in want.Stretch ?? [])
            if (LiveBackdrop.FindIn(root, path) is { } o)
            {
                o.Matrix = o.Matrix.PreConcat(SKMatrix.CreateScale(sx, sy));
                o.TransformedByScript = true;
            }
        // Across the stage's width, centred on the scene's part of it (above the bottom bar).
        float scale = (float)width / StageWidth;
        float viewHeight = StageWidth * height / (float)width;
        float top = 425 / 2f - viewHeight / 2, left = 0;
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        bool vector = VectorImages;
        VectorImages = true;
        void Draw(SKColor? backing = null)
        {
            surface.Canvas.Clear(backing ?? SKColors.Black);
            var baseMatrix = SKMatrix.CreateScaleTranslation(scale, scale, -left * scale, -top * scale);
            DrawRoots(new Mnog.Engine.Render.RenderContext(surface.Canvas, baseMatrix, this, 1), surface.Canvas, baseMatrix);
        }
        // (Drawn first over magenta: where it still shows, the episode drew nothing. With the letterbox kept, it's as shown.)
        bool crop = !want.Letterbox && want.Crop;
        if (crop) Draw(new SKColor(255, 0, 255));
        if (crop)
        using (var first = surface.Snapshot())
        using (var pixels = SKBitmap.FromImage(first))
        {
            int step = Math.Max(1, width / 240);
            bool Empty(int y)
            {
                int lit = 0, n = 0;
                for (int x = 0; x < width; x += step, n++)
                    if (pixels.GetPixel(x, y) is var c && !(c.Red > 240 && c.Green < 16 && c.Blue > 240)) lit++;
                // (Empty: a tenth or more of the row not drawn.)
                return lit < n * 9 / 10;
            }
            int y0 = 0, y1 = height - 1;
            while (y0 < height / 3 && Empty(y0)) y0++;
            while (y1 > height * 2 / 3 && Empty(y1)) y1--;
            if (y0 > 0 || y1 < height - 1)
            {
                float artTop = top + y0 / scale, artBottom = top + (y1 + 1) / scale;
                float zoom = viewHeight / (artBottom - artTop);
                scale *= zoom;
                float viewWidth = StageWidth / zoom;
                viewHeight /= zoom;
                top = artTop;
                left = (StageWidth - viewWidth) / 2;
            }
        }
        // The entry's own camera: zoomed about the view's middle, then moved (stage pixels).
        if (want.Zoom != 1 || want.X != 0 || want.Y != 0)
        {
            float viewWidth = width / scale, cx = left + viewWidth / 2 + want.X, cy = top + viewHeight / 2 + want.Y;
            scale *= want.Zoom;
            viewWidth /= want.Zoom;
            viewHeight /= want.Zoom;
            left = cx - viewWidth / 2;
            top = cy - viewHeight / 2;
        }
        Draw();
        // Vector art is drawn in the background at this size: waited for, then drawn again.
        var waited = System.Diagnostics.Stopwatch.StartNew();
        if (Volatile.Read(ref Mnog.Engine.Render.RenderContext.VectorImagesPending) > 0)
        {
            while (Volatile.Read(ref Mnog.Engine.Render.RenderContext.VectorImagesPending) > 0 && waited.ElapsedMilliseconds < 15000) Thread.Sleep(30);
            Draw();
        }
        if (svg != null)
        {
            using (var svgCanvas = SKSvgCanvas.Create(SKRect.Create(width, height), svg))
            {
                svgCanvas.Clear(SKColors.Black);
                var baseMatrix = SKMatrix.CreateScaleTranslation(scale, scale, -left * scale, -top * scale);
                DrawRoots(new Mnog.Engine.Render.RenderContext(svgCanvas, baseMatrix, this, 1) { ClipMasks = true }, svgCanvas, baseMatrix);
            }
        }
        VectorImages = vector;
        // (The view's cached layers were drawn meanwhile with Vectorize bitmaps on: redrawn as they are.)
        _layerCache.Clear();
        return surface.Snapshot();
    }

    /// <summary>Saves a chronicle slot's wallpaper at that size, where the player chooses (the Save dialog).</summary>
    public void SaveWallpaper(int slot, int width, int height, WallpaperShot? shot = null, int number = 1, bool asSvg = false)
    {
        byte[] png;
        using (var svgStream = asSvg ? new MemoryStream() : null)
        {
            using var picture = RenderWallpaper(slot, width, height, shot, svgStream);
            if (picture == null) { ShowMessage(Lang.T("No picture for this entry")); return; }
            png = svgStream != null ? svgStream.ToArray() : picture.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }
        string ext = asSvg ? "svg" : "png";
        string name = $"Mata Nui Online Game - {ChronicleTitle(slot)} wallpaper {number}{(asSvg ? "" : $" ({width}x{height})")}.{ext}";
        foreach (char bad in Path.GetInvalidFileNameChars()) name = name.Replace(bad, '-');
        // (MNOG_SAVETO: a folder to save into without asking, for the test harness.)
        if (Environment.GetEnvironmentVariable("MNOG_SAVETO") is { Length: > 0 } saveTo)
        {
            File.WriteAllBytes(Path.Combine(saveTo, name), png);
            ShowMessage(Lang.F("Wallpaper saved: {0}", name));
            return;
        }
        SaveDialog.Ask("Save wallpaper", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), name, asSvg ? "SVG image" : "PNG image", ext, path =>
        {
            if (path == null) { _pendingMessage = "Wallpaper not saved"; return; }
            try { File.WriteAllBytes(path, png); _pendingMessage = $"Wallpaper saved: {(OperatingSystem.IsLinux() ? path : Path.GetFileName(path))}"; }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { _pendingMessage = $"Couldn't save: {e.Message}"; }
        });
    }


    /// <summary>The title screen's scenery (its beach and sand, without the logo and menu) as an SVG of that size, framed
    /// as the window shows it (the project page's background).</summary>
    public bool SaveTitleSvg(Stream svg, int width, int height)
    {
        if (GetLevel(TitleLevel) is not { } title) return false;
        var ui = title.ChildByName("ui");
        bool shown = ui?.Visible ?? false;
        if (ui != null) ui.Visible = false;
        var (scale, ox, oy) = Viewport(width, height);
        using (var canvas = SKSvgCanvas.Create(SKRect.Create(width, height), svg))
        {
            canvas.Clear(SKColors.Black);
            var matrix = SKMatrix.CreateScaleTranslation(scale, scale, ox, oy);
            new Mnog.Engine.Render.RenderContext(canvas, matrix, this, 1) { ClipMasks = true }
                .DrawContainer(title.Children, title.Matrix, CxForm.Identity);
        }
        if (ui != null) ui.Visible = shown;
        return true;
    }

    /// <summary>
    /// The title screen's scenery in layers, for the project page to animate its bird and water, from the back: sky.svg
    /// (the sky and distant scenery), the bird's frames (bird-NNN.svg), back.svg (the beach beneath the water), the
    /// water's frames (sparkle-NNN, foam-NNN, waves-NNN.svg), and front.svg (the pods and sand over it). All but the
    /// sky are transparent where they don't draw. Into <paramref name="dir"/>, at that size.
    /// </summary>
    public bool SaveTitleLayers(string dir, int width, int height)
    {
        if (GetLevel(TitleLevel) is not { } title || title.ChildByName("holder") is not MovieClip holder ||
            holder.ChildByName("Back-Ground") is not MovieClip background ||
            holder.ChildByName("Middle-Ground") is not MovieClip middle) return false;
        // The beach's (I.1-S) moving clips, by their symbols: the bird in the sky, and the water's sparkle, foam lines
        // and waves on the shore.
        var loops = new (string Name, int Symbol)[] { ("bird", 18), ("sparkle", 25), ("foam", 32), ("waves", 37) };
        var moving = background.Children.Values.Concat(middle.Children.Values).OfType<MovieClip>()
            .Where(c => loops.Any(l => l.Symbol == c.Asset?.Id)).ToList();
        var water = moving.Where(c => c.Parent == middle).ToList();
        if (water.Count == 0) return false;
        int top = water.Max(c => c.Depth);
        var all = title.Children.Values.Concat(holder.Children.Values).Concat(background.Children.Values).Concat(middle.Children.Values).ToList();
        var was = all.ToDictionary(o => o, o => o.Visible);
        var (scale, ox, oy) = Viewport(width, height);
        void Save(string name, bool opaque, Func<DisplayObject, bool> shown)
        {
            foreach (var o in all) o.Visible = was[o] && shown(o);
            // (The containers on the way to what's shown stay shown.)
            holder.Visible = background.Visible = middle.Visible = true;
            if (title.ChildByName("ui") is { } ui) ui.Visible = false;
            using var file = File.Create(Path.Combine(dir, name + ".svg"));
            using var canvas = SKSvgCanvas.Create(SKRect.Create(width, height), file);
            if (opaque) canvas.Clear(SKColors.Black);
            var matrix = SKMatrix.CreateScaleTranslation(scale, scale, ox, oy);
            new Mnog.Engine.Render.RenderContext(canvas, matrix, this, 1) { ClipMasks = true, SkyGradients = false }
                .DrawContainer(title.Children, title.Matrix, CxForm.Identity);
        }
        // (Each layer: what's shown of the background, of the middle ground, and of the rest, the containers aside.)
        bool Shown(DisplayObject o, Func<DisplayObject, bool> back, Func<DisplayObject, bool> mid, bool rest) =>
            o.Parent == background ? back(o) : o.Parent == middle ? mid(o) : o == holder || o == background || o == middle || rest;
        Save("sky", true, o => Shown(o, b => !moving.Contains(b), _ => false, false));
        Save("back", false, o => Shown(o, _ => false, m => m.Depth < top && !moving.Contains(m), false));
        Save("front", false, o => Shown(o, _ => false, m => m.Depth > top, o.Parent == title || o.Parent == holder));
        var at = moving.ToDictionary(c => c, c => c.CurrentFrame);
        foreach (var (name, symbol) in loops)
        {
            var clips = moving.Where(c => c.Asset?.Id == symbol).ToList();
            if (clips.Count == 0) continue;
            // A loop moving a clip with a loop of its own (the bird: a 199-frame path carrying its 39-frame turn and
            // flap, each playing on by itself): the inner loop's frames at the path's start (NAME-NNN.svg), and the
            // path as the inner clip's offset from there at each of its frames, in output pixels (NAME-path.json).
            clips[0].GotoFrame(1);
            if (clips.Count == 1 && clips[0].Children.Values.OfType<MovieClip>().FirstOrDefault(c => c.FrameCount > 1) is { } inner)
            {
                var origin = inner.WorldMatrix;
                for (int f = 1; f <= inner.FrameCount; f++)
                {
                    inner.GotoFrame(f);
                    Save($"{name}-{f:000}", false, o => Shown(o, clips.Contains, clips.Contains, false));
                }
                var path = new List<string>();
                for (int f = 1; f <= clips[0].FrameCount; f++)
                {
                    clips[0].GotoFrame(f);
                    if (clips[0].Children.Values.OfType<MovieClip>().FirstOrDefault(c => c.Asset == inner.Asset) is not { } now) continue;
                    var m = now.WorldMatrix;
                    path.Add($"[{(m.TransX - origin.TransX) * scale:0.##},{(m.TransY - origin.TransY) * scale:0.##}]");
                }
                File.WriteAllText(Path.Combine(dir, $"{name}-path.json"), "[" + string.Join(",", path) + "]\n");
                continue;
            }
            for (int f = 1; f <= clips[0].FrameCount; f++)
            {
                foreach (var c in clips) c.GotoFrame(f);
                Save($"{name}-{f:000}", false, o => Shown(o, clips.Contains, clips.Contains, false));
            }
        }
        foreach (var (c, f) in at) c.GotoFrame(f);
        foreach (var (o, v) in was) o.Visible = v;
        return true;
    }

    /// <summary>A message from another thread (the save dialog's), shown on the next tick.</summary>
    volatile string? _pendingMessage;

    // ---- Messages ----

    (string Text, long At, bool Bar)? _message;
    const double MessageMs = 3500;
    /// <summary>A message in the bar (a setting just switched): only a quick confirmation.</summary>
    const double BarMessageMs = 1500;

    /// <summary>A short message shown over the view for a few seconds (the "message" fscommand); with
    /// <paramref name="bar"/>, in the middle of the bar along the bottom instead, clear of the scene (not on the title
    /// screen, whose menu buttons are there).</summary>
    public void ShowMessage(string text, bool bar = false)
    {
        _message = (text, ElapsedMs, bar);
        Log($"message: {text}");
    }

    /// <summary>The message, if it's shown in the bar (<paramref name="inBar"/>) or over the view (not).</summary>
    void DrawMessage(SKCanvas canvas, bool inBar)
    {
        if (_message is not { } m || (m.Bar && !TitleShowing) != inBar) return;
        double age = ElapsedMs - m.At;
        double shown = m.Bar ? BarMessageMs : MessageMs;
        if (age > shown) { _message = null; return; }
        // Fades in and out over a quarter second.
        double alpha = Math.Min(1, Math.Min(age, shown - age) / 250);
        using var font = new SKFont(MenuTypeface, 16);
        float w = font.MeasureText(m.Text) + 32, h = 34;
        // Below the backpack, when it's open; or centred in the bar (stage y 425 to 475).
        float top = inBar ? 450 - h / 2 : 150;
        var box = new SKRect(385 - w / 2, top, 385 + w / 2, top + h);
        byte a = (byte)Math.Round(alpha * 255);
        using var fill = new SKPaint { Color = new SKColor(0x22, 0x22, 0x22, a), IsAntialias = true };
        using var stroke = new SKPaint { Color = new SKColor(0x99, 0x99, 0x99, a), Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, IsAntialias = true };
        using var text = new SKPaint { Color = new SKColor(0xFF, 0x99, 0x00, a), IsAntialias = true };
        canvas.DrawRect(box, fill);
        canvas.DrawRect(box, stroke);
        canvas.DrawText(m.Text, box.Left + 16, box.MidY + 6, font, text);
    }

    /// <summary>Drawn like the settings dialog's buttons: grey box and border, orange label (white when pointed at).</summary>
    void DrawSkip(SKCanvas canvas, Render.RenderContext ctx, SKMatrix baseMatrix)
    {
        canvas.DrawRect(SkipRect, _skipBack);
        canvas.DrawRect(SkipRect, _skipBorder);
        bool lit = SkipRect.Contains(MouseStage);
        var paint = lit ? _skipTextLit : _skipText;
        const float arrow = 6, gap = 5;
        float y = SkipRect.MidY, x, textWidth;
        if (SkipLabel() is var (normal, litText))
        {
            textWidth = normal.Bounds.Width;
            x = SkipRect.MidX - (textWidth + gap + arrow) / 2;
            // Baseline so the capitals sit centred (cap height is about 0.8 em).
            ctx.DrawText(lit ? litText : normal, SKMatrix.CreateTranslation(x, y + (float)normal.Doc.Runs[0].Size * 0.4f), CxForm.Identity);
            canvas.SetMatrix(baseMatrix);
        }
        else
        {
            textWidth = _skipFont.MeasureText(Lang.T("Skip"));
            x = SkipRect.MidX - (textWidth + gap + arrow) / 2;
            canvas.DrawText(Lang.T("Skip"), x, y + 4.5f, SKTextAlign.Left, _skipFont, paint);
        }
        using var tri = new SKPath();
        tri.MoveTo(x + textWidth + gap, y - 4.5f);
        tri.LineTo(x + textWidth + gap + arrow, y);
        tri.LineTo(x + textWidth + gap, y + 4.5f);
        tri.Close();
        canvas.DrawPath(tri, paint);
    }

    // ---- Rendering ----

    readonly Dictionary<(MorphAsset, int), (float Ratio, SKPath Path)> _morphPaths = [];

    /// <summary>Interpolated morph path, cached per part until the ratio changes.</summary>
    public SKPath MorphPath(MorphAsset m, int part, float ratio)
    {
        if (_morphPaths.TryGetValue((m, part), out var c) && c.Ratio == ratio) return c.Path;
        c.Path?.Dispose();
        var path = m.Build(m.Parts[part], ratio);
        _morphPaths[(m, part)] = (ratio, path);
        return path;
    }

    /// <param name="t">Smooth mode: how far (0..1) the display is from the previous game frame to the current one.</param>
    public void Render(SKCanvas canvas, int width, int height, float t = 1)
    {
        RenderView(canvas, width, height, t);
        // Starting: drawn (so its vector sand gets made) but hidden until ready.
        if (!_started && !_capturing && !_picturing && StartupHidden())
        {
            canvas.ResetMatrix();
            canvas.Clear(SKColors.Black);
        }
    }

    void RenderView(SKCanvas canvas, int width, int height, float t = 1)
    {
        if (_captureRequested && !_capturing && !_picturing) CaptureTransition(canvas, width, height, t);
        if (_pictureAt > 0 && TickCount >= _pictureAt && !_capturing && !_picturing) TakeSlotPicture(canvas, width, height, t);
        var bg = Levels.TryGetValue(0, out var l0) ? Geo.Color(l0.Movie.Doc.Background) : SKColors.White;
        canvas.Clear(SKColors.Black);
        var (scale, ox, oy) = Viewport(width, height);
        var stage = new SKRect(ox, oy, ox + StageWidth * scale, oy + StageHeight * scale);
        var visible = stage;
        _windowSize = (width, height);
        PlaceTitleCorners();
        DisplayObject? scene = null;
        var sceneClip = stage;
        if (Widescreen)
        {
            // The whole view is shown (menu bars reach the window edges); the scene only as far as its artwork reaches.
            visible = new SKRect(0, stage.Top, width, stage.Bottom);
            scene = SceneRoot();
            if (scene != null)
            {
                // Measured once per game frame (it costs a few ms). In smooth mode the edge then slides from the
                // previous frame's to this frame's, along with the scenery.
                if (_extentTick != TickCount || _extentSize != (width, height) || _extentScene != scene)
                {
                    long started = System.Diagnostics.Stopwatch.GetTimestamp();
                    bool continues = _extentScene == scene && _extentSize == (width, height) && _extentTick == TickCount - 1;
                    // Cutscenes and minigames can't be panned and may keep actors waiting off-stage, and the telescope's
                    // frame ends at the stage edges: these keep the original stage width.
                    string sceneName = (scene as MovieClip)?.Movie.Name ?? "";
                    // (Unless a scene's fix file lets it show its artwork wider: "widescreen": true.)
                    var measured = SceneKeepsStage(scene) || !SceneShowsWide(scene) &&
                                   (sceneName.StartsWith("Episode", StringComparison.OrdinalIgnoreCase) || sceneName.StartsWith("Game", StringComparison.OrdinalIgnoreCase) ||
                                   sceneName.Equals("II.4A", StringComparison.OrdinalIgnoreCase)) // the telescope: its eyepiece frame is 4:3
                        ? (0f, (float)StageWidth)
                        // The Charred Forest maze and the Ga-Koro mural (II.4B) are seamless loops that scroll forever.
                        : ScrollsForever(sceneName) ? (ViewLeft, ViewRight)
                        // Panoramas the pan limits keep filled are shown whole: where a fixed backdrop ends a little
                        // short, its gap is a small dark corner, not a strip cut off down the whole view.
                        // A view set by hand: as far as given (else the whole view, over a backdrop colour).
                        : SceneView(scene) is { } v && (v.Left != null || v.Right != null || v.Backdrop != null)
                            ? (Math.Max(ViewLeft, v.Left ?? ViewLeft), Math.Min(ViewRight, v.Right ?? ViewRight))
                        : PanManaged(scene) || _noClamp ? (ViewLeft, ViewRight)
                        : SceneryExtent(scene, 1);
                    _extentPrev = continues ? _extent : measured;
                    _extent = measured;
                    if (_extentDebug) _extentMs.Add(System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds);
                    if (_extentDebug && (_extent.Left > ViewLeft || _extent.Right < ViewRight))
                        Console.WriteLine($"extent tick {TickCount}: {_extent.Left:0.0}..{_extent.Right:0.0} of view {ViewLeft:0.0}..{ViewRight:0.0}");
                    (_extentTick, _extentSize, _extentScene) = (TickCount, (width, height), scene);
                }
                float k = Smooth ? t : 1;
                float left = _extentPrev.Left + (_extent.Left - _extentPrev.Left) * k;
                float right = _extentPrev.Right + (_extent.Right - _extentPrev.Right) * k;
                sceneClip = new SKRect(Math.Max(0, ox + left * scale), stage.Top, Math.Min(width, ox + right * scale), stage.Bottom);
            }
        }
        canvas.Save();
        canvas.ClipRect(visible);
        canvas.DrawRect(stage, new SKPaint { Color = bg });
        if (SceneView(scene).Backdrop is SKColor backdrop) canvas.DrawRect(sceneClip, new SKPaint { Color = backdrop });
        var baseMatrix = SKMatrix.CreateScaleTranslation(scale, scale, ox, oy);
        if (Mnog.Engine.Render.RenderContext.VectorImageReady)
        {
            Mnog.Engine.Render.RenderContext.VectorImageReady = false;
            _layerCache.Clear();
        }
        _layerCache.BeginFrame(TickCount);
        var ctx = new Render.RenderContext(canvas, baseMatrix, this, Smooth ? t : 1) { Cache = UseLayerCache ? _layerCache : null };
        if (Widescreen)
        {
            // The scene shows as far as its artwork reaches; the rest of the game (its menus, and objects parked
            // off-stage) stays within the original stage. Only the overlay (this port's wide menu bar) uses the full width.
            if (scene?.Parent is MovieClip sceneParent)
                foreach (var c in sceneParent.Children.Values)
                {
                    // The game's cursor and the backpack (whose items are dragged around) follow the mouse anywhere;
                    // the flute's backdrop is widened to the view's edges (Patches.FluteBackdrop).
                    if (c.Name is { } n && (n.Equals("mousedrag", StringComparison.OrdinalIgnoreCase) || n.Equals("Backpack", StringComparison.OrdinalIgnoreCase) ||
                                            n.Equals("Flute", StringComparison.OrdinalIgnoreCase))) continue;
                    // The title's sand strip (below its beach) is as wide as the beach, and its UI reaches the view's
                    // corners (PlaceTitleCorners).
                    bool wide = c == scene || sceneParent.Level == TitleLevel && c.Name is "sand" or "ui";
                    ctx.Clips[c] = wide ? sceneClip : stage;
                }
            else if (scene != null) ctx.Clips[scene] = sceneClip;
            foreach (var level in Levels.Values)
                if (!ctx.Clips.ContainsKey(level) && level != scene?.Parent && level.Movie.Name != "overlay") ctx.Clips[level] = stage;
        }
        // Below the top of the bottom bar (stage y 425): the bar's black box once hid everything of the levels under it
        // there. It now starts under the vectorized bar's open top edge so the scene's own art shows through that gap;
        // all else (dialog answers still scrolling up, menus) stays cut off there as it was hidden.
        var overlayLevel = Levels.FirstOrDefault(kv => kv.Value.Movie.Name == "overlay");
        if (overlayLevel.Value != null)
        {
            float barTop = oy + 425 * scale;
            var whole = new SKRect(-1e6f, -1e6f, 1e6f, 1e6f);
            SKRect Cut(SKRect r) => new(r.Left, r.Top, r.Right, Math.Min(r.Bottom, barTop));
            var barScene = scene ?? SceneRoot();
            foreach (var (n, level) in Levels)
            {
                if (n >= overlayLevel.Key) continue;
                if (barScene?.Parent == level && level is MovieClip holder)
                {
                    foreach (var c in holder.Children.Values)
                        if (c != barScene && !(level.Level == TitleLevel && c.Name == "sand"))
                            ctx.Clips[c] = Cut(ctx.Clips.TryGetValue(c, out var r) ? r : whole);
                }
                else ctx.Clips[level] = Cut(ctx.Clips.TryGetValue(level, out var r) ? r : whole);
            }
            // The dialogs are loaded into the scene itself (/holder/TextSub): their answer rows scroll up from under the bar.
            void CutDialogs(DisplayObject o, int depth)
            {
                if (o is not MovieClip mc) return;
                foreach (var c in mc.Children.Values)
                {
                    if (c is MovieClip { LoadedRoot: true, Movie.Name: var mn } && (mn.Equals("TextSub", StringComparison.OrdinalIgnoreCase) || mn.Equals("MiniSub", StringComparison.OrdinalIgnoreCase)))
                        ctx.Clips[c] = Cut(ctx.Clips.TryGetValue(c, out var r) ? r : whole);
                    else if (depth < 3) CutDialogs(c, depth + 1);
                }
            }
            if (barScene != null) CutDialogs(barScene, 0);
        }
        foreach (var level in Levels.Values)
        {
            if (!level.Visible) continue;
            bool clip = ctx.Clips.TryGetValue(level, out var levelClip);
            if (clip) { canvas.Save(); canvas.ResetMatrix(); canvas.ClipRect(levelClip); }
            level.Render(ctx, level.DrawMatrix(ctx.T), level.DrawColor(ctx.T));
            if (clip) canvas.Restore();
        }
        if (!_capturing && !_picturing) DrawTransition(canvas, width, height);
        if (!_picturing && TitleShowing) { canvas.Save(); canvas.SetMatrix(baseMatrix); DrawTitleSlot(canvas); canvas.Restore(); }
        if (!_picturing) { canvas.Save(); canvas.SetMatrix(baseMatrix); DrawSettingsHint(canvas); canvas.Restore(); }
        if (!_picturing && (SkipVisible || Warp != null || SlotMenu != null || Ending != null || AudioSettings != null || Help != null || Wallpaper != null || Music != null || _message != null))
        {
            canvas.Save();
            canvas.SetMatrix(baseMatrix);
            if (SkipVisible) DrawSkip(canvas, ctx, baseMatrix);
            Warp?.Draw(canvas);
            Ending?.Draw(canvas);
            AudioSettings?.Draw(canvas);
            Help?.Draw(canvas);
            Wallpaper?.Draw(canvas);
            Music?.Draw(canvas);
            SlotMenu?.Draw(canvas);
            DrawMessage(canvas, inBar: false);
            canvas.Restore();
        }
        // Not over the title screen, whose buttons reach down to where the panel is.
        if (Debug && !TitleShowing && !_picturing)
        {
            var overlay = _overlay ??= new DebugOverlay(this);
            overlay.FrameDrawn();
            canvas.Save();
            canvas.SetMatrix(baseMatrix);
            overlay.Draw(canvas, Widescreen ? ViewLeft : 0, Widescreen ? ViewRight : StageWidth);
            canvas.Restore();
        }
        // (A message in the bar: over the debug panel, which fills the bar.)
        if (!_picturing && _message != null)
        {
            canvas.Save();
            canvas.SetMatrix(baseMatrix);
            DrawMessage(canvas, inBar: true);
            canvas.Restore();
        }
        canvas.Restore();
    }


    int _extentTick = -1;
    public readonly List<double> _extentMs = [];
    static readonly bool _extentDebug = Environment.GetEnvironmentVariable("MNOG_EXTENTDBG") != null;
    /// <summary>Debugging: MNOG_NOCLAMP turns the widescreen pan limits off (to see where the artwork ends).</summary>
    static readonly bool _noClamp = Environment.GetEnvironmentVariable("MNOG_NOCLAMP") != null;
    (int, int) _extentSize;
    DisplayObject? _extentScene;
    (float Left, float Right) _extent, _extentPrev;

    /// <summary>The clip holding the current scene (the player's "holder"), or the main movie when there is none.</summary>
    DisplayObject? SceneRoot()
    {
        // The title screen shows the first beach scene in its own "holder" over everything else.
        if (GetLevel(TitleLevel) is { Removed: false } title && title.ChildByName("holder") is MovieClip { Visible: true } titleScene)
            return titleScene;
        var level1 = GetLevel(1);
        if (level1?.ChildByName("holder") is MovieClip { Visible: true } holder) return holder;
        return Levels.TryGetValue(0, out var l0) && level1 == null ? l0 : null;
    }

    /// <summary>
    /// Horizontal extent (stage coordinates) the scene's artwork covers: from each stage edge outward, as far
    /// as the rows the artwork covers there (and at the stage center) stay covered. Beyond it the scene would
    /// show gaps or stray off-stage objects, so it is drawn black. Rows already open at the stage edge are
    /// gaps the original view shows too; like the pan limits (ClampScenery), they don't count.
    /// </summary>
    (float Left, float Right) SceneryExtent(DisplayObject scene, float t)
    {
        var world = scene.DrawMatrix(t);
        for (var o = scene.Parent; o != null; o = o.Parent) world = o.DrawMatrix(t).PreConcat(world);
        bool Covered(float x, float y) => Draws(scene, new SKPoint(x, y), world, t);
        var rows = SceneryRows.Where(y => Covered(StageWidth / 2f, y)).ToList();
        if (rows.Count == 0) return (0, StageWidth);
        const float step = 4;
        float Reach(float from, float to)
        {
            float dir = Math.Sign(to - from);
            var side = rows.Where(y => Covered(from - dir, y)).ToList(); // a pixel inside the stage edge
            if (side.Count == 0) return from;
            // A column counts as scenery if nearly all rows are covered: small gaps (seams between the Charred
            // Forest's panels) are part of the artwork and also show inside the stage.
            int allowedGaps = side.Count / 6;
            bool Column(float x)
            {
                int gaps = 0;
                foreach (var y in side)
                    if (!Covered(x, y) && ++gaps > allowedGaps) return false;
                return true;
            }
            // Usually the artwork reaches the view edge: check there (half a pixel inside, as ClampScenery does)
            // and halfway first. Otherwise find where it ends: the last covered column before the first gap.
            float last = to - dir * 0.5f;
            if (Column(last) && Column((from + last) / 2)) return to;
            float reached = from;
            for (float x = from + dir * step; dir * (last - x) > 0; x += dir * step)
            {
                // A narrow seam between pieces of artwork (a thin line in the original) doesn't end the scenery.
                if (!Column(x) && !(Column(x + dir * step) || Column(x + dir * 2 * step))) break;
                reached = x;
            }
            float lo = reached, hi = reached + dir * step;
            if (dir * (hi - last) > 0) hi = last;
            for (int it = 0; it < 4; it++)
            {
                float mid = (lo + hi) / 2;
                if (Column(mid)) lo = mid; else hi = mid;
            }
            return lo;
        }
        return (Reach(0, ViewLeft), Reach(StageWidth, ViewRight));
    }

    /// <summary>
    /// Widescreen helper for scrolling panoramas: keeps the panning inside the range where the scenery fills
    /// the wider view. The range is measured once per scene (and window width) by trying the pan positions,
    /// then simply enforced, so it never jumps or fights the game's own panning. Scenes whose artwork can't
    /// fill the view at both ends keep the original range.
    /// </summary>
    public void ClampScenery(IReadOnlyList<DisplayObject> layers, IReadOnlyList<double> rates, bool vertical = false)
    {
        if (!Widescreen || layers.Count == 0 || _windowSize.W == 0 || _noClamp) return;
        // The Charred Forest scrolls its own looping panels (entered from Ta-Koro, the game's panning stays on too).
        if (SceneRoot() is MovieClip { } sceneClip && ScrollsForever(sceneClip.Movie.Name)) return;
        int k = 0;
        for (int i = 0; i < rates.Count; i++) if (rates[i] == 1) k = i;
        var anchor = layers[k];
        // How the layers sit relative to each other (constant while panning; set when a scene loads).
        double Pos(DisplayObject l) => vertical ? l.Y : l.X;
        double layout = 0;
        for (int i = 0; i < layers.Count; i++) layout += (Pos(layers[i]) - rates[i] / rates[k] * Pos(anchor)) * (i + 1);
        if (_panAnchor != anchor || _panView != ViewRight || _panVertical != vertical || Math.Abs(_panLayout - layout) > 5)
        {
            long started = System.Diagnostics.Stopwatch.GetTimestamp();
            // Up and down, the extra width shows the same sides at every height: scrolling can't hide a gap there,
            // and limiting it only cuts the scroll short. The game's own scrolling is left alone (unless set by hand).
            (_panMin, _panMax) = vertical ? (double.NegativeInfinity, double.PositiveInfinity) : PanLimits(layers, rates, k, vertical);
            _panVertical = vertical;
            // A range set by hand for the scene (assets/fixes, written as the movie's pan-limits.json) replaces it.
            if (anchor.Parent is MovieClip { } sceneMovie && _movieDirs.TryGetValue(sceneMovie.Movie.Name, out var sceneDir) &&
                File.Exists(Path.Combine(sceneDir, "pan-limits.json")) &&
                System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(Path.Combine(sceneDir, "pan-limits.json"))) is System.Text.Json.Nodes.JsonObject set)
            {
                if (set["Min"]?.GetValue<double>() is double setMin) _panMin = setMin;
                if (set["Max"]?.GetValue<double>() is double setMax) _panMax = setMax;
                // One end alone on a scene the measurement found no range for: the game's own range for the other.
                if (double.IsInfinity(_panMin) && !double.IsInfinity(_panMax)) _panMin = vertical ? GameVerticalPanMin : GamePanMin;
                if (double.IsInfinity(_panMax) && !double.IsInfinity(_panMin)) _panMax = vertical ? GameVerticalPanMax : GamePanMax;
            }
            if (_extentDebug) Console.WriteLine($"pan limits measured in {System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds:0.0} ms (layout {layout:0.0})");
            (_panAnchor, _panView, _panLayout) = (anchor, ViewRight, layout);
            if (_extentDebug) Console.WriteLine($"pan limits for {anchor.SlashPath}: {_panMin:0.0}..{_panMax:0.0}");
        }
        double p = Pos(anchor), target = Math.Clamp(p, _panMin, _panMax);
        if (target == p) return;
        double d = (target - p) / rates[k];
        for (int i = 0; i < layers.Count; i++)
            if (vertical) layers[i].Y += rates[i] * d; else layers[i].X += rates[i] * d;
    }

    bool _panVertical;

    /// <summary>Whether the scene is a panorama whose panning the limits keep within its scenery.</summary>
    /// <summary>Scenes that loop endlessly sideways (no ends to keep in view): no pan limits, shown the full width.</summary>
    static bool ScrollsForever(string scene) =>
        scene.StartsWith("III.3", StringComparison.OrdinalIgnoreCase) || scene.Equals("II.4B", StringComparison.OrdinalIgnoreCase);

    bool PanManaged(DisplayObject scene) => _panAnchor is { Removed: false } a && a.Parent == scene && !double.IsInfinity(_panMin);

    DisplayObject? _panAnchor;
    float _panView;
    double _panLayout;
    double _panMin = double.NegativeInfinity, _panMax = double.PositiveInfinity;

    /// <summary>The player's own pan range for the Middle-Ground layer (its frame-22 script): sideways and up/down.</summary>
    const double GamePanMin = 30, GamePanMax = 740, GameVerticalPanMin = 30, GameVerticalPanMax = 395;

    /// <summary>Rows the scenery is sampled on: down to the bottom of the view the menu bar leaves (425).</summary>
    static readonly float[] SceneryRows = [.. Enumerable.Range(0, 26).Select(i => 12f + 16 * i), 422f];

    (double Min, double Max) PanLimits(IReadOnlyList<DisplayObject> layers, IReadOnlyList<double> rates, int k, bool vertical)
    {
        var none = (double.NegativeInfinity, double.PositiveInfinity);
        double Pos(DisplayObject l) => vertical ? l.Y : l.X;
        var start = layers.Select(Pos).ToArray();
        double p0 = start[k];
        void MoveTo(double q)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                double v = start[i] + rates[i] * (q - p0) / rates[k];
                if (vertical) layers[i].Y = v; else layers[i].X = v;
            }
        }
        // Coverage by the whole scene, as the drawn edge (SceneryExtent) measures it: some scenes keep a fixed
        // backdrop behind the moving layers, which may end short of the wider view.
        var scene = layers[k].Parent;
        bool Covered(float x, float y)
        {
            var p = new SKPoint(x, y);
            if (scene != null) return Draws(scene, p, scene.WorldMatrix);
            foreach (var l in layers)
                if (l.Visible && Draws(l, p, l.WorldMatrix)) return true;
            return false;
        }
        // A gap next to near-black artwork can't be told from it (the view beyond the scenery is black too).
        bool Dark(float x, float y) => scene != null && ColorAt(scene, new SKPoint(x, y), scene.WorldMatrix) is { } c &&
                                       c.Alpha > 200 && 0.299 * c.Red + 0.587 * c.Green + 0.114 * c.Blue < 60;
        // Whether the scenery fills one side of the wider view: every row the artwork fills at the original stage
        // edge stays filled out to the view edge (or ends in a gap next to dark artwork, which can't be seen).
        // Gaps the original view shows too are left alone.
        bool SideFilled(double q, bool right)
        {
            MoveTo(q);
            // A unit past the view edge, so the scenery doesn't stop a pixel short of it.
            float edge = right ? ViewRight + 1 : ViewLeft - 1;
            float inner = right ? StageWidth - 1 : 1;
            foreach (var y in SceneryRows)
            {
                if (!Covered(StageWidth / 2f, y) || !Covered(inner, y)) continue;
                float last = inner;
                for (int s = 1; s <= 3; s++)
                {
                    float x = inner + (edge - inner) * s / 3;
                    if (Covered(x, y)) { last = x; continue; }
                    if (!Dark(last, y))
                    {
                        if (Environment.GetEnvironmentVariable("MNOG_LIMITDBG") != null)
                            Console.WriteLine($"DBG q={q:0.0} {(right ? "right" : "left")} row {y} gap at {x:0.0}, last {last:0.0} colour {(scene != null ? ColorAt(scene, new SKPoint(last, y), scene.WorldMatrix) : null)}");
                        return false;
                    }
                    break;
                }
            }
            return true;
        }
        bool Filled(double q) => SideFilled(q, true) && SideFilled(q, false);
        // Sideways, panning right lowers the position (toward GamePanMin) and can run out of scenery on the right;
        // panning left raises it and can run out on the left. Up and down, either end can run out on both sides.
        // From each end, find where the view is first filled.
        double lo = Math.Min(vertical ? GameVerticalPanMin : GamePanMin, p0), hi = Math.Max(vertical ? GameVerticalPanMax : GamePanMax, p0);
        double Find(bool fromLow)
        {
            bool Ok(double q) => vertical ? Filled(q) : SideFilled(q, fromLow);
            double from = fromLow ? lo : hi, to = fromLow ? hi : lo, dir = fromLow ? 1 : -1, step = vertical ? 5 : 10;
            if (Ok(from)) return from;
            for (double q = from + dir * step; dir * (to - q) >= 0; q += dir * step)
            {
                if (!Ok(q)) continue;
                double bad = q - dir * step, good = q;
                for (int it = 0; it < 6; it++)
                {
                    double mid = (bad + good) / 2;
                    if (Ok(mid)) good = mid; else bad = mid;
                }
                return good;
            }
            return double.NaN;
        }
        try
        {
            double min = Find(true), max = Find(false);
            if (double.IsNaN(min) || double.IsNaN(max) || min > max) return none;
            return (min, max);
        }
        finally
        {
            MoveTo(p0);
        }
    }

    /// <summary>
    /// The colour of the front-most artwork at a stage point (a gradient's average, ignoring colour transforms),
    /// or null where there's none or it's a bitmap.
    /// </summary>
    static SKColor? ColorAt(DisplayObject o, SKPoint p, SKMatrix world)
    {
        switch (o)
        {
            case MovieClip mc:
                foreach (var c in mc.Children.Values.Reverse())
                    if (c.Visible && c.ClipDepth == 0 && ColorAt(c, p, world.PreConcat(c.Matrix)) is { } found) return found;
                return null;
            case GraphicObject { Asset: ShapeAsset s } when world.TryInvert(out var inv):
                var local = inv.MapPoint(p);
                if (!s.Bounds.Contains(local)) return null;
                for (int i = s.Parts.Count - 1; i >= 0; i--)
                {
                    var part = s.Parts[i];
                    if (part.Fill is not { } fill || !part.Path.Bounds.Contains(local) || !part.Path.Contains(local.X, local.Y)) continue;
                    if (fill.Type == "solid") return Geo.Color(fill.Color);
                    if (fill.Stops is { Count: > 0 } stops)
                    {
                        var cs = stops.Select(st => Geo.Color(st.Color)).ToList();
                        return new SKColor((byte)cs.Average(c => c.Red), (byte)cs.Average(c => c.Green), (byte)cs.Average(c => c.Blue), (byte)cs.Average(c => c.Alpha));
                    }
                    return null;
                }
                return null;
            default:
                return null;
        }
    }

    /// <summary>Debugging: the shapes (and which of their filled parts) drawn at a stage point.</summary>
    public List<string> ArtAt(SKPoint p)
    {
        var found = new List<string>();
        void Walk(DisplayObject o, SKMatrix world)
        {
            if (!o.Visible) return;
            if (o is MovieClip mc) { foreach (var c in mc.Children.Values) Walk(c, world.PreConcat(c.DrawMatrix(1))); return; }
            if (o is GraphicObject { Asset: ShapeAsset s } g && world.TryInvert(out var inv))
            {
                var local = inv.MapPoint(p);
                for (int i = 0; i < s.Parts.Count; i++)
                    if (s.Parts[i].Fill is { } f && s.Parts[i].Path.Contains(local.X, local.Y))
                        found.Add($"{g.SlashPath} {s.Movie.Name}#{s.Id} part {i} {f.Type} {f.Color} local {local.X:0.0},{local.Y:0.0}");
            }
        }
        foreach (var level in Levels.Values) Walk(level, level.Matrix);
        return found;
    }

    /// <summary>Whether visible artwork (shapes, text, images) is drawn at a stage point.</summary>
    static bool Draws(DisplayObject o, SKPoint p, SKMatrix world, float t = 1)
    {
        switch (o)
        {
            case MovieClip mc:
                foreach (var c in mc.Children.Values)
                    if (c.Visible && c.ClipDepth == 0 && Draws(c, p, world.PreConcat(c.DrawMatrix(t)), t)) return true;
                return false;
            case GraphicObject { Asset: ShapeAsset s } g when s.Parts.Any(part => part.Fill?.Type == "bitmap"):
                return DrawsShape(g, s, p, world);
            case GraphicObject g:
                return Mnog.Engine.Render.RenderContext.HitGraphic(g, p, world, fillsOnly: true);
            case BitmapObject:
                return o.HitTest(p, world);
            default:
                return false;
        }
    }

    /// <summary>
    /// A shape with bitmap fills draws at a point only where the image is opaque there: an image with transparent
    /// parts can fill a much larger outline than what's seen (e.g. V.2-S's islands, transparent on the left).
    /// </summary>
    static bool DrawsShape(GraphicObject g, ShapeAsset s, SKPoint p, SKMatrix world)
    {
        if (!world.TryInvert(out var inv)) return false;
        var local = inv.MapPoint(p);
        if (!s.Bounds.Contains(local)) return false;
        foreach (var part in s.Parts)
        {
            if (part.Fill is not { } fill || !part.Path.Bounds.Contains(local) || !part.Path.Contains(local.X, local.Y)) continue;
            if (fill.Type != "bitmap") return true;
            if (fill.Bitmap is int id && s.Movie.Symbol(id) is ImageAsset img && Geo.Matrix(fill.Matrix).TryInvert(out var toImage))
            {
                var px = toImage.MapPoint(local);
                if (img.AlphaAt(px.X, px.Y) >= 128) return true;
            }
        }
        return false;
    }

    /// <summary>Stage-to-window scale and offset: the whole stage fits, centered.</summary>
    (float Scale, float X, float Y) Viewport(int width, int height)
    {
        float scale = Math.Min(width / (float)StageWidth, height / (float)StageHeight);
        return (scale, (width - StageWidth * scale) / 2, (height - StageHeight * scale) / 2);
    }

    /// <summary>Maps window pixel coordinates to stage coordinates (inverse of the letterbox in Render).</summary>
    public SKPoint WindowToStage(float x, float y, int width, int height)
    {
        var (scale, ox, oy) = Viewport(width, height);
        return new SKPoint((x - ox) / scale, (y - oy) / scale);
    }
}

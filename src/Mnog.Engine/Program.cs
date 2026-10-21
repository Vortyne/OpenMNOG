using System.Globalization;
using Mnog.Engine;
using Mnog.Engine.Host;
using SkiaSharp;

// Usage:
//   Mnog.Engine [gameDir] [--windowed]            play (fullscreen, or as last left, unless --windowed)
//   Mnog.Engine [gameDir] --autopilot "<commands>" the game in its window, driven by the same commands
//   Mnog.Engine [gameDir] --run "<commands>"      headless test run; commands separated by ';':
//       wait N | click X Y | move X Y | key CODE | shot FILE.png [W H [T]] | type TEXT | widescreen 0|1 | smooth 0|1 | tree [depth] | bench N W H

// Mnog.Engine --pathop IN.json OUT.json: path clipping for the build tools (Skia's path operations): IN is
// [{"subject": svg path, "clip": svg path, "near"?: svg path, "by"?: units}], OUT the subjects' parts inside their
// clips (and, with "near", within "by" of that shape), as svg path data (the subject read even-odd, as shapes are).
if (args.Length == 3 && args[0] == "--pathop")
{
    var tasks = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(File.ReadAllText(args[1]))!;
    var results = new List<string>();
    foreach (var t in tasks)
    {
        try
        {
            using var subject = SKPath.ParseSvgPathData(t["subject"]) ?? new SKPath();
            subject.FillType = SKPathFillType.EvenOdd;
            var clip = SKPath.ParseSvgPathData(t["clip"]) ?? new SKPath();
            clip.FillType = SKPathFillType.EvenOdd;
            // ("near", "near2": within "by" of each of those shapes too.)
            foreach (var key in new[] { "near", "near2" })
            {
                if (!t.TryGetValue(key, out var nearPath) || SKPath.ParseSvgPathData(nearPath) is not { } near) continue;
                // (The shape grown by "by" all round: itself and its outline stroked twice that wide.)
                using var paint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2 * float.Parse(t["by"], CultureInfo.InvariantCulture), StrokeJoin = SKStrokeJoin.Round };
                using var outline = new SKPath();
                paint.GetFillPath(near, outline);
                using var grown = near.Op(outline, SKPathOp.Union) ?? new SKPath(near);
                var both = clip.Op(grown, SKPathOp.Intersect) ?? new SKPath();
                clip.Dispose();
                near.Dispose();
                clip = both;
            }
            // ("minus": the subject's part outside the clip instead.)
            using var result = subject.Op(clip, t.TryGetValue("op", out var op) && op == "minus" ? SKPathOp.Difference : SKPathOp.Intersect);
            clip.Dispose();
            results.Add(result?.ToSvgPathData() ?? "");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"pathop: {e.Message}");
            results.Add("");
        }
    }
    File.WriteAllText(args[2], System.Text.Json.JsonSerializer.Serialize(results));
    return 0;
}

string? gameDir = null;
string? commands = null;
string? entry = null;
string? autopilot = null;
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--run" && i + 1 < args.Length) commands = args[++i] is var r && r.StartsWith('@') ? File.ReadAllText(r[1..]) : r;
    else if (args[i] == "--autopilot" && i + 1 < args.Length) autopilot = args[++i] is var a && a.StartsWith('@') ? File.ReadAllText(a[1..]) : a;
    else if (args[i] == "--movie" && i + 1 < args.Length) entry = args[++i];
    else if (args[i].StartsWith("--")) continue;
    else gameDir = args[i];
}
gameDir ??= FindGameDir();
if (gameDir == null || !File.Exists(Path.Combine(gameDir, "game.json")))
{
    Console.Error.WriteLine("Game data not found. Pass the folder containing game.json (the game/ folder of this repository).");
    return 1;
}

var player = new Player(gameDir);
// --movie Name starts a single movie instead of the game (for testing scenes in isolation).
if (entry != null) player.Manifest.EntryMovie = entry;
if (commands != null)
{
    // Headless runs draw only when asked (shot): no startup hold.
    player.SkipStartupHold();
    RunHeadless(player, commands);
    return 0;
}

using var host = new SdlHost(player, player.Manifest.Title);
// --autopilot "<commands>": the game in its window (on the GPU), driven by test commands as if played: wait N lets N
// game frames pass; "quit" closes it. For measuring the real renderer (its frame times are logged), or to take the
// game somewhere (e.g. "scene Episode_9-1; wait 40; goto holder 2205 play") and hand it over: after the last
// command it carries on as normal.
if (autopilot != null)
{
    var queue = new Queue<string>(autopilot.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    int waiting = 0;
    host.Autopilot = () =>
    {
        while (waiting == 0)
        {
            if (!queue.TryDequeue(out var c)) { host.Autopilot = null; return true; }
            if (c == "quit") return false;
            if (c.StartsWith("wait ")) waiting = int.Parse(c[5..]);
            else RunCommand(player, c);
        }
        waiting--;
        return true;
    };
}
// The game opens fullscreen, or as it was last left (F11 / Alt+Enter or the bar's button toggle it, and the
// choice is remembered); --windowed starts in a window this time.
if (!args.Contains("--windowed") && SdlHost.SavedFullscreen()) host.Fullscreen = true;
// A crash leaves its details in logs/crash-<time>.log (next to the game data) as well as on the console (and a freeze,
// logs/hang-<time>.log: Watchdog).
AppDomain.CurrentDomain.UnhandledException += (_, e) => WriteCrash(gameDir, player, e.ExceptionObject as Exception);
try
{
    player.Start();
    // A freeze (the main loop not coming round for a few seconds) leaves logs/hang-<time>.log, as a crash does.
    Watchdog.Beat("-");
    Watchdog.Start(player, LogsDir(gameDir));
    host.Run();
}
catch (Exception e)
{
    WriteCrash(gameDir, player, e);
    throw;
}
return 0;

static void WriteCrash(string gameDir, Player player, Exception? e)
{
    try
    {
        var dir = LogsDir(gameDir);
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, $"crash-{DateTime.Now:yyyyMMdd-HHmmss}.log");
        var scene = (player.GetLevel(1)?.ChildByName("holder") as Mnog.Engine.Display.MovieClip) is { } h ? $"{h.Movie.Name} frame {h.CurrentFrame}" : "-";
        var lines = new List<string> { $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  tick {player.TickCount}  scene {scene}", "Recent log:" };
        lines.AddRange(player.RecentLog.Select(l => "  " + l));
        lines.Add("");
        lines.Add(e?.ToString() ?? "(no exception)");
        var text = string.Join(Environment.NewLine, lines);
        File.WriteAllText(file, text);
        Console.Error.WriteLine($"Crashed; details saved to {file}");
    }
    catch (IOException) { }
}

/// <summary>Where crash and hang logs go: logs/ next to the game data (beside OpenMNOG.exe in a release).</summary>
static string LogsDir(string gameDir) => Path.Combine(Path.GetFullPath(Path.Combine(gameDir, "..")), "logs");

static string? FindGameDir()
{
    foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
    {
        for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
        {
            foreach (var candidate in new[] { Path.Combine(dir.FullName, "game"), Path.Combine(dir.FullName, "build", "game") })
                if (File.Exists(Path.Combine(candidate, "game.json"))) return candidate;
        }
    }
    return null;
}

static void RunHeadless(Player player, string commands)
{
    player.Start();
    // (MNOG_WATCHDOG=1: the freeze watchdog on, each command counting as a frame; "freeze N" tests it.)
    bool watch = Environment.GetEnvironmentVariable("MNOG_WATCHDOG") == "1";
    if (watch) Watchdog.Start(player, LogsDir(player.GameDir));
    foreach (var raw in commands.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        if (watch) { Watchdog.Beat(player.CurrentSceneName ?? "-"); Watchdog.Phase($"test command: {raw}"); }
        RunCommand(player, raw);
    }
}

/// <summary>One test command (see the usage above); wait N plays N game frames.</summary>
static void RunCommand(Player player, string raw)
{
    {
        var p = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        float F(int i) => float.Parse(p[i], CultureInfo.InvariantCulture);
        switch (p[0])
        {
            case "hover":
                // hover: what the mouse is over (as the debug panel says it).
                Console.WriteLine($"hover {player.MouseStage.X:0},{player.MouseStage.Y:0}: {player.HoverDescription}");
                break;
            case "freeze":
                // freeze MS: the game stuck that long (tests the watchdog: MNOG_WATCHDOG=1).
                using (Watchdog.Wait("the freeze test command"))
                    System.Threading.Thread.Sleep(int.Parse(p[1]));
                break;
            case "wait":
                for (int i = 0; i < int.Parse(p[1]); i++)
                {
                    player.Tick();
                    // Mix a frame's worth of audio too (as the host's audio callback would), so sound problems show.
                    if (MixAudio)
                    {
                        var mixed = new float[44100 / 18 * 2];
                        player.Audio.Mix(mixed);
                        // (MNOG_AUDIODUMP=file: the mixed audio appended there, raw 44.1 kHz stereo float, to measure.)
                        if (Environment.GetEnvironmentVariable("MNOG_AUDIODUMP") is { Length: > 0 } dump)
                            using (var f = new FileStream(dump, FileMode.Append))
                                f.Write(System.Runtime.InteropServices.MemoryMarshal.AsBytes(mixed.AsSpan()));
                    }
                }
                break;
            case "audio":
                MixAudio = p[1] == "1";
                break;
            case "move":
                player.OnMouseMove(new SKPoint(F(1), F(2)));
                break;
            case "press":
                // press X Y: the mouse moved there and held down (a button's down state), not released.
                player.OnMouseMove(new SKPoint(F(1), F(2)));
                player.OnMouseDown();
                break;
            case "click":
                player.OnMouseMove(new SKPoint(F(1), F(2)));
                player.OnMouseDown();
                player.Tick();
                player.OnMouseUp();
                break;
            case "key":
                // key CODE [BUTTONCODE]: a key pressed and let go (BUTTONCODE: as buttons' keyPress events see it).
                player.OnKeyDown(int.Parse(p[1]), p.Length > 2 ? int.Parse(p[2]) : 0);
                player.OnKeyUp(int.Parse(p[1]));
                break;
            case "keydown":
                // keydown CODE / keyup CODE: hold a key across frames (Key.isDown).
                player.OnKeyDown(int.Parse(p[1]), 0);
                break;
            case "vector":
                // vector 0|1: Misc > Vector sand.
                player.FsCommand("vectorsand", p[1]);
                break;
            case "turn":
                // turn: Left Shift's turn-around.
                player.TurnAround();
                break;
            case "keyup":
                player.OnKeyUp(int.Parse(p[1]));
                break;
            case "playframes":
                {
                    // playframes FILE W H: the scene (/holder) played through as the game plays it (its animations
                    // running, unlike "frames", which holds each frame with its clips at their start), drawn every tick
                    // and appended to FILE as raw RGB, with each capture's timeline frame in FILE.idx. Where it stops
                    // to wait for a click, it's moved on a frame; it ends at its last frame or when it hands over.
                    int w = int.Parse(p[2]), h = int.Parse(p[3]);
                    // (Optional FROM TO: only the captures of those timeline frames are kept.)
                    int keepFrom = p.Length > 5 ? int.Parse(p[4]) : 1, keepTo = p.Length > 5 ? int.Parse(p[5]) : int.MaxValue;
                    if (player.GetLevel(1)?.ChildByName("holder") is not Mnog.Engine.Display.MovieClip holder) { Console.WriteLine("playframes: no /holder"); break; }
                    using var surface = SKSurface.Create(new SKImageInfo(w, h, SKColorType.Rgb888x, SKAlphaType.Opaque));
                    using var outFile = File.Create(p[1]);
                    using var idx = new StreamWriter(p[1] + ".idx");
                    var row = new byte[w * h * 3];
                    int n = 0, still = 0, last = -1;
                    for (int tick = 0; tick < 20000; tick++)
                    {
                        if (player.GetLevel(1)?.ChildByName("holder") != holder || holder.Removed) break;
                        if (holder.CurrentFrame >= keepFrom && holder.CurrentFrame <= keepTo)
                        {
                            if (Environment.GetEnvironmentVariable("MNOG_UNBAR") == "1") Mnog.Engine.LiveBackdrop.HideLetterbox(holder, 0);
                            surface.Canvas.Clear(SKColors.Black);
                            // (MNOG_PLAYT: drawn that far (0..1) between the previous and current frame, as smoothing draws it.)
                            player.Render(surface.Canvas, w, h, float.TryParse(Environment.GetEnvironmentVariable("MNOG_PLAYT"), NumberStyles.Float, CultureInfo.InvariantCulture, out var playT) ? playT : 1);
                            using (var pix = surface.PeekPixels())
                            {
                                var span = pix.GetPixelSpan();
                                for (int i = 0, j = 0; i < w * h; i++, j += 4) { row[i * 3] = span[j]; row[i * 3 + 1] = span[j + 1]; row[i * 3 + 2] = span[j + 2]; }
                            }
                            outFile.Write(row);
                            idx.WriteLine(holder.CurrentFrame);
                            n++;
                        }
                        if (holder.CurrentFrame > keepTo) break;
                        if (holder.CurrentFrame >= holder.FrameCount) break;
                        still = holder.CurrentFrame == last ? still + 1 : 0;
                        last = holder.CurrentFrame;
                        // Waiting (stopped for a click, or a loop that doesn't move on): on a frame.
                        if (still >= 18) { holder.GotoFrame(holder.CurrentFrame + 1); holder.Play(); still = 0; }
                        player.Tick();
                    }
                    Console.WriteLine($"playframes: {n} captures of {holder.Movie.Name} ({holder.FrameCount}) to {p[1]}");
                    break;
                }
            case "wallsvg":
                {
                    // wallsvg SLOT N FILE: a chronicle slot's Nth wallpaper saved as an SVG (1920x1080), and as FILE.png.
                    var shots = player.WallpaperFrames(int.Parse(p[1]));
                    using var svgFile = File.Create(p[3]);
                    using var img = player.RenderWallpaper(int.Parse(p[1]), 1920, 1080, shots[Math.Clamp(int.Parse(p[2]) - 1, 0, shots.Count - 1)], svgFile);
                    if (img != null) { using var data = img.Encode(SKEncodedImageFormat.Png, 100); File.WriteAllBytes(p[3] + ".png", data.ToArray()); }
                    Console.WriteLine($"wallsvg {p[1]} {p[2]}: {(img != null ? p[3] : "none")}");
                    break;
                }
            case "titlesvg":
                {
                    // titlesvg FILE W H: the title screen's scenery, without its logo and menu, saved as an SVG.
                    using var svgFile = File.Create(p[1]);
                    bool saved = player.SaveTitleSvg(svgFile, int.Parse(p[2]), int.Parse(p[3]));
                    Console.WriteLine($"titlesvg: {(saved ? p[1] : "no title screen")}");
                    break;
                }
            case "useslot":
                // useslot N [title]: switch save slot as the Save Slot menu does (in game: the slot loaded).
                player.UseSaveSlot(int.Parse(p[1]), load: !(p.Length > 2 && p[2] == "title"));
                break;
            case "totitle":
                // totitle: leave the game for the title screen (as the ending prompt and the quit menu do).
                player.ReturnToTitle();
                break;
            case "titlelayers":
                {
                    // titlelayers DIR W H: the title scenery in layers, with each frame of its water (Player.SaveTitleLayers).
                    Directory.CreateDirectory(p[1]);
                    bool saved = player.SaveTitleLayers(p[1], int.Parse(p[2]), int.Parse(p[3]));
                    Console.WriteLine($"titlelayers: {(saved ? p[1] : "no title screen")}");
                    break;
                }
            case "wallshot":
                {
                    // wallshot SLOT N FILE [W H]: a chronicle slot's Nth wallpaper (wallpapers.json) saved as a PNG.
                    var shots = player.WallpaperFrames(int.Parse(p[1]));
                    int w = p.Length > 5 ? int.Parse(p[4]) : 960, h = p.Length > 5 ? int.Parse(p[5]) : 540;
                    using var img = player.RenderWallpaper(int.Parse(p[1]), w, h, shots[Math.Clamp(int.Parse(p[2]) - 1, 0, shots.Count - 1)]);
                    if (img != null) { using var data = img.Encode(SKEncodedImageFormat.Png, 100); File.WriteAllBytes(p[3], data.ToArray()); }
                    Console.WriteLine($"wallshot {p[1]} {p[2]}: {(img != null ? p[3] : "none")}");
                    break;
                }
            case "frames":
                {
                    // frames FILE FROM TO STEP W H: the scene (/holder) held at each of those frames and drawn, appended to
                    // FILE as raw RGB (W*H*3 bytes a frame), for matching stills (the chronicle's pictures) to their frames.
                    int from = int.Parse(p[2]), to = int.Parse(p[3]), step = int.Parse(p[4]), w = int.Parse(p[5]), h = int.Parse(p[6]);
                    if (player.GetLevel(1)?.ChildByName("holder") is not Mnog.Engine.Display.MovieClip holder) { Console.WriteLine("frames: no /holder"); break; }
                    using var surface = SKSurface.Create(new SKImageInfo(w, h, SKColorType.Rgb888x, SKAlphaType.Opaque));
                    using var outFile = File.Create(p[1]);
                    var row = new byte[w * h * 3];
                    int n = 0;
                    for (int f = from; f <= Math.Min(to, holder.FrameCount); f += step)
                    {
                        holder.GotoFrame(f);
                        holder.Playing = false;
                        // (MNOG_UNBAR=1: the episode's letterbox left out, as the chronicle's live pictures show it.)
                        if (Environment.GetEnvironmentVariable("MNOG_UNBAR") == "1") Mnog.Engine.LiveBackdrop.HideLetterbox(holder, 0);
                        surface.Canvas.Clear(SKColors.Black);
                        player.Render(surface.Canvas, w, h, 1);
                        using var pix = surface.PeekPixels();
                        var span = pix.GetPixelSpan();
                        for (int i = 0, j = 0; i < w * h; i++, j += 4) { row[i * 3] = span[j]; row[i * 3 + 1] = span[j + 1]; row[i * 3 + 2] = span[j + 2]; }
                        outFile.Write(row);
                        n++;
                    }
                    Console.WriteLine($"frames: {n} of {holder.Movie.Name} ({holder.FrameCount}) to {p[1]}");
                    break;
                }
            case "shot":
                {
                    int w = p.Length > 3 ? int.Parse(p[2]) : player.StageWidth;
                    int h = p.Length > 3 ? int.Parse(p[3]) : player.StageHeight;
                    using var surface = SKSurface.Create(new SKImageInfo(w, h));
                    // Optional 4th number: draw partway (0..1) between the previous and current game frame.
                    float t = p.Length > 4 ? F(4) : 1;
                    player.Render(surface.Canvas, w, h, t);
                    // Vector images are drawn in the background at this size: wait for them, then draw again.
                    var waitStart = DateTime.UtcNow;
                    if (System.Threading.Volatile.Read(ref Mnog.Engine.Render.RenderContext.VectorImagesPending) > 0)
                    {
                        while (System.Threading.Volatile.Read(ref Mnog.Engine.Render.RenderContext.VectorImagesPending) > 0 && (DateTime.UtcNow - waitStart).TotalSeconds < 20)
                            System.Threading.Thread.Sleep(50);
                        surface.Canvas.Clear();
                        player.Render(surface.Canvas, w, h, t);
                    }
                    using var img = surface.Snapshot();
                    using var data = img.Encode(SKEncodedImageFormat.Png, 100);
                    File.WriteAllBytes(p[1], data.ToArray());
                    Console.WriteLine($"saved {p[1]} (tick {player.TickCount})");
                    break;
                }
            case "type":
                player.OnTextInput(p[1]);
                break;
            case "load":
                // load URL TARGET: loadMovie as a script would (applied on the next tick).
                player.LoadMovie(p[1], Mnog.Engine.Script.Value.From(p[2]), player.Levels.Values.First());
                break;
            case "set":
                // set PATH VALUE: set a variable (relative to _level1, e.g. "MovieName"), as a script would.
                player.Vm.SetVariableAt(player.GetLevel(1) ?? player.Levels.Values.First(), p[1], Mnog.Engine.Script.Value.From(p[2]));
                break;
            case "setting":
                // setting KEY VALUE: a saved setting changed, as the settings menu changes it (e.g. setting pakari 1).
                player.SetSetting(p[1], p[2]);
                break;
            case "hide":
            case "shift":
            case "goto":
                // hide PATH: hide a display object (slash path from _level1; "#9" names an unnamed one by its depth).
                {
                    Mnog.Engine.Display.DisplayObject? o = player.GetLevel(1);
                    foreach (var part in p[1].Split('/', StringSplitOptions.RemoveEmptyEntries))
                        o = o is Mnog.Engine.Display.MovieClip mc
                            ? part.StartsWith('#') && int.TryParse(part.AsSpan(1), out int depth) ? mc.Children.GetValueOrDefault(depth) : mc.ChildByName(part)
                            : null;
                    if (o == null) { Console.WriteLine($"{p[0]}: no {p[1]}"); break; }
                    if (p[0] == "hide") o.Visible = false;
                    // shift PATH DX DY: move it; goto PATH FRAME: hold a clip at that frame.
                    else if (p[0] == "shift") { o.Matrix = o.Matrix.PostConcat(SKMatrix.CreateTranslation(F(2), F(3))); o.TransformedByScript = true; }
                    // (goto PATH FRAME play: went to, and playing on from there.)
                    else if (o is Mnog.Engine.Display.MovieClip held) { held.GotoFrame(int.Parse(p[2])); held.Playing = p.Length > 3 && p[3] == "play"; }
                    break;
                }
            case "scene":
                // scene NAME: go to a scene the way the game's navigation does (as the warp menu does).
                player.WarpTo(p[1]);
                break;
            case "extentms":
                if (player._extentMs.Count > 0)
                    Console.WriteLine($"extent: {player._extentMs.Count} runs, avg {player._extentMs.Average():0.00} ms, max {player._extentMs.Max():0.00} ms");
                break;
            case "flactest":
                // flactest FILE.flac RAW: decode, and compare with RAW (ffmpeg's decode: signed 16-bit little-endian, interleaved).
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var (samples, rate, channels) = Mnog.Engine.Assets.FlacDecoder.Decode(p[1]);
                    var ms = sw.Elapsed.TotalMilliseconds;
                    var ref16 = File.ReadAllBytes(p[2]);
                    int n = ref16.Length / 2, diffs = 0, maxDiff = 0;
                    for (int i = 0; i < Math.Min(n, samples.Length); i++)
                    {
                        int want = BitConverter.ToInt16(ref16, i * 2), got = (int)Math.Round(samples[i] * 32768);
                        if (got != want) { diffs++; maxDiff = Math.Max(maxDiff, Math.Abs(got - want)); }
                    }
                    Console.WriteLine($"flactest: {rate} Hz, {channels} ch, {samples.Length} samples (ffmpeg {n}) in {ms:0} ms; {diffs} differ, max {maxDiff}");
                }
                break;
            case "pos":
                // pos PATH...: print clips' x positions (paths relative to _level1).
                Console.WriteLine($"pos tick {player.TickCount}: " + string.Join("  ", p.Skip(1).Select(path =>
                    player.Vm.ResolveTargetFrom(player.GetLevel(1)!, Mnog.Engine.Script.Value.From(path)) is { } o ? $"{path}={o.Matrix.TransX:0.0}" + (Environment.GetEnvironmentVariable("MNOG_POSDBG") == "1" ? $" [{o.Matrix.ScaleX:0.###} {o.Matrix.SkewY:0.###} {o.Matrix.SkewX:0.###} {o.Matrix.ScaleY:0.###}] lerp={o.LerpMatrix}/{o.LerpFull}" : "") : $"{path}=?")));
                break;
            case "get":
                // get PATH...: print variables (slash paths relative to _level1, e.g. /Backpack/Backpack/:Slot1).
                foreach (var path in p.Skip(1))
                    Console.WriteLine($"get {path} = {player.Vm.GetVariableAt(player.GetLevel(1)!, path).ToStr(6)}");
                break;
            case "profile":
                // profile N W H: run N frames, drawing each at W x H, and report the slow ones.
                {
                    int n = int.Parse(p[1]), w = int.Parse(p[2]), h = int.Parse(p[3]);
                    using var surface = SKSurface.Create(new SKImageInfo(w, h));
                    for (int i = 0; i < n; i++)
                    {
                        long t0 = System.Diagnostics.Stopwatch.GetTimestamp();
                        player.Tick();
                        long t1 = System.Diagnostics.Stopwatch.GetTimestamp();
                        // (profile N W H SUB: also the SUB-1 in-between frames smooth mode draws.)
                        int sub = p.Length > 4 ? int.Parse(p[4]) : 1;
                        for (int k = 1; k <= sub; k++) player.Render(surface.Canvas, w, h, (float)k / sub);
                        surface.Canvas.Flush();
                        long t2 = System.Diagnostics.Stopwatch.GetTimestamp();
                        double tick = (t1 - t0) * 1000.0 / System.Diagnostics.Stopwatch.Frequency, draw = (t2 - t1) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
                        // (profile N W H SUB MS: frames slower than MS reported, 30 by default.)
                        if (tick + draw > (p.Length > 5 ? double.Parse(p[5], CultureInfo.InvariantCulture) : 30))
                        {
                            var holder = player.GetLevel(1)?.ChildByName("holder") as Mnog.Engine.Display.MovieClip;
                            Console.WriteLine($"slow tick {player.TickCount} ({holder?.Movie.Name} frame {holder?.CurrentFrame}): logic {tick:0.0} ms, draw {draw:0.0} ms" +
                                              (sub > 1 ? $" ({draw / sub:0.0} ms a drawing)" : "") +
                                              // (What the layer cache redrew meanwhile, as the window's slow-frame line has it.)
                                              (Mnog.Engine.Render.LayerCache.FrameEvents.Count > 0 ? "; " + string.Join(", ", Mnog.Engine.Render.LayerCache.FrameEvents
                                                  .GroupBy(e => e).Select(g => g.Count() > 1 ? $"{g.Key} x{g.Count()}" : g.Key).Take(8)) : ""));
                        }
                        Mnog.Engine.Render.LayerCache.FrameEvents.Clear();
                        {
                        }
                    }
                    break;
                }
            case "vimg":
                // vimg MOVIE ID SCALE FILE: an image's vector replacement alone, as the engine draws it.
                {
                    var m = Mnog.Engine.Assets.MovieAsset.Load(p[1], System.IO.Path.Combine("game", "movies", p[1]));
                    if (m.Symbol(int.Parse(p[2])) is Mnog.Engine.Assets.ImageAsset ia && Mnog.Engine.Render.RenderContext.RenderVectorImageAlone(ia, F(3)) is { } vi)
                    {
                        using var data = vi.Encode(SKEncodedImageFormat.Png, 100);
                        File.WriteAllBytes(p[4], data.ToArray());
                        Console.WriteLine($"saved {p[4]} {vi.Width}x{vi.Height}");
                    }
                    else Console.WriteLine($"vimg: no vector for {p[1]}#{p[2]} " + string.Join("; ", m.Errors));
                    break;
                }
            case "symshot":
                // symshot MOVIE ID SCALE FILE [FRAME]: one symbol alone (a sprite at FRAME, 1 by default), as the
                // engine draws it, on a transparent background, sized to its bounds.
                {
                    // (MOVIE: a game movie's name, or a converted movie's folder elsewhere, e.g. a scratch conversion.)
                    var m = Directory.Exists(p[1]) ? Mnog.Engine.Assets.MovieAsset.Load(Path.GetFileName(Path.TrimEndingDirectorySeparator(p[1])), p[1]) : player.GetMovie(p[1]);
                    if (m == null) { Console.WriteLine($"symshot: no movie {p[1]}"); break; }
                    var holder = new Mnog.Engine.Display.MovieClip(player, m, m.Timeline) { Inert = true };
                    // (ID 0: the movie's own timeline, at FRAME.)
                    Mnog.Engine.Display.DisplayObject? obj = p[2] == "0" ? holder : holder.Create(int.Parse(p[2]), 1);
                    if (obj == null) { Console.WriteLine($"symshot: no symbol {p[1]}#{p[2]}"); break; }
                    var children = p[2] == "0" ? new SortedList<int, Mnog.Engine.Display.DisplayObject> { [1] = holder } : holder.Children;
                    if (obj is Mnog.Engine.Display.MovieClip clip) { clip.Construct(); if (p.Length > 5) clip.GotoFrame(int.Parse(p[5])); }
                    float scale = F(3);
                    var b = obj.LocalBounds;
                    int w = Math.Max(1, (int)Math.Ceiling(b.Width * scale) + 2), h = Math.Max(1, (int)Math.Ceiling(b.Height * scale) + 2);
                    using var surface = SKSurface.Create(new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Premul));
                    surface.Canvas.Clear(SKColors.Transparent);
                    var matrix = SKMatrix.CreateScaleTranslation(scale, scale, -b.Left * scale + 1, -b.Top * scale + 1);
                    new Mnog.Engine.Render.RenderContext(surface.Canvas, SKMatrix.Identity, player, 1).DrawContainer(children, matrix, Mnog.Engine.Assets.CxForm.Identity);
                    using var img = surface.Snapshot();
                    using var data = img.Encode(SKEncodedImageFormat.Png, 100);
                    File.WriteAllBytes(p[4], data.ToArray());
                    Console.WriteLine($"saved {p[4]} {w}x{h}");
                    break;
                }
            case "art":
                // art X Y: the shapes drawn at a stage point.
                foreach (var a in player.ArtAt(new SKPoint(F(1), F(2)))) Console.WriteLine("art " + a);
                break;
            case "cache":
                player.UseLayerCache = p[1] == "1";
                break;
            case "cachestats":
                Console.WriteLine("cache: " + string.Join(", ", player.LayerCacheStats.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key} {kv.Value}")));
                player.LayerCacheStats.Clear();
                break;
            case "drawprof":
                if (Mnog.Engine.Render.RenderContext.DrawProfile is { } dp)
                {
                    foreach (var kv in dp.OrderByDescending(kv => kv.Value).Take(12)) Console.WriteLine($"drawprof {kv.Value,8:0.0} ms  {kv.Key}");
                    dp.Clear();
                }
                break;
            case "sounds":
                Console.WriteLine($"sounds (tick {player.TickCount}): {string.Join(", ", player.Audio.Describe())}");
                break;
            case "widescreen":
                player.Widescreen = p[1] == "1";
                break;
            case "smooth":
                player.Smooth = p[1] is "1" or "2";
                player.SmoothAll = p[1] == "2";
                break;
            case "fs":
                // fs CMD [ARG]: an fscommand, as a script would send it.
                player.FsCommand(p[1], p.Length > 2 ? p[2] : "");
                break;
            case "debug":
                player.Debug = p[1] == "1";
                break;
            case "tree":
                {
                    // tree [maxDepth]: dump the display list.
                    int max = p.Length > 1 ? int.Parse(p[1]) : 99;
                    void Dump(Mnog.Engine.Display.DisplayObject o, int ind)
                    {
                        var mc = o as Mnog.Engine.Display.MovieClip;
                        Console.WriteLine($"{new string(' ', ind * 2)}{o.Depth} '{o.Name}' {o.GetType().Name} id={o.Asset?.Id} vis={o.Visible}" +
                            (mc != null ? $" [{mc.Movie.Name}] frame {mc.CurrentFrame}/{mc.FrameCount}{(mc.Playing ? " playing" : "")}" : "") + $" placed@{o.PlaceFrame}{(o.LerpMatrix ? " LERP-MOVE" : "")}{(o.LerpColor ? " LERP-COLOR" : "")} cx={o.ColorTransform.RM:0.##}/{o.PrevColor.RM:0.##} snap={o.Snapshotted} clip={o.ClipDepth}{(o.WidescreenScaleX != 1 ? $" WIDE x{o.WidescreenScaleX}" : "")} world={o.WorldMatrix.MapRect(o.LocalBounds)}");
                        if (mc != null && ind < max) foreach (var c in mc.Children.Values) Dump(c, ind + 1);
                    }
                    foreach (var l in player.Levels.Values) Dump(l, 0);
                    break;
                }
            case "bench":
                {
                    // bench N W H: average render time without encoding.
                    int n = int.Parse(p[1]), w = int.Parse(p[2]), h = int.Parse(p[3]);
                    using var surface = SKSurface.Create(new SKImageInfo(w, h));
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    for (int i = 0; i < n; i++) { player.Tick(); player.Render(surface.Canvas, w, h); }
                    surface.Canvas.Flush();
                    Console.WriteLine($"tick+render {w}x{h}: {sw.Elapsed.TotalMilliseconds / n:F1} ms/frame");
                    break;
                }
            default:
                Console.Error.WriteLine($"unknown command {p[0]}");
                break;
        }
    }
}

static partial class Program
{
    static bool MixAudio;
}

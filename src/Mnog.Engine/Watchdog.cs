using System.Collections.Concurrent;
using System.Diagnostics;

namespace Mnog.Engine;

/// <summary>
/// Notices the game freezing. The main loop marks each frame (Beat) and what it's doing (Phase, and Wait around
/// anything that may block on another thread); background jobs (pictures drawn, scenes loaded ahead) and the sound
/// thread say when they start and finish. When the main loop hasn't come round for a few seconds, what everything was
/// doing is written to build/hang-&lt;time&gt;.log next to the game (where crash logs go), with the recent log: it
/// can't say where the code was, but it says which part and what it was waiting for.
/// </summary>
public static class Watchdog
{
    /// <summary>How long without a frame counts as frozen (Windows calls a window not responding after 5 s).</summary>
    const double StallMs = 3000;

    static long _beat = Stopwatch.GetTimestamp();
    static string _phase = "starting";
    static string? _wait;
    static long _waitSince;
    static string _scene = "-";
    static long _mixSince;
    static int _mainThread = -1;
    static readonly Done Nothing = new(() => { });
    static readonly ConcurrentDictionary<string, long> _jobs = new();
    static Thread? _thread;

    /// <summary>Starts watching (once); reports go to <paramref name="dir"/>.</summary>
    public static void Start(Player player, string dir)
    {
        if (_thread != null) return;
        _thread = new Thread(() => Watch(player, dir)) { IsBackground = true, Name = "Watchdog", Priority = ThreadPriority.AboveNormal };
        _thread.Start();
    }

    /// <summary>The main loop came round: the game is running (and the scene it's in, for the report).</summary>
    public static void Beat(string scene)
    {
        _mainThread = Environment.CurrentManagedThreadId;
        _scene = scene;
        Volatile.Write(ref _beat, Stopwatch.GetTimestamp());
    }

    /// <summary>What the main loop is doing now (events, game logic, drawing, presenting).</summary>
    public static void Phase(string phase) => _phase = phase;

    /// <summary>The main loop waiting for something another thread is doing (until disposed); on other threads,
    /// nothing.</summary>
    public static IDisposable Wait(string what)
    {
        if (Environment.CurrentManagedThreadId != _mainThread) return Nothing;
        _wait = what;
        Volatile.Write(ref _waitSince, Stopwatch.GetTimestamp());
        return new Done(() => _wait = null);
    }

    /// <summary>A lock taken (until disposed), as lock () does; if another thread holds it, the main thread notes what it's
    /// waiting for meanwhile.</summary>
    public static Held Hold(object gate, string what)
    {
        if (!Monitor.TryEnter(gate))
            using (Wait(what)) Monitor.Enter(gate);
        return new Held(gate);
    }

    public readonly struct Held(object gate) : IDisposable
    {
        public void Dispose() => Monitor.Exit(gate);
    }

    /// <summary>A background job (until disposed): listed in a report with how long it's been running.</summary>
    public static IDisposable Job(string what)
    {
        string key = $"{what} (thread {Environment.CurrentManagedThreadId})";
        _jobs[key] = Stopwatch.GetTimestamp();
        return new Done(() => _jobs.TryRemove(key, out _));
    }

    /// <summary>The sound thread mixing (true) or done (false).</summary>
    public static void Mixing(bool on) => Volatile.Write(ref _mixSince, on ? Stopwatch.GetTimestamp() : 0);

    sealed class Done(Action end) : IDisposable
    {
        int _done;
        public void Dispose() { if (this != Nothing && Interlocked.Exchange(ref _done, 1) == 0) end(); }
    }

    static double Ms(long since) => Stopwatch.GetElapsedTime(since).TotalMilliseconds;

    static void Watch(Player player, string dir)
    {
        string? reported = null;
        long reportedBeat = 0;
        while (true)
        {
            Thread.Sleep(250);
            long beat = Volatile.Read(ref _beat);
            double stalled = Ms(beat);
            if (reported != null && beat != reportedBeat)
            {
                // Came back: noted in the report (a long stall rather than a freeze).
                try { File.AppendAllText(reported, $"{Environment.NewLine}Resumed after {Ms(reportedBeat) / 1000:0.0} s.{Environment.NewLine}"); }
                catch (IOException) { }
                reported = null;
            }
            if (reported != null || stalled < StallMs) continue;
            reportedBeat = beat;
            reported = Report(player, dir, stalled);
        }
    }

    static string? Report(Player player, string dir, double stalled)
    {
        try
        {
            var lines = new List<string>
            {
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  the game hasn't drawn a frame for {stalled / 1000:0.0} s",
                $"Scene: {_scene}",
                $"Main loop: {_phase}",
            };
            if (_wait is { } wait) lines.Add($"  waiting for: {wait} (for {Ms(Volatile.Read(ref _waitSince)) / 1000:0.0} s)");
            long mix = Volatile.Read(ref _mixSince);
            lines.Add(mix != 0 ? $"Sound thread: mixing for {Ms(mix):0} ms" : "Sound thread: not mixing");
            lines.Add($"Vector pictures being drawn: {Volatile.Read(ref Render.RenderContext.VectorImagesPending)}");
            var jobs = _jobs.ToArray();
            lines.Add(jobs.Length == 0 ? "Background jobs: none" : "Background jobs:");
            foreach (var (what, since) in jobs.OrderBy(j => j.Value)) lines.Add($"  {what}, for {Ms(since) / 1000:0.0} s");
            lines.Add("");
            lines.Add("Recent log:");
            string[] recent;
            try { recent = [.. player.RecentLog]; } catch (Exception e) when (e is InvalidOperationException or ArgumentException) { recent = ["(changing as it was read)"]; }
            lines.AddRange(recent.Select(l => "  " + l));
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, $"hang-{DateTime.Now:yyyyMMdd-HHmmss}.log");
            File.WriteAllLines(file, lines);
            Console.Error.WriteLine($"The game seems frozen; details saved to {file}");
            return file;
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }
}

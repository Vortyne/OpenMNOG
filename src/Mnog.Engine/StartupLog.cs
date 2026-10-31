using System.Runtime.InteropServices;

namespace Mnog.Engine;

/// <summary>
/// logs/startup.log (beside the exe, made anew each run): each step of starting, written at once. Crash logging needs
/// the game running, and a crash inside a native library (a graphics driver) can't be caught at all: the last line
/// says how far it got. A failure before the game is up also shows an error box: a release has no console, and it
/// would otherwise just vanish.
/// </summary>
public static class StartupLog
{
    static StreamWriter? _log;
    static bool _opened, _up;
    static readonly System.Diagnostics.Stopwatch Clock = System.Diagnostics.Stopwatch.StartNew();

    /// <summary>Whether a failure shows the error box (not in headless test runs).</summary>
    public static bool ShowErrors = true;

    /// <summary>The log's folder: logs/ beside the exe, or the temp folder where that can't be written.</summary>
    public static string Dir { get; private set; } = Path.GetTempPath();

    static void Open()
    {
        _opened = true;
        foreach (var dir in new[] { Path.Combine(AppContext.BaseDirectory, "logs"), Path.Combine(Path.GetTempPath(), "OpenMNOG-logs") })
            try
            {
                Directory.CreateDirectory(dir);
                _log = new StreamWriter(Path.Combine(dir, "startup.log"), false);
                Dir = dir;
                return;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
    }

    /// <summary>A step reached, flushed to disk before going on.</summary>
    public static void Step(string step)
    {
        if (_up) return;
        if (!_opened) Open();
        try { _log?.WriteLine($"[{Clock.Elapsed.TotalSeconds,7:0.000}] {step}"); _log?.Flush(); }
        catch (IOException) { }
    }

    /// <summary>The game is up (its first view shown): nothing more to note; crashes go to logs/crash-*.log from here.</summary>
    public static void Up()
    {
        Step("up: the first view is shown");
        _up = true;
        try { _log?.Dispose(); }
        catch (IOException) { }
        _log = null;
    }

    /// <summary>Starting failed: noted, and told to the player.</summary>
    public static void Failed(string what)
    {
        if (_up) return;
        Step("FAILED: " + what);
        Console.Error.WriteLine(what);
        if (!ShowErrors || !OperatingSystem.IsWindows()) return;
        try { MessageBoxW(IntPtr.Zero, $"{what}\n\nDetails are in {Path.Combine(Dir, "startup.log")}", "OpenMNOG could not start", 0x10); }
        catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException) { }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int MessageBoxW(IntPtr window, string text, string caption, uint type);
}

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.SDL;
using SkiaSharp;

namespace Mnog.Engine.Host;

/// <summary>Window, input and audio via SDL2; frames are rendered with Skia into a streaming texture.</summary>
public sealed unsafe class SdlHost : IHost, IDisposable
{
    readonly Sdl _sdl = Sdl.GetApi();
    readonly Player _player;
    Window* _window;
    Renderer* _renderer;
    void* _glContext;
    GRContext? _gr;
    Texture* _texture;
    int _texW, _texH;
    uint _audio;
    Cursor* _arrow, _hand;
    bool _running = true;
    bool _dirty = true;

    public SdlHost(Player player, string title)
    {
        _player = player;
        // (Windows: drawn at the screen's own resolution. Otherwise, on a screen with display scaling (a 4K one at
        // 150%), Windows has the game draw at the scaled-down size and enlarges it, blocky in fullscreen.)
        _sdl.SetHint("SDL_WINDOWS_DPI_AWARENESS", "permonitorv2");
        if (_sdl.Init(Sdl.InitVideo | Sdl.InitAudio | Sdl.InitEvents) < 0)
            throw new InvalidOperationException("SDL init failed: " + _sdl.GetErrorS());
        _sdl.SetHint(Sdl.HintRenderScaleQuality, "1");

        // Default to 2x the stage size, shrunk to fit the screen.
        DisplayMode mode;
        int w = player.StageWidth * 2, h = player.StageHeight * 2;
        if (_sdl.GetDesktopDisplayMode(0, &mode) == 0)
        {
            // (Windows: in the screen's pixels, so as large as the display scaling makes other windows.)
            float ddpi, hdpi, vdpi;
            if (OperatingSystem.IsWindows() && _sdl.GetDisplayDPI(0, &ddpi, &hdpi, &vdpi) == 0 && hdpi > 96)
            {
                w = (int)(w * hdpi / 96); h = (int)(h * hdpi / 96);
            }
            float fit = Math.Min(mode.W * 0.85f / w, mode.H * 0.85f / h);
            if (fit < 1) { w = (int)(w * fit); h = (int)(h * fit); }
        }
        if (!Environment.GetCommandLineArgs().Contains("--software")) TryCreateGl(title, w, h);
        if (_gr == null)
        {
            _window = _sdl.CreateWindow(title, Sdl.WindowposCentered, Sdl.WindowposCentered, w, h,
                (uint)(WindowFlags.Resizable | WindowFlags.AllowHighdpi | WindowFlags.Hidden));
            if (_window == null) throw new InvalidOperationException("SDL window failed: " + _sdl.GetErrorS());
            _renderer = _sdl.CreateRenderer(_window, -1, (uint)(RendererFlags.Accelerated | RendererFlags.Presentvsync));
            if (_renderer == null) _renderer = _sdl.CreateRenderer(_window, -1, (uint)RendererFlags.Software);
            player.Log("rendering on the CPU");
        }
        SetIcon(Path.Combine(player.GameDir, "files", "icon.png"));

        // The sound card pulls its audio on its own thread (AudioCallback), so a slow frame never starves it.
        _audioCallback = AudioCallback;
        var want = new AudioSpec
        {
            Freq = Audio.AudioMixer.OutputRate, Format = Sdl.AudioF32, Channels = 2, Samples = 1024,
            Callback = new PfnAudioCallback(_audioCallback),
        };
        AudioSpec have;
        _audio = _sdl.OpenAudioDevice((byte*)null, 0, &want, &have, 0);
        if (_audio != 0) _sdl.PauseAudioDevice(_audio, 0);
        else player.Log("audio unavailable: " + _sdl.GetErrorS());

        _arrow = _sdl.CreateSystemCursor(SystemCursor.SystemCursorArrow);
        _hand = _sdl.CreateSystemCursor(SystemCursor.SystemCursorHand);
        _sdl.StartTextInput();
        player.Host = this;
    }

    /// <summary>The window's icon (taskbar, title bar), from the game's icon.png: shown however the engine is run (the
    /// program's own icon is only Windows' exe, not dotnet run's or Linux's).</summary>
    void SetIcon(string file)
    {
        if (!File.Exists(file)) return;
        try
        {
            using var image = SKImage.FromEncodedData(file);
            if (image == null) return;
            int w = image.Width, h = image.Height;
            // (RGBA bytes, straight alpha: SDL's ABGR8888 on a little-endian machine.)
            var pixels = new byte[w * h * 4];
            fixed (byte* p = pixels)
            {
                if (!image.ReadPixels(new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Unpremul), (IntPtr)p, w * 4)) return;
                var surface = _sdl.CreateRGBSurfaceWithFormatFrom(p, w, h, 32, w * 4, (uint)PixelFormatEnum.Abgr8888);
                if (surface == null) return;
                _sdl.SetWindowIcon(_window, surface);
                _sdl.FreeSurface(surface);
            }
        }
        catch (Exception e) when (e is IOException or ArgumentException) { _player.Log($"window icon: {e.Message}"); }
    }

    public bool Fullscreen
    {
        get => (_sdl.GetWindowFlags(_window) & (uint)WindowFlags.FullscreenDesktop) == (uint)WindowFlags.FullscreenDesktop;
        set
        {
            _sdl.SetWindowFullscreen(_window, value ? (uint)WindowFlags.FullscreenDesktop : 0);
            _dirty = true;
            SaveWindowState(value);
        }
    }

    /// <summary>Whether the game was last left fullscreen (it opens that way next time); fullscreen at first.</summary>
    public static bool SavedFullscreen()
    {
        try
        {
            if (File.Exists(WindowStateFile) && System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(WindowStateFile))?["fullscreen"] is { } v)
                return v.GetValue<bool>();
        }
        catch (Exception e) when (e is IOException or System.Text.Json.JsonException or InvalidOperationException or FormatException) { }
        return true;
    }

    static string WindowStateFile => Path.Combine(SaveLocation.Folder, "window.json");

    static void SaveWindowState(bool fullscreen)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(WindowStateFile)!);
            File.WriteAllText(WindowStateFile, $"{{ \"fullscreen\": {(fullscreen ? "true" : "false")} }}" + "\n");
        }
        catch (IOException) { }
    }

    public void SetClipboard(string text) => _sdl.SetClipboardText(text);
    public void ShowCursor(bool visible) => _sdl.ShowCursor(visible ? 1 : 0);
    public void SetHandCursor(bool hand) => _sdl.SetCursor(hand ? _hand : _arrow);
    public void Quit() => _running = false;

    public void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch (Exception e) when (e is not OutOfMemoryException) { _player.Log($"could not open {url}: {e.Message}"); }
    }

    /// <summary>GPU rendering: Skia draws straight into an OpenGL window. Falls back to the CPU on any failure.</summary>
    void TryCreateGl(string title, int w, int h)
    {
        _sdl.GLSetAttribute(GLattr.StencilSize, 8);
        _sdl.GLSetAttribute(GLattr.Doublebuffer, 1);
        _window = _sdl.CreateWindow(title, Sdl.WindowposCentered, Sdl.WindowposCentered, w, h,
            (uint)(WindowFlags.Resizable | WindowFlags.AllowHighdpi | WindowFlags.Hidden | WindowFlags.Opengl));
        if (_window == null) return;
        _glContext = _sdl.GLCreateContext(_window);
        if (_glContext != null && _sdl.GLMakeCurrent(_window, _glContext) == 0)
        {
            // (MNOG_NOVSYNC=1: not held to the display's refresh, to measure how fast frames can be drawn.)
            _sdl.GLSetSwapInterval(Environment.GetEnvironmentVariable("MNOG_NOVSYNC") == "1" ? 0 : 1);
            var glInterface = GRGlInterface.Create(name => (IntPtr)_sdl.GLGetProcAddress(name));
            if (glInterface != null && glInterface.Validate()) _gr = GRContext.CreateGl(glInterface);
        }
        if (_gr != null)
        {
            // (Room for the upscaled photos and the layer cache's images: at Skia's default budget (96 MB) they pushed
            // each other out, and were uploaded again partway through a pan.)
            _gr.SetResourceCacheLimit(1024L * 1024 * 1024);
            _player.Log("rendering on the GPU (OpenGL)");
            return;
        }
        if (_glContext != null) _sdl.GLDeleteContext(_glContext);
        _glContext = null;
        _sdl.DestroyWindow(_window);
        _window = null;
    }

    (int W, int H) OutputSize()
    {
        int w, h;
        if (_gr != null) _sdl.GLGetDrawableSize(_window, &w, &h);
        else _sdl.GetRendererOutputSize(_renderer, &w, &h);
        return (w, h);
    }

    /// <summary>Window coordinates (points) to stage coordinates, accounting for high-DPI scaling.</summary>
    SKPoint ToStage(int x, int y)
    {
        int ww, wh;
        _sdl.GetWindowSize(_window, &ww, &wh);
        var (pw, ph) = OutputSize();
        float sx = ww > 0 ? pw / (float)ww : 1, sy = wh > 0 ? ph / (float)wh : 1;
        return _player.WindowToStage(x * sx, y * sy, pw, ph);
    }

    // The window is created hidden (and made fullscreen while hidden) and shown once the game's first view is
    // ready (Player.Started), so it doesn't open as a bare black window that then changes mode.
    bool _shown;
    const double ShowWindowAfterMs = 3000;

    /// <summary>Called before each game frame once started (Program's --autopilot); false closes the game.</summary>
    public Func<bool>? Autopilot;

    public void Run()
    {
        var clock = Stopwatch.StartNew();
        double interval = 1000.0 / _player.FrameRate;
        double next = 0;
        while (_running)
        {
            Watchdog.Beat(_player.CurrentSceneName ?? "-");
            Watchdog.Phase("input (mouse, keys, window)");
            PumpEvents();
            double now = clock.Elapsed.TotalMilliseconds;
            int ticks = 0;
            // Starting (Player.StartupFrozen): not ticked until the first view is ready to show.
            if (_player.StartupFrozen) { next = now; _dirty = true; }
            while (now >= next && ticks < 4 && !_player.StartupFrozen)
            {
                if (Autopilot != null && _player.Started && !Autopilot()) { _running = false; break; }
                long tickStart = Stopwatch.GetTimestamp();
                Watchdog.Phase("game logic (scripts, loading scenes)");
                _player.Tick();
                _tickMs += Stopwatch.GetElapsedTime(tickStart).TotalMilliseconds;
                next += interval;
                ticks++;
                _dirty = true;
                interval = 1000.0 / _player.FrameRate;
            }
            if (now - next > 250) next = now; // fell far behind (e.g. a slow load): don't fast-forward
            if (_player.Smooth || _player.StartupFrozen)
            {
                // Draw every display refresh (vsync paces the loop), partway between game frames.
                Render((float)Math.Clamp((now - (next - interval)) / interval, 0, 1));
                _dirty = false;
            }
            else if (_dirty) { Render(1); _dirty = false; }
            else _sdl.Delay(1);
            if (!_shown && (_player.Started || clock.Elapsed.TotalMilliseconds > ShowWindowAfterMs))
            {
                _shown = true;
                _sdl.ShowWindow(_window);
                _sdl.RaiseWindow(_window);

            }
        }
    }

    readonly AudioCallback _audioCallback;
    float[] _callbackBuffer = [];

    /// <summary>The sound card asks for more (on its own thread): the game's sounds, or silence until it shows.</summary>
    void AudioCallback(void* userdata, byte* stream, int len)
    {
        int floats = len / sizeof(float);
        var out_ = new Span<float>(stream, floats);
        if (!_player.Started) { out_.Clear(); return; }
        if (_callbackBuffer.Length != floats) _callbackBuffer = new float[floats];
        Watchdog.Mixing(true);
        _player.Audio.Mix(_callbackBuffer);
        Watchdog.Mixing(false);
        _callbackBuffer.CopyTo(out_);
    }

    void PumpEvents()
    {
        Event ev;
        while (_sdl.PollEvent(&ev) != 0)
        {
            switch ((EventType)ev.Type)
            {
                case EventType.Quit:
                    _running = false;
                    break;
                case EventType.Windowevent:
                    _dirty = true;
                    break;
                case EventType.Mousemotion:
                    _player.OnMouseMove(ToStage(ev.Motion.X, ev.Motion.Y));
                    _dirty = true;
                    break;
                case EventType.Mousebuttondown when ev.Button.Button == 1:
                    _player.OnMouseMove(ToStage(ev.Button.X, ev.Button.Y));
                    _player.OnMouseDown();
                    _dirty = true;
                    break;
                case EventType.Mousebuttonup when ev.Button.Button == 1:
                    _player.OnMouseMove(ToStage(ev.Button.X, ev.Button.Y));
                    _player.OnMouseUp();
                    _dirty = true;
                    break;
                case EventType.Mousewheel:
                    _player.OnMouseWheel(ev.Wheel.Y);
                    _dirty = true;
                    break;
                case EventType.Keydown:
                    {
                        var sym = ev.Key.Keysym.Sym;
                        bool alt = (ev.Key.Keysym.Mod & (ushort)Keymod.Alt) != 0;
                        if (sym == (int)KeyCode.KF11 || (alt && sym == (int)KeyCode.KReturn))
                        {
                            if (ev.Key.Repeat == 0) Fullscreen = !Fullscreen;
                            break;
                        }
                        // Left Shift turns around (the scene's turn-around button), as well as being Shift.
                        // (Not with Ctrl, Alt or the Windows key held: Shift as part of a key combination.)
                        bool combo = (ev.Key.Keysym.Mod & (ushort)(Keymod.Ctrl | Keymod.Alt | Keymod.Gui)) != 0;
                        if (sym == (int)KeyCode.KLshift && ev.Key.Repeat == 0 && !combo) _player.TurnAround();
                        var (flash, button) = MapKey(sym);
                        if (flash != 0) _player.OnKeyDown(flash, button);
                        _dirty = true;
                        break;
                    }
                case EventType.Keyup:
                    {
                        var (flash, _) = MapKey(ev.Key.Keysym.Sym);
                        if (flash != 0) _player.OnKeyUp(flash);
                        break;
                    }
                case EventType.Textinput:
                    {
                        var text = Marshal.PtrToStringUTF8((IntPtr)ev.Text.Text) ?? "";
                        _player.OnTextInput(text);
                        _dirty = true;
                        break;
                    }
            }
        }
    }

    /// <summary>SDL key → (Flash Key code, button keyPress code for special keys).</summary>
    static (int, int) MapKey(int sym)
    {
        switch ((KeyCode)sym)
        {
            case KeyCode.KBackspace: return (8, 8);
            case KeyCode.KTab: return (9, 18);
            case KeyCode.KReturn: case KeyCode.KKPEnter: return (13, 13);
            case KeyCode.KEscape: return (27, 19);
            case KeyCode.KSpace: return (32, 0);
            case KeyCode.KPageup: return (33, 16);
            case KeyCode.KPagedown: return (34, 17);
            case KeyCode.KEnd: return (35, 4);
            case KeyCode.KHome: return (36, 3);
            case KeyCode.KLeft: return (37, 1);
            case KeyCode.KUp: return (38, 14);
            case KeyCode.KRight: return (39, 2);
            case KeyCode.KDown: return (40, 15);
            case KeyCode.KInsert: return (45, 5);
            case KeyCode.KDelete: return (46, 6);
            case KeyCode.KLshift: case KeyCode.KRshift: return (16, 0);
            case KeyCode.KLctrl: case KeyCode.KRctrl: return (17, 0);
            case KeyCode.KPause: return (19, 0); // Pause|Break: pauses and plays on an episode
        }
        if (sym is >= 'a' and <= 'z') return (sym - 32, 0);
        if (sym is >= '0' and <= '9') return (sym, 0);
        // , . / (the < > ? key's own characters): Flash's codes for them (the episode frame stepping uses them).
        if (sym == ',') return (188, 0);
        if (sym == '.') return (190, 0);
        if (sym == '/') return (191, 0);
        return (0, 0);
    }

    double _tickMs;
    long _lastFrame;

    void Render(float t)
    {
        var (w, h) = OutputSize();
        if (w <= 0 || h <= 0) return;
        long start = Stopwatch.GetTimestamp();
        RenderFrame(w, h, t);
        // A slow frame (a visible hitch): logged with what took the time, to find it afterwards.
        double draw = Stopwatch.GetElapsedTime(start).TotalMilliseconds, gap = _lastFrame == 0 ? 0 : Stopwatch.GetElapsedTime(_lastFrame).TotalMilliseconds;
        // (Drawn every refresh in smooth mode; otherwise once per game frame, so a gap that long is normal.)
        if (_shown && gap > (_player.Smooth ? 40 : 1000.0 / _player.FrameRate + 25))
            _player.Log($"slow frame: {gap:0} ms since the last (game logic {_tickMs:0.0} ms, drawing {draw:0.0} ms) at {w}x{h} in {_player.CurrentSceneName}" +
                (Mnog.Engine.Render.LayerCache.FrameEvents.Count > 0 ? "; " + string.Join(", ", Mnog.Engine.Render.LayerCache.FrameEvents.GroupBy(e => e).Select(g => g.Count() > 1 ? $"{g.Key} x{g.Count()}" : g.Key).Take(12)) : ""));
        _statsBuilds += Mnog.Engine.Render.LayerCache.FrameEvents.Count(e => e.StartsWith("BUILD"));
        Mnog.Engine.Render.LayerCache.FrameEvents.Clear();
        // Every few seconds, how evenly frames came (in smooth mode, one a display refresh): to find stutter too small
        // for the slow-frame line, e.g. a scene that can't keep up with the display while panning.
        if (_shown && _player.Smooth && gap > 0)
        {
            string scene = _player.CurrentSceneName ?? "";
            if (scene != _statsScene || Stopwatch.GetElapsedTime(_statsStart).TotalSeconds >= 3)
            {
                if (_gaps.Count >= 30)
                {
                    _gaps.Sort(); _draws.Sort();
                    double seconds = Stopwatch.GetElapsedTime(_statsStart).TotalSeconds;
                    double Pct(List<double> v, double p) => v[Math.Min(v.Count - 1, (int)(v.Count * p))];
                    _player.Log($"frames in {_statsScene}: {_gaps.Count / seconds:0} per second; between frames {_gaps.Average():0.0} ms on average, " +
                                $"{Pct(_gaps, 0.5):0.0} median, {Pct(_gaps, 0.95):0.0} at 95%, {_gaps[^1]:0.0} worst; drawing (with the wait for the display) " +
                                $"{_draws.Average():0.0} ms on average, {Pct(_draws, 0.95):0.0} at 95%; {_statsBuilds} cache images built" +
                                (_builds.Count > 0 ? $"; of which building the frame {_builds.Average():0.0} ms on average ({Sorted(_builds, 0.95):0.0} at 95%), " +
                                 $"handing it to the GPU {_flushes.Average():0.0} ({Sorted(_flushes, 0.95):0.0}), presenting it {_swaps.Average():0.0} ({Sorted(_swaps, 0.95):0.0}); " +
                                 $"game logic {_ticks.Average():0.0} ms a frame (worst {_ticks.Max():0.0})" : ""));
                }
                _gaps.Clear(); _draws.Clear(); _builds.Clear(); _flushes.Clear(); _swaps.Clear(); _ticks.Clear(); _statsBuilds = 0;
                _statsScene = scene;
                _statsStart = Stopwatch.GetTimestamp();
            }
            _gaps.Add(gap);
            _draws.Add(draw);
            if (_gr != null) { _builds.Add(_buildMs); _flushes.Add(_flushMs); _swaps.Add(_swapMs); }
            _ticks.Add(_tickMs);
        }
        _lastFrame = Stopwatch.GetTimestamp();
        _tickMs = 0;
    }

    readonly List<double> _gaps = [], _draws = [], _builds = [], _flushes = [], _swaps = [], _ticks = [];
    double _buildMs, _flushMs, _swapMs;
    static double Sorted(List<double> v, double p) { var s = v.Order().ToList(); return s[Math.Min(s.Count - 1, (int)(s.Count * p))]; }
    string? _statsScene;
    long _statsStart;
    int _statsBuilds;

    void RenderFrame(int w, int h, float t)
    {
        if (_gr != null)
        {
            const uint GlRgba8 = 0x8058;
            using var target = new GRBackendRenderTarget(w, h, 0, 8, new GRGlFramebufferInfo(0, GlRgba8));
            using var gpuSurface = SKSurface.Create(_gr, target, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
            long t0 = Stopwatch.GetTimestamp();
            Watchdog.Phase("drawing the frame");
            _player.Render(gpuSurface.Canvas, w, h, t);
            long t1 = Stopwatch.GetTimestamp();
            Watchdog.Phase("handing the frame to the GPU");
            gpuSurface.Flush();
            _gr.Flush();
            long t2 = Stopwatch.GetTimestamp();
            Watchdog.Phase("presenting the frame (waiting for the display)");
            _sdl.GLSwapWindow(_window);
            _buildMs = Stopwatch.GetElapsedTime(t0, t1).TotalMilliseconds;
            _flushMs = Stopwatch.GetElapsedTime(t1, t2).TotalMilliseconds;
            _swapMs = Stopwatch.GetElapsedTime(t2).TotalMilliseconds;
            return;
        }
        if (_texture == null || w != _texW || h != _texH)
        {
            if (_texture != null) _sdl.DestroyTexture(_texture);
            _texture = _sdl.CreateTexture(_renderer, Sdl.PixelformatArgb8888, (int)TextureAccess.Streaming, w, h);
            _texW = w;
            _texH = h;
        }
        void* pixels;
        int pitch;
        if (_sdl.LockTexture(_texture, null, &pixels, &pitch) != 0) return;
        var info = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);
        using (var surface = SKSurface.Create(info, (IntPtr)pixels, pitch))
        {
            Watchdog.Phase("drawing the frame");
            _player.Render(surface.Canvas, w, h, t);
            surface.Canvas.Flush();
        }
        Watchdog.Phase("presenting the frame (waiting for the display)");
        _sdl.UnlockTexture(_texture);
        _sdl.RenderClear(_renderer);
        _sdl.RenderCopy(_renderer, _texture, null, null);
        _sdl.RenderPresent(_renderer);
    }

    public void Dispose()
    {
        if (_audio != 0) _sdl.CloseAudioDevice(_audio);
        if (_texture != null) _sdl.DestroyTexture(_texture);
        if (_renderer != null) _sdl.DestroyRenderer(_renderer);
        _gr?.Dispose();
        if (_glContext != null) _sdl.GLDeleteContext(_glContext);
        if (_window != null) _sdl.DestroyWindow(_window);
        _sdl.Quit();
    }
}

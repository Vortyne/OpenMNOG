using System.Text.Json.Nodes;
using Mnog.Engine.Assets;
using SkiaSharp;

namespace Mnog.Engine;

/// <summary>
/// The chronicle's music window (its music button, the "music" fscommand): the entry's soundtrack tracks
/// (game/audio/soundtrack/soundtrack.json), each with play and pause, how far it's got and its name, played from the
/// original FLAC (decoded when first played, a spinner on its button meanwhile), and a download button that opens the
/// soundtrack folder with that track selected. In the settings menus' look.
/// </summary>
public sealed class MusicMenu
{
    /// <summary>The soundtrack folder (beside the HQ audio).</summary>
    public static string Folder(Player player) => Path.GetFullPath(Path.Combine(player.GameDir, "audio", "soundtrack"));

    /// <summary>A chronicle entry's tracks whose files are here, as (name, file).</summary>
    public static List<(string Name, string File)> Tracks(Player player, int slot)
    {
        var folder = Folder(player);
        var list = new List<(string, string)>();
        try
        {
            var doc = JsonNode.Parse(File.ReadAllText(Path.Combine(folder, "soundtrack.json")));
            foreach (var f in doc?["chronicle"]?[slot.ToString()]?.AsArray() ?? [])
                if (f?.GetValue<string>() is { } name && Path.Combine(folder, name) is var file && File.Exists(file))
                    list.Add((NameOf(name), file));
        }
        catch (Exception e) when (e is IOException or System.Text.Json.JsonException or InvalidOperationException) { }
        return list;
    }

    static string NameOf(string file)
    {
        var name = Path.GetFileNameWithoutExtension(file);
        return name.StartsWith("MNOG OST - ", StringComparison.OrdinalIgnoreCase) ? name["MNOG OST - ".Length..] : name;
    }

    readonly Player _player;
    readonly int _slot;
    readonly List<(string Name, string File)> _tracks;
    int _current = -1;   // the track in the mixer
    int _loading = -1;   // the track being decoded
    Task<(float[] Samples, int Rate, int Channels)>? _decode;
    SKPoint _mouse;

    // Layout in stage coordinates, centred on the stage.
    const float W = 420, RowH = 56;
    readonly SKRect _panel, _closeBox, _doneBox;

    public MusicMenu(Player player, int slot)
    {
        _player = player;
        _slot = slot;
        _tracks = Tracks(player, slot);
        float h = 50 + _tracks.Count * RowH + 56, top = (425 - h) / 2;
        _panel = new SKRect(385 - W / 2, top, 385 + W / 2, top + h);
        _closeBox = new SKRect(_panel.Right - 30, top + 10, _panel.Right - 8, top + 32);
        _doneBox = new SKRect(_panel.Right - 116, _panel.Bottom - 46, _panel.Right - 16, _panel.Bottom - 14);
    }

    SKRect Row(int i) => new(_panel.Left + 16, _panel.Top + 50 + i * RowH, _panel.Right - 16, _panel.Top + 50 + i * RowH + RowH - 8);
    SKRect PlayBox(int i) { var r = Row(i); return new SKRect(r.Left + 4, r.MidY - 18, r.Left + 40, r.MidY + 18); }
    // At the row's right: its download button, the time left of it.
    SKRect DownloadBox(int i) { var r = Row(i); return new SKRect(r.Right - 40, r.MidY - 16, r.Right - 8, r.MidY + 16); }
    SKRect BarBox(int i) { var r = Row(i); return new SKRect(r.Left + 52, r.Top + 30, DownloadBox(i).Left - 84, r.Top + 36); }

    void Play(int i)
    {
        if (i == _current && _loading < 0)
        {
            _player.Audio.TrackPlaying = !_player.Audio.TrackPlaying;
            return;
        }
        if (i == _loading) return;
        // Another track: the one playing stops (and lets go of its samples), this one is decoded meanwhile.
        _player.Audio.SetTrack(null);
        _current = -1;
        _loading = i;
        var file = _tracks[i].File;
        _decode = Task.Run(() => FlacDecoder.Decode(file));
    }

    /// <summary>A decoded track starts playing (checked as the window is drawn).</summary>
    void Loaded()
    {
        if (_decode is not { IsCompleted: true } done) return;
        _decode = null;
        int i = _loading;
        _loading = -1;
        if (done.IsCompletedSuccessfully)
        {
            var (samples, rate, channels) = done.Result;
            _player.Audio.SetTrack(samples, rate, channels);
            _player.Audio.TrackPlaying = true;
            _current = i;
        }
        else _player.ShowMessage($"Couldn't play {_tracks[i].Name}");
    }

    public void MouseMove(SKPoint p) => _mouse = p;

    /// <summary>Whether a click there does something (the pointer cursor shows): close, Done, play and download, and the
    /// playing track's bar.</summary>
    public bool OverClickable(SKPoint p)
    {
        if (_closeBox.Contains(p) || _doneBox.Contains(p)) return true;
        for (int i = 0; i < _tracks.Count; i++)
            if (PlayBox(i).Contains(p) || DownloadBox(i).Contains(p) || i == _current && SKRect.Inflate(BarBox(i), 0, 6).Contains(p)) return true;
        return false;
    }

    public void MouseDown(SKPoint p)
    {
        _mouse = p;
        if (_closeBox.Contains(p) || _doneBox.Contains(p) || !_panel.Contains(p)) { Close(); return; }
        for (int i = 0; i < _tracks.Count; i++)
        {
            if (PlayBox(i).Contains(p)) { Play(i); return; }
            if (DownloadBox(i).Contains(p)) { Download(i); return; }
            // The bar: the playing track moved to that point.
            var bar = BarBox(i);
            if (i == _current && SKRect.Inflate(bar, 0, 6).Contains(p))
            {
                var (_, length) = _player.Audio.TrackTime;
                _player.Audio.SeekTrack(Math.Clamp((p.X - bar.Left) / bar.Width, 0, 1) * length);
                return;
            }
        }
    }

    /// <summary>The soundtrack folder opened with that track selected in it (Windows' Explorer; elsewhere just the
    /// folder).</summary>
    void Download(int i)
    {
        if (OperatingSystem.IsWindows())
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{_tracks[i].File}\"");
                return;
            }
            catch (Exception e) when (e is System.ComponentModel.Win32Exception or InvalidOperationException) { }
        }
        _player.Host?.OpenUrl(Folder(_player));
    }

    public void KeyDown(int code)
    {
        if (code == 27) Close();
        // Space: the playing track (or the first) played or paused.
        if (code == 32 && _tracks.Count > 0) Play(Math.Max(0, _current));
    }

    void Close()
    {
        _player.Audio.SetTrack(null);
        _player.Music = null;
    }

    public void Draw(SKCanvas canvas)
    {
        Loaded();
        using var fill = new SKPaint { IsAntialias = true };
        using var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
        using var text = new SKPaint { IsAntialias = true };
        using var title = new SKFont(_player.MenuTypeface, 20);
        using var name = new SKFont(_player.MenuTypeface, 15);
        using var small = new SKFont(_player.MenuTypeface, 12);
        fill.Color = WallpaperMenu.Back;
        canvas.DrawRect(_panel, fill);
        stroke.Color = WallpaperMenu.Grey;
        canvas.DrawRect(_panel, stroke);
        canvas.DrawLine(_panel.Left + 8, _panel.Top + 42, _panel.Right - 8, _panel.Top + 42, stroke);
        text.Color = SKColors.White;
        string heading = $"Music: {_player.ChronicleTitle(_slot)}";
        canvas.DrawText(heading, 385 - title.MeasureText(heading) / 2, _panel.Top + 29, title, text);
        MenuGlyphs.DrawClose(canvas, _closeBox, _closeBox.Contains(_mouse));
        var (at, length) = _player.Audio.TrackTime;
        bool playing = _player.Audio.TrackPlaying;
        for (int i = 0; i < _tracks.Count; i++)
        {
            var row = Row(i);
            bool current = i == _current;
            fill.Color = current ? new SKColor(0x2A, 0x26, 0x1E) : new SKColor(0x22, 0x22, 0x22);
            canvas.DrawRect(row, fill);
            stroke.Color = current ? WallpaperMenu.Orange : WallpaperMenu.Grey;
            canvas.DrawRect(row, stroke);
            // Play or pause, or a spinner while the track is decoded.
            var play = PlayBox(i);
            bool lit = play.Contains(_mouse);
            fill.Color = lit ? new SKColor(0x44, 0x44, 0x44) : new SKColor(0x33, 0x33, 0x33);
            canvas.DrawOval(play, fill);
            stroke.Color = WallpaperMenu.Grey;
            canvas.DrawOval(play, stroke);
            var glyph = lit ? SKColors.White : WallpaperMenu.Orange;
            if (i == _loading)
            {
                using var spin = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3, StrokeCap = SKStrokeCap.Round, Color = glyph };
                float turn = _player.ElapsedMs % 1000 / 1000f * 360;
                canvas.DrawArc(SKRect.Inflate(play, -10, -10), turn, 270, false, spin);
            }
            else if (current && playing)
            {
                fill.Color = glyph;
                canvas.DrawRect(new SKRect(play.MidX - 7, play.MidY - 8, play.MidX - 2, play.MidY + 8), fill);
                canvas.DrawRect(new SKRect(play.MidX + 2, play.MidY - 8, play.MidX + 7, play.MidY + 8), fill);
            }
            else
            {
                using var tri = new SKPath();
                tri.MoveTo(play.MidX - 5, play.MidY - 9); tri.LineTo(play.MidX + 9, play.MidY); tri.LineTo(play.MidX - 5, play.MidY + 9); tri.Close();
                fill.Color = glyph;
                canvas.DrawPath(tri, fill);
            }
            // The name, then how far it's got: a bar, and the time.
            text.Color = current ? SKColors.White : WallpaperMenu.Orange;
            var download = DownloadBox(i);
            DrawFitted(canvas, _tracks[i].Name, row.Left + 52, row.Top + 20, download.Left - 8 - (row.Left + 52), name, text);
            var bar = BarBox(i);
            fill.Color = new SKColor(0x44, 0x44, 0x44);
            canvas.DrawRect(bar, fill);
            if (current && length > 0)
            {
                fill.Color = WallpaperMenu.Orange;
                canvas.DrawRect(new SKRect(bar.Left, bar.Top, bar.Left + (float)(bar.Width * Math.Clamp(at / length, 0, 1)), bar.Bottom), fill);
            }
            string time = i == _loading ? "Loading..." : current ? $"{Clock(at)} / {Clock(length)}" : "";
            text.Color = WallpaperMenu.Grey;
            canvas.DrawText(time, download.Left - 8 - small.MeasureText(time), bar.MidY + 4, small, text);
            // Download: an arrow down onto a tray.
            bool over = download.Contains(_mouse);
            fill.Color = over ? new SKColor(0x44, 0x44, 0x44) : new SKColor(0x33, 0x33, 0x33);
            canvas.DrawRect(download, fill);
            stroke.Color = WallpaperMenu.Grey;
            canvas.DrawRect(download, stroke);
            using var arrow = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f, StrokeCap = SKStrokeCap.Round,
                                            StrokeJoin = SKStrokeJoin.Round, Color = over ? SKColors.White : WallpaperMenu.Orange };
            float ax = download.MidX, ay = download.MidY;
            using var glyphPath = new SKPath();
            glyphPath.MoveTo(ax, ay - 9); glyphPath.LineTo(ax, ay + 3);
            glyphPath.MoveTo(ax - 5, ay - 2); glyphPath.LineTo(ax, ay + 3); glyphPath.LineTo(ax + 5, ay - 2);
            glyphPath.MoveTo(ax - 8, ay + 4); glyphPath.LineTo(ax - 8, ay + 8); glyphPath.LineTo(ax + 8, ay + 8); glyphPath.LineTo(ax + 8, ay + 4);
            canvas.DrawPath(glyphPath, arrow);
        }
        using var label = new SKFont(_player.MenuTypeface, 17);
        Button(canvas, _doneBox, "Close", label, fill, stroke, text);
    }

    /// <summary>A long name made smaller to fit the width (down to 11), then cut short with "..." if it still doesn't.</summary>
    static void DrawFitted(SKCanvas canvas, string s, float x, float y, float width, SKFont font, SKPaint paint)
    {
        using var fitted = new SKFont(font.Typeface, font.Size);
        float w = fitted.MeasureText(s);
        if (w > width) fitted.Size = Math.Max(11, font.Size * width / w);
        if (fitted.MeasureText(s) > width)
        {
            while (s.Length > 1 && fitted.MeasureText(s + "...") > width) s = s[..^1].TrimEnd();
            s += "...";
        }
        canvas.DrawText(s, x, y, fitted, paint);
    }

    static string Clock(double seconds) => $"{(int)seconds / 60}:{(int)seconds % 60:00}";

    void Button(SKCanvas canvas, SKRect r, string caption, SKFont font, SKPaint fill, SKPaint stroke, SKPaint text)
    {
        bool lit = r.Contains(_mouse);
        fill.Color = new SKColor(0x22, 0x22, 0x22);
        canvas.DrawRect(r, fill);
        stroke.Color = WallpaperMenu.Grey;
        canvas.DrawRect(r, stroke);
        text.Color = lit ? SKColors.White : WallpaperMenu.Orange;
        canvas.DrawText(caption, r.MidX - font.MeasureText(caption) / 2, r.MidY + font.Size * 0.35f, font, text);
    }
}

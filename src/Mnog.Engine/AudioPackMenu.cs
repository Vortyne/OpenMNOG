using SkiaSharp;

namespace Mnog.Engine;

/// <summary>
/// What the High quality option and the chronicle's music button show while the high-quality audio isn't here (see
/// AudioPack): that it's a separate download, and a button to the page with it. Once that's been pressed, also what to
/// do with the download and a button to the game's folder. The zip is looked for in the game's folder all the while, and
/// once it's there, a button unpacks it, in the background with a bar; ready (what was asked for) then happens. The audio
/// is looked for each time either is pressed, so nothing needs restarting. In the settings menus' look.
/// </summary>
public sealed class AudioPackMenu(Player player, Action ready)
{
    SKPoint _mouse;
    bool _opened;   // the download page has been opened
    string? _zip = AudioPack.Zip(player.GameDir);
    long _nextLook;
    Task? _unpack;
    volatile float _progress;
    bool _failed;
    readonly CancellationTokenSource _cancel = new();

    const float W = 400, H = 172, Top = 213 - H / 2;
    static readonly SKRect Panel = new(385 - W / 2, Top, 385 + W / 2, Top + H);
    static readonly SKRect CloseBox = new(Panel.Right - 30, Top + 10, Panel.Right - 8, Top + 32);
    static readonly SKRect FullBox = new(385 - 100, Panel.Bottom - 48, 385 + 100, Panel.Bottom - 14);
    static readonly SKRect LeftBox = new(Panel.Left + 16, Panel.Bottom - 48, 381, Panel.Bottom - 14);
    static readonly SKRect RightBox = new(389, Panel.Bottom - 48, Panel.Right - 16, Panel.Bottom - 14);
    static readonly SKColor Orange = new(0xFF, 0x99, 0x00);

    SKRect PageBox => _opened ? LeftBox : FullBox;
    bool CanUnpack => _zip != null && _unpack == null;
    bool ShowPage => _zip == null;

    public void MouseMove(SKPoint p) => _mouse = p;

    /// <summary>Whether a click there does something (the pointer cursor shows): the buttons.</summary>
    public bool OverClickable(SKPoint p) => CloseBox.Contains(p) || CanUnpack && FullBox.Contains(p) ||
        ShowPage && (PageBox.Contains(p) || _opened && RightBox.Contains(p));

    public void MouseDown(SKPoint p)
    {
        _mouse = p;
        if (CloseBox.Contains(p) || !Panel.Contains(p)) Close();
        else if (CanUnpack)
        {
            if (FullBox.Contains(p)) Unpack();
        }
        else if (!ShowPage) return;
        else if (PageBox.Contains(p))
        {
            player.Host?.OpenUrl(AudioPack.Page);
            _opened = true;
        }
        else if (_opened && RightBox.Contains(p)) player.Host?.OpenUrl(AudioPack.GameFolder(player.GameDir));
    }

    public void KeyDown(int code)
    {
        if (code == 27) Close();
    }

    /// <summary>Closed; an unpacking under way stops, and leaves nothing behind.</summary>
    void Close()
    {
        _cancel.Cancel();
        player.AudioPrompt = null;
    }

    void Unpack()
    {
        string zip = _zip!, gameDir = player.GameDir;
        _failed = false;
        _progress = 0;
        _unpack = Task.Run(() => AudioPack.Unpack(zip, gameDir, p => _progress = p, _cancel.Token));
    }

    /// <summary>The zip looked for, once a second; an unpacking that's finished seen to.</summary>
    void Update()
    {
        if (_unpack is { IsCompleted: true } done)
        {
            _unpack = null;
            if (done.IsCompletedSuccessfully)
            {
                player.AudioPrompt = null;
                ready();
                return;
            }
            _failed = true;
        }
        if (_unpack == null && Environment.TickCount64 >= _nextLook)
        {
            _nextLook = Environment.TickCount64 + 1000;
            _zip = AudioPack.Zip(player.GameDir);
        }
    }

    public void Draw(SKCanvas canvas)
    {
        Update();
        using var fill = new SKPaint { IsAntialias = true };
        using var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
        using var text = new SKPaint { IsAntialias = true };
        using var title = new SKFont(player.MenuTypeface, 20);
        using var label = new SKFont(player.MenuTypeface, 15);
        using var body = new SKFont(player.MenuTypeface, 13);
        fill.Color = WallpaperMenu.Back;
        canvas.DrawRect(Panel, fill);
        stroke.Color = WallpaperMenu.Grey;
        canvas.DrawRect(Panel, stroke);
        canvas.DrawLine(Panel.Left + 8, Top + 42, Panel.Right - 8, Top + 42, stroke);
        text.Color = SKColors.White;
        MenuGlyphs.DrawFitted(canvas, Lang.T("High quality audio"), new SKRect(Panel.Left + 36, Top + 8, Panel.Right - 36, Top + 36), title, text, 4);
        MenuGlyphs.DrawClose(canvas, CloseBox, CloseBox.Contains(_mouse));
        float y = Top + 62;
        foreach (var line in Wrap(_unpack == null && !_failed && _zip == null && !_opened
            ? Lang.F("To use High Quality audio, you need to download the High Quality Audio Pack first. ({0} MB)", AudioPack.SizeMb)
            : Lang.T(_unpack != null ? "Unpacking..."
            : _failed ? "Couldn't unpack the audio. The zip may be incomplete, or the game's folder may not allow it."
            : _zip != null ? "The audio download is in the game's folder."
            : "Put the file you downloaded in OpenMNOG's game folder."), body, W - 32))
        {
            canvas.DrawText(line, 385 - body.MeasureText(line) / 2, y, body, text);
            y += 18;
        }
        if (_unpack != null)
        {
            var bar = new SKRect(FullBox.Left, FullBox.MidY - 4, FullBox.Right, FullBox.MidY + 4);
            fill.Color = new SKColor(0x1A, 0x1A, 0x1A);
            canvas.DrawRoundRect(bar, 4, 4, fill);
            fill.Color = Orange;
            canvas.DrawRoundRect(new SKRect(bar.Left, bar.Top, bar.Left + bar.Width * _progress, bar.Bottom), 4, 4, fill);
            return;
        }
        if (_zip != null)
        {
            Button(canvas, FullBox, Lang.T("Unpack audio"), label, fill, stroke, text);
            return;
        }
        Button(canvas, PageBox, Lang.T("Download page"), label, fill, stroke, text);
        if (_opened) Button(canvas, RightBox, Lang.T("Open game folder"), label, fill, stroke, text);
    }

    void Button(SKCanvas canvas, SKRect box, string name, SKFont font, SKPaint fill, SKPaint stroke, SKPaint text)
    {
        bool lit = box.Contains(_mouse);
        fill.Color = new SKColor(0x33, 0x33, 0x33);
        canvas.DrawRect(box, fill);
        stroke.Color = lit ? Orange : WallpaperMenu.Grey;
        canvas.DrawRect(box, stroke);
        text.Color = lit ? SKColors.White : Orange;
        MenuGlyphs.DrawFitted(canvas, name, box, font, text, 4);
    }

    static IEnumerable<string> Wrap(string s, SKFont font, float width)
    {
        var line = "";
        foreach (var word in s.Split(' '))
        {
            if (line != "" && font.MeasureText(line + " " + word) > width) { yield return line; line = word; }
            else line = line == "" ? word : line + " " + word;
        }
        yield return line;
    }
}

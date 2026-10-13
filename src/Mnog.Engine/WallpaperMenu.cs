using SkiaSharp;

namespace Mnog.Engine;

/// <summary>
/// The chronicle's wallpaper menu (its "mac" button, the "wallpaper" fscommand): a preview of the entry's wallpaper
/// (Player.RenderWallpaper), its sizes to choose from, and Save (then the Save dialog) or Cancel. In the settings
/// menus' look.
/// </summary>
public sealed class WallpaperMenu
{
    readonly Player _player;
    readonly int _slot;
    SKImage? _preview;
    readonly List<Player.WallpaperShot> _frames;
    int _frame;
    bool _listOpen;
    readonly (string Label, int W, int H)[] _sizes;
    int _chosen;
    bool _svg;
    SKPoint _mouse;

    // Layout in stage coordinates, centred on the stage.
    const float W = 420, PreviewH = (W - 32) * 9 / 16, Top = 10;
    static readonly SKRect Panel = new(385 - W / 2, Top, 385 + W / 2, Top + 80 + PreviewH + 104);
    static readonly SKRect CloseBox = new(Panel.Right - 30, Top + 10, Panel.Right - 8, Top + 32);
    static readonly SKRect PreviewBox = new(Panel.Left + 16, Top + 80, Panel.Right - 16, Top + 80 + PreviewH);
    static SKRect SizeBox(int i) => new(Panel.Left + 16 + i * 98.5f, Top + 80 + PreviewH + 14, Panel.Left + 16 + i * 98.5f + 92, Top + 80 + PreviewH + 44);
    static readonly SKRect SaveBox = new(Panel.Right - 16 - 208, Top + 80 + PreviewH + 62, Panel.Right - 16 - 108, Top + 80 + PreviewH + 94);
    // Bottom left: the wallpaper as an SVG (vector, any size).
    static readonly SKRect SvgBox = new(Panel.Left + 16, Top + 80 + PreviewH + 62, Panel.Left + 86, Top + 80 + PreviewH + 94);
    static readonly SKRect CancelBox = new(Panel.Right - 16 - 100, Top + 80 + PreviewH + 62, Panel.Right - 16, Top + 80 + PreviewH + 94);
    internal static readonly SKColor Orange = new(0xFF, 0x99, 0x00), Grey = new(0x99, 0x99, 0x99), Back = new(0x33, 0x33, 0x33, 0xF0);

    public WallpaperMenu(Player player, int slot)
    {
        _player = player;
        _slot = slot;
        var (sw, sh) = player.WindowSize;
        _sizes = [("1080p", 1920, 1080), ("1440p", 2560, 1440), ("4K", 3840, 2160), ("Screen", Math.Max(sw, 640), Math.Max(sh, 360))];
        _chosen = 0;
        _frames = player.WallpaperFrames(slot);
        Preview();
    }

    /// <summary>The chosen frame drawn for the preview (at the panel's size on screen, 16:9).</summary>
    void Preview()
    {
        _preview?.Dispose();
        _preview = _player.RenderWallpaper(_slot, 960, 540, _frames[_frame]);
    }

    string FrameLabel(int i) => $"Wallpaper {i + 1}";

    // The frame dropdown: its box above the preview, its list opening down over it.
    static readonly SKRect FrameBox = new(Panel.Left + 16, Top + 48, Panel.Right - 16 - 56, Top + 72);
    // Up and down arrows beside it: the previous and next wallpaper, without opening the list.
    static readonly SKRect UpBox = new(FrameBox.Right + 6, FrameBox.Top, FrameBox.Right + 31, FrameBox.Bottom);
    static readonly SKRect DownBox = new(FrameBox.Right + 31, FrameBox.Top, FrameBox.Right + 56, FrameBox.Bottom);

    void Step(int by)
    {
        int i = Math.Clamp(_frame + by, 0, _frames.Count - 1);
        if (i != _frame) { _frame = i; Preview(); }
    }
    // The open list's rows: 22 high, smaller to fit above the panel's bottom, in columns when they'd get too small.
    SKRect FrameItem(int i)
    {
        float room = Panel.Bottom - 8 - FrameBox.Bottom;
        int columns = Math.Max(1, (int)Math.Ceiling(_frames.Count * 18 / room));
        int rows = (int)Math.Ceiling(_frames.Count / (float)columns);
        float h = Math.Min(22, room / rows), w = FrameBox.Width / columns;
        int c = i / rows, r = i % rows;
        return new(FrameBox.Left + c * w, FrameBox.Bottom + r * h, FrameBox.Left + (c + 1) * w, FrameBox.Bottom + (r + 1) * h);
    }

    public void MouseMove(SKPoint p) => _mouse = p;

    public void MouseDown(SKPoint p)
    {
        _mouse = p;
        if (_listOpen)
        {
            _listOpen = false;
            for (int i = 0; i < _frames.Count; i++)
                if (FrameItem(i).Contains(p)) { if (i != _frame) { _frame = i; Preview(); } return; }
            return;
        }
        if (FrameBox.Contains(p) && _frames.Count > 1) { _listOpen = true; return; }
        if (UpBox.Contains(p)) { Step(-1); return; }
        if (DownBox.Contains(p)) { Step(1); return; }
        if (CloseBox.Contains(p) || CancelBox.Contains(p) || !Panel.Contains(p)) { Close(); return; }
        for (int i = 0; i < _sizes.Length; i++)
            if (SizeBox(i).Contains(p)) { _chosen = i; _svg = false; return; }
        // SVG: chosen as the format (instead of a size); Save saves it.
        if (SvgBox.Contains(p)) { _svg = true; return; }
        if (SaveBox.Contains(p))
        {
            var (_, w, h) = _sizes[_chosen];
            Close();
            _player.SaveWallpaper(_slot, _svg ? 1920 : w, _svg ? 1080 : h, _frames[_frame], _frame + 1, asSvg: _svg);
        }
    }

    public void KeyDown(int code)
    {
        if (code == 27) { if (_listOpen) _listOpen = false; else Close(); }
        if (code == 38) Step(-1);
        if (code == 40) Step(1);
        if (code == 13) MouseDown(new SKPoint(SaveBox.MidX, SaveBox.MidY));
    }

    void Close()
    {
        _player.Wallpaper = null;
        _preview?.Dispose();
    }

    public void Draw(SKCanvas canvas)
    {
        using var fill = new SKPaint { IsAntialias = true };
        using var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
        using var text = new SKPaint { IsAntialias = true };
        using var title = new SKFont(_player.MenuTypeface, 20);
        using var label = new SKFont(_player.MenuTypeface, 12);
        using var size = new SKFont(_player.MenuTypeface, 15);
        fill.Color = Back;
        canvas.DrawRect(Panel, fill);
        stroke.Color = Grey;
        canvas.DrawRect(Panel, stroke);
        canvas.DrawLine(Panel.Left + 8, Top + 42, Panel.Right - 8, Top + 42, stroke);
        text.Color = SKColors.White;
        string heading = $"Wallpaper: {_player.ChronicleTitle(_slot)}";
        canvas.DrawText(heading, 385 - title.MeasureText(heading) / 2, Top + 29, title, text);
        MenuGlyphs.DrawClose(canvas, CloseBox, CloseBox.Contains(_mouse));
        // The preview: the wallpaper's own 16:9 (or the screen's shape for "Your screen"), fitted in the box.
        fill.Color = SKColors.Black;
        canvas.DrawRect(PreviewBox, fill);
        if (_preview != null)
        {
            var (_, w, h) = _sizes[_chosen];
            float aspect = (float)w / h;
            var box = PreviewBox;
            var r = box.Width / box.Height > aspect
                ? SKRect.Create(box.MidX - box.Height * aspect / 2, box.Top, box.Height * aspect, box.Height)
                : SKRect.Create(box.Left, box.MidY - box.Width / aspect / 2, box.Width, box.Width / aspect);
            // (The preview is 16:9; another shape shows its middle, as the saved one is framed.)
            float sa = (float)_preview.Width / _preview.Height;
            var src = sa > aspect
                ? SKRect.Create((_preview.Width - _preview.Height * aspect) / 2, 0, _preview.Height * aspect, _preview.Height)
                : SKRect.Create(0, (_preview.Height - _preview.Width / aspect) / 2, _preview.Width, _preview.Width / aspect);
            using var sampled = new SKPaint { IsAntialias = true };
            canvas.DrawImage(_preview, src, r, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear), sampled);
        }
        stroke.Color = Grey;
        canvas.DrawRect(PreviewBox, stroke);
        for (int i = 0; i < _sizes.Length; i++)
            Button(canvas, SizeBox(i), _sizes[i].Label, !_svg && i == _chosen, size, fill, stroke, text);
        // The chosen size's pixels, under its button row.
        var (_, cw, ch) = _sizes[_chosen];
        string px = _svg ? "SVG: vector, any size" : $"{cw} x {ch} pixels";
        text.Color = Grey;
        canvas.DrawText(px, 385 - label.MeasureText(px) / 2, SizeBox(0).Bottom + 13, label, text);
        Button(canvas, SaveBox, "Save", false, title, fill, stroke, text);
        Button(canvas, SvgBox, "SVG", _svg, size, fill, stroke, text);
        Button(canvas, CancelBox, "Cancel", false, title, fill, stroke, text);
        // The frame dropdown, drawn last so its open list lies over the rest.
        bool many = _frames.Count > 1;
        fill.Color = new SKColor(0x22, 0x22, 0x22);
        canvas.DrawRect(FrameBox, fill);
        stroke.Color = _listOpen || (many && FrameBox.Contains(_mouse)) ? Orange : Grey;
        canvas.DrawRect(FrameBox, stroke);
        text.Color = SKColors.White;
        canvas.DrawText(FrameLabel(_frame), FrameBox.Left + 8, FrameBox.MidY + 5, size, text);
        if (many)
        {
            string count = $"{_frame + 1} of {_frames.Count}";
            text.Color = Grey;
            canvas.DrawText(count, FrameBox.Right - 30 - label.MeasureText(count), FrameBox.MidY + 4, label, text);
            using var tri = new SKPath();
            float tx = FrameBox.Right - 16, ty = FrameBox.MidY;
            tri.MoveTo(tx - 6, ty - 3); tri.LineTo(tx + 6, ty - 3); tri.LineTo(tx, ty + 4); tri.Close();
            fill.Color = Orange;
            canvas.DrawPath(tri, fill);
        }
        // The arrows (greyed at either end).
        foreach (var (box, up) in new[] { (UpBox, true), (DownBox, false) })
        {
            bool can = up ? _frame > 0 : _frame < _frames.Count - 1;
            fill.Color = can && box.Contains(_mouse) ? new SKColor(0x44, 0x44, 0x44) : new SKColor(0x22, 0x22, 0x22);
            canvas.DrawRect(box, fill);
            stroke.Color = Grey;
            canvas.DrawRect(box, stroke);
            using var arrow = new SKPath();
            float ax = box.MidX, ay = box.MidY;
            if (up) { arrow.MoveTo(ax - 6, ay + 3); arrow.LineTo(ax + 6, ay + 3); arrow.LineTo(ax, ay - 4); }
            else { arrow.MoveTo(ax - 6, ay - 3); arrow.LineTo(ax + 6, ay - 3); arrow.LineTo(ax, ay + 4); }
            arrow.Close();
            fill.Color = !can ? new SKColor(0x55, 0x55, 0x55) : box.Contains(_mouse) ? SKColors.White : Orange;
            canvas.DrawPath(arrow, fill);
        }
        if (_listOpen)
            for (int i = 0; i < _frames.Count; i++)
            {
                var r = FrameItem(i);
                bool lit = r.Contains(_mouse);
                fill.Color = i == _frame ? new SKColor(0x55, 0x44, 0x22) : lit ? new SKColor(0x44, 0x44, 0x44) : new SKColor(0x22, 0x22, 0x22, 0xF8);
                canvas.DrawRect(r, fill);
                stroke.Color = Grey;
                canvas.DrawRect(r, stroke);
                text.Color = lit || i == _frame ? SKColors.White : Orange;
                canvas.DrawText(FrameLabel(i), r.Left + 8, r.MidY + 5, size, text);
            }
    }

    void Button(SKCanvas canvas, SKRect r, string caption, bool chosen, SKFont font, SKPaint fill, SKPaint stroke, SKPaint text)
    {
        bool lit = r.Contains(_mouse);
        fill.Color = chosen ? new SKColor(0x55, 0x44, 0x22) : new SKColor(0x22, 0x22, 0x22);
        canvas.DrawRect(r, fill);
        stroke.Color = chosen ? Orange : Grey;
        canvas.DrawRect(r, stroke);
        text.Color = lit || chosen ? SKColors.White : Orange;
        canvas.DrawText(caption, r.MidX - font.MeasureText(caption) / 2, r.MidY + font.Size * 0.35f, font, text);
    }
}

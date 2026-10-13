using SkiaSharp;

namespace Mnog.Engine;

/// <summary>
/// Debug menu for jumping to any scene, cutscene (Episode) or minigame (Game). Opened with = in the settings
/// dialog; type to filter, arrows/wheel to move, Enter or click to warp, Esc or a click outside to close.
/// </summary>
public sealed class WarpMenu
{
    readonly Player _player;
    readonly List<string> _all;
    List<string> _shown;
    string _filter = "";
    int _selected, _scroll;

    const int Rows = 15;
    const float RowHeight = 19, Width = 300, Left = 385 - Width / 2, Top = 28;
    const float ListTop = Top + 52;
    static float Height => 52 + Rows * RowHeight + 24;

    readonly SKFont _font = new(SKTypeface.FromFamilyName("Consolas") ?? SKTypeface.Default, 12);
    readonly SKFont _titleFont = new(SKTypeface.FromFamilyName("Consolas", SKFontStyle.Bold) ?? SKTypeface.Default, 14);
    readonly SKPaint _back = new() { Color = new SKColor(0x22, 0x22, 0x22, 0xF0) };
    readonly SKPaint _border = new() { Color = new SKColor(0x99, 0x99, 0x99), Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, IsAntialias = true };
    readonly SKPaint _text = new() { Color = SKColors.White, IsAntialias = true };
    readonly SKPaint _accent = new() { Color = new SKColor(0xFF, 0x99, 0x00), IsAntialias = true };
    readonly SKPaint _dim = new() { Color = new SKColor(0xAA, 0xAA, 0xAA), IsAntialias = true };
    readonly SKPaint _highlight = new() { Color = new SKColor(0xFF, 0x99, 0x00, 0x50) };

    public WarpMenu(Player player)
    {
        _player = player;
        _all = player.WarpTargets().ToList();
        _shown = _all;
        // Start at the current scene.
        int here = _all.FindIndex(n => n.Equals(player.CurrentSceneName, StringComparison.OrdinalIgnoreCase));
        if (here >= 0) Select(here);
    }

    void Refilter()
    {
        var words = _filter.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        // Names starting with the filter come first ("V.1" lists V.1-S before IV.1-N).
        _shown = _all.Where(n => words.All(w => n.Contains(w, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(n => words.Length > 0 && n.StartsWith(words[0], StringComparison.OrdinalIgnoreCase) ? 0 : 1).ToList();
        _selected = 0;
        _scroll = 0;
    }

    void Select(int i)
    {
        if (_shown.Count == 0) return;
        _selected = Math.Clamp(i, 0, _shown.Count - 1);
        if (_selected < _scroll) _scroll = _selected;
        if (_selected >= _scroll + Rows) _scroll = _selected - Rows + 1;
    }

    void Scroll(int by) => _scroll = Math.Clamp(_scroll + by, 0, Math.Max(0, _shown.Count - Rows));

    void Warp(int i)
    {
        if (i < 0 || i >= _shown.Count) return;
        _player.CloseWarpMenu();
        _player.WarpTo(_shown[i]);
    }

    /// <summary>Flash key codes: 8 backspace, 13 enter, 27 escape, 33/34 page up/down, 38/40 up/down.</summary>
    public void KeyDown(int code)
    {
        switch (code)
        {
            case 27: _player.CloseWarpMenu(); break;
            case 13: Warp(_selected); break;
            case 38: Select(_selected - 1); break;
            case 40: Select(_selected + 1); break;
            case 33: Select(_selected - Rows); break;
            case 34: Select(_selected + Rows); break;
            case 8:
                if (_filter.Length > 0) { _filter = _filter[..^1]; Refilter(); }
                break;
        }
    }

    public void Text(string text)
    {
        foreach (var ch in text)
            if (!char.IsControl(ch)) _filter += ch;
        Refilter();
    }

    public void Wheel(float dy) => Scroll(dy > 0 ? -3 : 3);

    int? RowAt(SKPoint p)
    {
        if (p.X < Left || p.X > Left + Width || p.Y < ListTop) return null;
        int r = (int)((p.Y - ListTop) / RowHeight);
        return r < Rows && _scroll + r < _shown.Count ? _scroll + r : null;
    }

    public void MouseMove(SKPoint p)
    {
        if (RowAt(p) is int i) _selected = i;
    }

    /// <summary>Whether a click there does something (the pointer cursor shows): a scene in the list.</summary>
    public bool OverClickable(SKPoint p) => p.X >= Left && p.X <= Left + Width && RowAt(p) != null;

    public void MouseDown(SKPoint p)
    {
        bool inside = p.X >= Left && p.X <= Left + Width && p.Y >= Top && p.Y <= Top + Height;
        if (!inside) { _player.CloseWarpMenu(); return; }
        if (RowAt(p) is int i) Warp(i);
    }

    /// <summary>Draws in stage coordinates.</summary>
    public void Draw(SKCanvas canvas)
    {
        var box = new SKRect(Left, Top, Left + Width, Top + Height);
        canvas.DrawRect(box, _back);
        canvas.DrawRect(box, _border);
        canvas.DrawText("Warp to scene", Left + 10, Top + 20, _titleFont, _accent);
        canvas.DrawText($"{_shown.Count} of {_all.Count}", Left + Width - 10, Top + 20, SKTextAlign.Right, _font, _dim);
        canvas.DrawText("Filter: " + _filter + "_", Left + 10, Top + 42, _font, _text);
        canvas.DrawLine(Left + 6, ListTop - 4, Left + Width - 6, ListTop - 4, _border);
        for (int r = 0; r < Rows && _scroll + r < _shown.Count; r++)
        {
            int i = _scroll + r;
            float y = ListTop + r * RowHeight;
            if (i == _selected) canvas.DrawRect(new SKRect(Left + 4, y, Left + Width - 4, y + RowHeight), _highlight);
            string name = _shown[i];
            canvas.DrawText(name, Left + 12, y + 14, _font, _text);
            string kind = Player.WarpKind(name);
            canvas.DrawText(kind, Left + Width - 12, y + 14, SKTextAlign.Right, _font, _dim);
        }
        if (_shown.Count > Rows)
        {
            // Scroll bar.
            float track = Rows * RowHeight, thumb = Math.Max(12, track * Rows / _shown.Count);
            float at = ListTop + (track - thumb) * _scroll / Math.Max(1, _shown.Count - Rows);
            canvas.DrawRect(new SKRect(Left + Width - 4, at, Left + Width - 1, at + thumb), _dim);
        }
        canvas.DrawText("Type to filter   ↑↓ Enter   Esc to close", Left + 10, Top + Height - 8, _font, _dim);
    }
}

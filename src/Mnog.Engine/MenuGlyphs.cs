using Mnog.Engine.Assets;
using SkiaSharp;

namespace Mnog.Engine;

/// <summary>Marks the engine's own menus share with the overlay's settings dialogs.</summary>
public static class MenuGlyphs
{
    /// <summary>A label centred in a box, its text made smaller if it wouldn't fit across it (a longer translation).</summary>
    public static void DrawFitted(SKCanvas canvas, string s, SKRect box, SKFont font, SKPaint paint, float pad = 8)
    {
        float w = font.MeasureText(s), room = box.Width - pad * 2;
        if (w <= room || w <= 0) { canvas.DrawText(s, box.MidX - w / 2, box.MidY + font.Size * 0.35f, font, paint); return; }
        using var small = new SKFont(font.Typeface, font.Size * room / w);
        canvas.DrawText(s, box.MidX - room / 2, box.MidY + small.Size * 0.35f, small, paint);
    }

    static readonly SKColor Orange = new(0xFF, 0x99, 0x00);

    // The settings dialogs' close X (the overlay's shape 179): 14 x 14 about its centre.
    static readonly SKPath CloseX = SKPath.ParseSvgPathData(
        "M-7 -4.8 L-4.8 -7 L0 -2.2 L4.8 -7 L7 -4.8 L2.2 0 L7 4.8 L4.8 7 L0 2.2 L-4.8 7 L-7 4.8 L-2.2 0 Z");

    /// <summary>
    /// A settings option's description, in a box of the dialogs' look just below the dialog (as wide as it). Each line
    /// of the text is its own paragraph; one starting with a value's name and a colon ("On: ...", "Partial: ...") has
    /// the name in the menus' font and what it means beside it, in the dialog font (wrapped under itself).
    /// </summary>
    public static void DrawHint(SKCanvas canvas, SKRect dialog, string text, SKTypeface menuFace, TextFont body, float bottom = 450)
    {
        using var valueFont = new SKFont(menuFace, 12.5f);
        const float Pad = 9, Line = 16, Gap = 2;
        float width = dialog.Width - Pad * 2;
        // Each paragraph: its value (if any), where its text starts, and its text's lines.
        var paras = new List<(string? Value, float Indent, List<string> Lines)>();
        foreach (var para in text.Split('\n'))
        {
            // ("Partial: ...", and in German and French "Teilweise: ...", "Très lente : ...".)
            var m = System.Text.RegularExpressions.Regex.Match(para, @"^(\p{Lu}[\p{L} ]{0,13}?)( ?:)\s+(.*)$");
            string? value = m.Success ? m.Groups[1].Value + m.Groups[2].Value : null;
            string rest = m.Success ? m.Groups[3].Value : para;
            float indent = value != null ? valueFont.MeasureText(value) + 6 : 0;
            var lines = new List<string>();
            string line = "";
            foreach (var word in rest.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string next = line.Length == 0 ? word : line + " " + word;
                if (line.Length > 0 && body.Measure(next) > width - indent) { lines.Add(line); line = word; }
                else line = next;
            }
            lines.Add(line);
            paras.Add((value, indent, lines));
        }
        float height = Pad * 2 + paras.Sum(p => p.Lines.Count * Line) + Gap * (paras.Count - 1) - 4;
        // (Kept on the stage: pulled up over the dialog's bottom if it would run past it.)
        float top = Math.Min(dialog.Bottom + 8, bottom - 4 - height);
        var box = new SKRect(dialog.Left, top, dialog.Right, top + height);
        using var paint = new SKPaint { IsAntialias = true, Color = new SKColor(0x33, 0x33, 0x33, 0xF4) };
        canvas.DrawRect(box, paint);
        paint.Style = SKPaintStyle.Stroke; paint.StrokeWidth = 2; paint.Color = new SKColor(0x99, 0x99, 0x99);
        canvas.DrawRect(box, paint);
        paint.Style = SKPaintStyle.Fill;
        float y = box.Top + Pad + 10;
        foreach (var (value, indent, lines) in paras)
        {
            if (value != null)
            {
                paint.Color = Orange;
                canvas.DrawText(value, box.Left + Pad, y, valueFont, paint);
            }
            paint.Color = new SKColor(0xE6, 0xE6, 0xE6);
            foreach (var l in lines) { body.Draw(canvas, l, box.Left + Pad + indent, y, paint); y += Line; }
            y += Gap;
        }
    }

    /// <summary>The close X centred in its box: orange, white when the mouse is over it (as the dialogs' X is).</summary>
    public static void DrawClose(SKCanvas canvas, SKRect box, bool hover)
    {
        using var paint = new SKPaint { IsAntialias = true, Color = hover ? SKColors.White : Orange };
        canvas.Save();
        canvas.Translate(box.MidX, box.MidY);
        canvas.DrawPath(CloseX, paint);
        canvas.Restore();
    }
}

/// <summary>Text in a typeface (the menus' font), or in a font of the game's own (glyph outlines, as its text fields
/// draw them).</summary>
public sealed class TextFont
{
    readonly SKFont? _font;
    readonly FontAsset? _glyphs;
    readonly float _size;
    public TextFont(SKFont font) => _font = font;
    public TextFont(FontAsset glyphs, float size) { _glyphs = glyphs; _size = size; }

    public float Measure(string s)
    {
        if (_font != null) return _font.MeasureText(s);
        float w = 0, scale = _size / (float)_glyphs!.Doc.UnitsPerEm;
        foreach (var ch in s)
            w += !_glyphs.ByChar.TryGetValue(ch, out int i) ? (ch == ' ' ? _size / 4 : 0)
                : _glyphs.Doc.Glyphs[i].Advance is double adv ? (float)adv * scale : (_glyphs.Glyphs[i].Bounds.Right + 50) * scale;
        return w;
    }

    public void Draw(SKCanvas canvas, string s, float x, float y, SKPaint paint)
    {
        if (_font != null) { canvas.DrawText(s, x, y, _font, paint); return; }
        float scale = _size / (float)_glyphs!.Doc.UnitsPerEm;
        foreach (var ch in s)
        {
            if (_glyphs.ByChar.TryGetValue(ch, out int i))
            {
                canvas.Save();
                canvas.Translate(x, y);
                canvas.Scale(scale);
                canvas.DrawPath(_glyphs.Glyphs[i], paint);
                canvas.Restore();
            }
            x += Measure(ch.ToString());
        }
    }
}

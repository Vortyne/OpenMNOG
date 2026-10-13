using SkiaSharp;

namespace Mnog.Engine;

/// <summary>
/// The Audio settings (opened from the settings dialog's Audio button, the "audiomenu" fscommand): volume sliders
/// for everything, the music and the other sounds (ambience and effects). Each is saved in
/// the game's save data as a percentage (see Player.Volumes); below them, the High quality option (Player.HqAudio:
/// high-quality replacements for the game's sounds, where found). The back arrow returns to the settings dialog.
/// </summary>
public sealed class AudioMenu(Player player)
{
    public static readonly (string Key, string Label)[] Rows =
        [("volmaster", "Master"), ("volmusic", "Music"), ("volsfx", "SFX")];

    SKPoint _mouse;
    int? _dragging;

    // Layout in stage coordinates, centred on the stage like the settings dialog; the settings menus' look.
    const float W = 360, RowH = 40, Top = 213 - (52 + RowH * 4 + 16) / 2, H = 52 + RowH * 4 + 16;
    static readonly SKRect Panel = new(385 - W / 2, Top, 385 + W / 2, Top + H);
    static readonly SKRect CloseBox = new(Panel.Right - 30, Top + 10, Panel.Right - 8, Top + 32);
    static readonly SKRect BackBox = new(Panel.Left + 8, Top + 10, Panel.Left + 30, Top + 32);
    // The High quality option: a button across the row below the sliders, as the settings dialogs' options are.
    static readonly SKRect HqBox = new(Panel.Left + 10, Top + 52 + RowH * 3 + 6, Panel.Right - 10, Top + 52 + RowH * 3 + 36);
    static SKRect Track(int i) => new(Panel.Left + 110, Top + 52 + RowH * i + 14, Panel.Right - 78, Top + 52 + RowH * i + 22);
    static readonly SKColor Orange = new(0xFF, 0x99, 0x00), Grey = new(0x99, 0x99, 0x99), Back = new(0x33, 0x33, 0x33, 0xF0);

    public void MouseMove(SKPoint p)
    {
        _mouse = p;
        if (_dragging is int i) Set(i, p.X);
    }

    public void MouseDown(SKPoint p)
    {
        _mouse = p;
        if (CloseBox.Contains(p) || !Panel.Contains(p)) { player.CloseAudioMenu(false); return; }
        if (BackBox.Contains(p)) { player.CloseAudioMenu(true); return; }
        if (HqBox.Contains(p)) { player.SetHqAudio(!player.HqAudio); return; }
        for (int i = 0; i < Rows.Length; i++)
        {
            var t = Track(i);
            if (new SKRect(t.Left - 8, t.Top - 10, t.Right + 8, t.Bottom + 10).Contains(p)) { _dragging = i; Set(i, p.X); return; }
        }
    }

    public void MouseUp() => _dragging = null;

    /// <summary>Whether a click there does something (the pointer cursor shows): the buttons and the sliders.</summary>
    public bool OverClickable(SKPoint p)
    {
        if (_dragging != null || CloseBox.Contains(p) || BackBox.Contains(p) || HqBox.Contains(p)) return true;
        for (int i = 0; i < Rows.Length; i++)
        {
            var t = Track(i);
            if (new SKRect(t.Left - 8, t.Top - 10, t.Right + 8, t.Bottom + 10).Contains(p)) return true;
        }
        return false;
    }

    public void KeyDown(int code)
    {
        if (code == 27) player.CloseAudioMenu(false);
    }

    void Set(int i, float x)
    {
        var t = Track(i);
        int percent = (int)Math.Round(Math.Clamp((x - t.Left) / t.Width, 0, 1) * 20) * 5;
        player.SetVolume(Rows[i].Key, percent);
    }

    public void Draw(SKCanvas canvas)
    {
        using var fill = new SKPaint { IsAntialias = true };
        using var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
        using var text = new SKPaint { IsAntialias = true };
        using var title = new SKFont(player.MenuTypeface, 22);
        using var label = new SKFont(player.MenuTypeface, 15);
        fill.Color = Back;
        canvas.DrawRect(Panel, fill);
        stroke.Color = Grey;
        canvas.DrawRect(Panel, stroke);
        canvas.DrawLine(Panel.Left + 8, Top + 42, Panel.Right - 8, Top + 42, stroke);
        text.Color = SKColors.White;
        canvas.DrawText("Audio", 385 - title.MeasureText("Audio") / 2, Top + 30, title, text);
        // X and back arrow, as the settings dialogs draw them.
        MenuGlyphs.DrawClose(canvas, CloseBox, CloseBox.Contains(_mouse));
        using (var arrow = new SKPath())
        {
            float cx = BackBox.MidX, cy = BackBox.MidY;
            arrow.MoveTo(cx - 7, cy); arrow.LineTo(cx - 1, cy - 6); arrow.LineTo(cx - 1, cy - 2.2f); arrow.LineTo(cx + 7, cy - 2.2f);
            arrow.LineTo(cx + 7, cy + 2.2f); arrow.LineTo(cx - 1, cy + 2.2f); arrow.LineTo(cx - 1, cy + 6); arrow.Close();
            fill.Color = BackBox.Contains(_mouse) ? SKColors.White : Orange;
            canvas.DrawPath(arrow, fill);
        }
        for (int i = 0; i < Rows.Length; i++)
        {
            var t = Track(i);
            int v = player.Volume(Rows[i].Key);
            bool lit = _dragging == i || new SKRect(t.Left - 8, t.Top - 10, t.Right + 8, t.Bottom + 10).Contains(_mouse);
            text.Color = lit ? SKColors.White : Orange;
            canvas.DrawText(Rows[i].Label, Panel.Left + 16, t.MidY + 5, label, text);
            fill.Color = new SKColor(0x1A, 0x1A, 0x1A);
            canvas.DrawRoundRect(t, 4, 4, fill);
            fill.Color = Orange;
            canvas.DrawRoundRect(new SKRect(t.Left, t.Top, t.Left + t.Width * v / 100f, t.Bottom), 4, 4, fill);
            stroke.Color = Grey;
            canvas.DrawRoundRect(t, 4, 4, stroke);
            float kx = t.Left + t.Width * v / 100f;
            fill.Color = lit ? SKColors.White : new SKColor(0xDD, 0xDD, 0xDD);
            canvas.DrawCircle(kx, t.MidY, 8, fill);
            canvas.DrawCircle(kx, t.MidY, 8, stroke);
            text.Color = SKColors.White;
            string pct = $"{v}%";
            canvas.DrawText(pct, Panel.Right - 16 - label.MeasureText(pct), t.MidY + 5, label, text);
        }
        bool hqLit = HqBox.Contains(_mouse);
        fill.Color = new SKColor(0x33, 0x33, 0x33);
        canvas.DrawRect(HqBox, fill);
        stroke.Color = Grey;
        canvas.DrawRect(HqBox, stroke);
        // ("Partial": only some of the game's sounds have high-quality originals so far.)
        string hq = $"High quality: {(player.HqAudio ? "Partial" : "Off")}";
        text.Color = hqLit ? SKColors.White : Orange;
        canvas.DrawText(hq, HqBox.MidX - label.MeasureText(hq) / 2, HqBox.MidY + 5, label, text);
        if (hqLit)
            MenuGlyphs.DrawHint(canvas, Panel,
                "Partial: sounds and music play from high-quality originals, where they've been found so far.\nOff: the game's original sounds.",
                player.MenuTypeface, player.DialogFont(13.5f), player.StageHeight);
    }
}

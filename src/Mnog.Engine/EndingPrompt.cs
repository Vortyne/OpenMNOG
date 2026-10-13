using SkiaSharp;

namespace Mnog.Engine;

/// <summary>
/// Shown when the last episode ends (the "gameend" fscommand; see Patches.GameEnd), in place of the original's
/// Chronicle that can't be closed once the game is won: restart the slot, open the Chronicle (closing it comes back
/// here, see Player.OpenEndingChronicle), or go back to the title screen (the save slot menu instead when the title
/// screen is turned off). The slot keeps its last save (the final episode) unless it's restarted.
/// </summary>
public sealed class EndingPrompt(Player player)
{
    SKPoint _mouse;

    static readonly SKRect Box = new(385 - 180, 213 - 75, 385 + 180, 213 + 75);
    static readonly SKRect RestartBox = new(385 - 165, 213 + 30, 385 - 60, 213 + 58), ChronicleBox = new(385 - 52, 213 + 30, 385 + 52, 213 + 58),
        TitleBox = new(385 + 60, 213 + 30, 385 + 165, 213 + 58);
    static readonly SKColor Orange = new(0xFF, 0x99, 0x00), Grey = new(0x99, 0x99, 0x99), Back = new(0x33, 0x33, 0x33, 0xF0), BackLit = new(0x4D, 0x4D, 0x4D, 0xF0);

    public void MouseMove(SKPoint p) => _mouse = p;

    /// <summary>Whether a click there does something (the pointer cursor shows): one of the three choices.</summary>
    public bool OverClickable(SKPoint p) => RestartBox.Contains(p) || ChronicleBox.Contains(p) || TitleBox.Contains(p);

    public void MouseDown(SKPoint p)
    {
        _mouse = p;
        if (RestartBox.Contains(p)) { player.CloseEndingPrompt(); player.RestartSaveSlot(player.CurrentSaveSlot); }
        else if (ChronicleBox.Contains(p)) player.OpenEndingChronicle();
        else if (TitleBox.Contains(p) && player.TitleEnabled) { player.CloseEndingPrompt(); player.ReturnToTitle(); }
        else if (TitleBox.Contains(p)) player.SlotMenu = new SaveSlotMenu(player, SaveSlotMenu.Mode.Game);
    }

    public void Draw(SKCanvas canvas)
    {
        using var fill = new SKPaint { IsAntialias = true, Color = new SKColor(0, 0, 0, 0x99) };
        using var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
        using var text = new SKPaint { IsAntialias = true };
        using var title = new SKFont(player.MenuTypeface, 24);
        using var label = new SKFont(player.MenuTypeface, 15);
        canvas.DrawRect(new SKRect(player.ViewLeft, 0, player.ViewRight, player.StageHeight), fill);
        void BoxAt(SKRect r, bool lit)
        {
            fill.Color = lit ? BackLit : Back;
            canvas.DrawRect(r, fill);
            stroke.Color = lit ? Orange : Grey;
            canvas.DrawRect(r, stroke);
        }
        void Centered(string s, float y, SKFont font, SKColor color, SKRect? r = null)
        {
            text.Color = color;
            float cx = r?.MidX ?? Box.MidX;
            canvas.DrawText(s, cx - font.MeasureText(s) / 2, y, font, text);
        }
        BoxAt(Box, false);
        Centered("Thanks for playing!", Box.Top + 40, title, SKColors.White);
        Centered($"Restart save slot {player.CurrentSaveSlot}?", Box.Top + 68, label, Grey);
        foreach (var (r, s) in new[] { (RestartBox, "Restart slot"), (ChronicleBox, "Chronicle"), (TitleBox, player.TitleEnabled ? "Title screen" : "Save slots") })
        {
            bool lit = r.Contains(_mouse);
            BoxAt(r, lit);
            Centered(s, r.MidY + label.Size * 0.35f, label, lit ? SKColors.White : Orange, r);
        }
    }
}

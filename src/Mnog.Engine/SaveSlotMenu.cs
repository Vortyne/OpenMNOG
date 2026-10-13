using SkiaSharp;

namespace Mnog.Engine;

/// <summary>
/// The save slot menu: 12 slots, 4 to a tab, each with a picture of where it was last saved. Opened from the
/// bottom bar's Change Save Slot button (picking a slot loads it) or the title screen's Save Slot button (picking
/// one makes it the slot Continue and New Game use). Saves can be deleted, after a confirmation. The game saves
/// by itself, so there's no save or load; the slots live in the patched game's save (see Patches, useSlot).
/// </summary>
public sealed class SaveSlotMenu
{
    public enum Mode { Game, Title }

    public const int Slots = 12, PerPage = 4;

    readonly Player _player;
    readonly Mode _mode;
    int _page;
    int? _confirm;
    SKPoint _mouse;
    readonly SlotInfo[] _slots = new SlotInfo[Slots + 1];
    int _current;

    sealed class SlotInfo : IDisposable
    {
        public bool Has;
        public string Scene = "";
        public DateTime? Saved;
        public SKImage? Picture;
        public void Dispose() => Picture?.Dispose();
    }

    // Layout, in stage coordinates (the panel sits over the original stage).
    const float W = 560, H = 344, L = 385 - W / 2, T = 213 - H / 2;
    const float CardW = (W - 16 * 3) / 2, CardH = 118, CardTop = T + 84, Gap = 12;
    const float PicW = 144, PicH = 81;
    static readonly SKRect Panel = new(L, T, L + W, T + H);
    static readonly SKRect CloseBox = new(L + W - 30, T + 8, L + W - 8, T + 30);
    static SKRect Tab(int i) => new(L + 16 + i * 84, T + 46, L + 16 + i * 84 + 76, T + 70);
    static SKRect Card(int i) => new(L + 16 + i % 2 * (CardW + 16), CardTop + i / 2 * (CardH + Gap), L + 16 + i % 2 * (CardW + 16) + CardW, CardTop + i / 2 * (CardH + Gap) + CardH);
    static SKRect Picture(SKRect card) => new(card.Left + 10, card.Top + (card.Height - PicH) / 2, card.Left + 10 + PicW, card.Top + (card.Height + PicH) / 2);
    static SKRect DeleteBox(SKRect card) => new(card.Right - 78, card.Bottom - 30, card.Right - 10, card.Bottom - 8);
    static readonly SKRect ConfirmBox = new(385 - 160, 213 - 62, 385 + 160, 213 + 62);
    static readonly SKRect YesBox = new(385 - 140, 213 + 22, 385 - 10, 213 + 48), NoBox = new(385 + 10, 213 + 22, 385 + 140, 213 + 48);

    // The settings dialogs' look: dark boxes with a light grey border, orange labels (white when pointed at).
    static readonly SKColor Orange = new(0xFF, 0x99, 0x00), Grey = new(0x99, 0x99, 0x99), Box = new(0x33, 0x33, 0x33, 0xF0), BoxLit = new(0x4D, 0x4D, 0x4D, 0xF0);
    readonly SKPaint _fill = new() { IsAntialias = true };
    readonly SKPaint _stroke = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
    readonly SKPaint _text = new() { IsAntialias = true };
    readonly SKFont _title, _label, _small;

    public SaveSlotMenu(Player player, Mode mode)
    {
        _player = player;
        _mode = mode;
        var face = player.MenuTypeface;
        _title = new SKFont(face, 22);
        _label = new SKFont(face, 15);
        _small = new SKFont(face, 11);
        Reload();
        _page = (_current - 1) / PerPage;
    }

    void Reload()
    {
        _current = _player.CurrentSaveSlot;
        for (int n = 1; n <= Slots; n++)
        {
            _slots[n]?.Dispose();
            var state = _player.SaveSlotState(n);
            var info = new SlotInfo { Has = state.Length > 0 && state != "&nostate=1&" };
            if (info.Has)
            {
                foreach (var pair in state.Split('&'))
                    if (pair.StartsWith("mvn=", StringComparison.OrdinalIgnoreCase)) info.Scene = Path.GetFileNameWithoutExtension(pair[4..]);
                var file = _player.SlotPictureFile(n);
                if (File.Exists(file))
                {
                    info.Saved = File.GetLastWriteTime(file);
                    // Read into memory: an image made straight from the file keeps it open (and undeletable).
                    try { info.Picture = SKImage.FromEncodedData(File.ReadAllBytes(file)); } catch (Exception e) when (e is IOException or ArgumentException) { }
                }
            }
            _slots[n] = info;
        }
    }

    public void Close()
    {
        for (int n = 1; n <= Slots; n++) _slots[n]?.Dispose();
        _player.CloseSaveSlotMenu();
    }

    bool CanDelete(int n) => _slots[n].Has && !(_mode == Mode.Game && n == _current);

    /// <summary>In game, the slot being played can be restarted instead (the original's Start New Game).</summary>
    bool CanRestart(int n) => _mode == Mode.Game && n == _current;

    /// <summary>The confirmation asked for: deleting a slot, or restarting the one being played.</summary>
    bool _confirmRestart;

    // ---- Input ----

    public void MouseMove(SKPoint p) => _mouse = p;

    /// <summary>Whether a click there does something (the pointer cursor shows): the confirmation's answers, or the
    /// close button, page tabs and slots.</summary>
    public bool OverClickable(SKPoint p)
    {
        if (_confirm != null) return YesBox.Contains(p) || NoBox.Contains(p);
        if (CloseBox.Contains(p)) return true;
        for (int i = 0; i < Slots / PerPage; i++)
            if (Tab(i).Contains(p)) return true;
        for (int i = 0; i < PerPage; i++)
            if (Card(i).Contains(p)) return true;
        return false;
    }

    public void MouseDown(SKPoint p)
    {
        _mouse = p;
        if (_confirm is int n)
        {
            if (YesBox.Contains(p) && _confirmRestart) { Close(); _player.RestartSaveSlot(n); }
            else if (YesBox.Contains(p)) { _player.DeleteSaveSlot(n, _mode == Mode.Title); _confirm = null; Reload(); }
            else if (NoBox.Contains(p) || !ConfirmBox.Contains(p)) _confirm = null;
            return;
        }
        if (CloseBox.Contains(p) || !Panel.Contains(p)) { Close(); return; }
        for (int i = 0; i < Slots / PerPage; i++)
            if (Tab(i).Contains(p)) { _page = i; return; }
        for (int i = 0; i < PerPage; i++)
        {
            int slot = _page * PerPage + i + 1;
            var card = Card(i);
            if (!card.Contains(p)) continue;
            if ((CanDelete(slot) || CanRestart(slot)) && DeleteBox(card).Contains(p)) { _confirm = slot; _confirmRestart = CanRestart(slot); return; }
            Choose(slot);
            return;
        }
    }

    public void KeyDown(int code)
    {
        if (code == 27) { if (_confirm != null) _confirm = null; else Close(); }
        else if (code == 37) _page = Math.Max(0, _page - 1);
        else if (code == 39) _page = Math.Min(Slots / PerPage - 1, _page + 1);
    }

    void Choose(int slot)
    {
        Close();
        if (_mode == Mode.Game && slot == _current) return;
        _player.UseSaveSlot(slot, load: _mode == Mode.Game);
    }

    // ---- Drawing ----

    public void Draw(SKCanvas canvas)
    {
        // Dim everything behind the menu.
        _fill.Color = new SKColor(0, 0, 0, 0x99);
        canvas.DrawRect(new SKRect(_player.ViewLeft, 0, _player.ViewRight, _player.StageHeight), _fill);
        BoxAt(canvas, Panel, false);
        Text(canvas, Lang.T("Save Slots"), L + 16, T + 32, _title, SKColors.White);
        MenuGlyphs.DrawClose(canvas, CloseBox, CloseBox.Contains(_mouse));
        for (int i = 0; i < Slots / PerPage; i++)
        {
            var r = Tab(i);
            bool on = i == _page, lit = r.Contains(_mouse) && _confirm == null;
            BoxAt(canvas, r, on || lit, on ? Orange : null);
            Centered(canvas, $"{i * PerPage + 1}-{i * PerPage + PerPage}", r, _label, on || lit ? SKColors.White : Orange);
        }
        for (int i = 0; i < PerPage; i++) DrawCard(canvas, i, _page * PerPage + i + 1);
        if (_confirm is int n) DrawConfirm(canvas, n);
    }

    void DrawCard(SKCanvas canvas, int i, int slot)
    {
        var info = _slots[slot];
        var card = Card(i);
        bool active = _confirm == null, overDelete = active && (CanDelete(slot) || CanRestart(slot)) && DeleteBox(card).Contains(_mouse);
        bool lit = active && card.Contains(_mouse) && !overDelete;
        BoxAt(canvas, card, lit, slot == _current ? Orange : null);
        var pic = Picture(card);
        _fill.Color = new SKColor(0x11, 0x11, 0x11);
        canvas.DrawRect(pic, _fill);
        if (info.Picture is { } image)
        {
            canvas.DrawImage(image, pic, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
        }
        else Centered(canvas, Lang.T(info.Has ? "No picture yet" : "Empty"), pic, _small, Grey);
        _stroke.Color = Grey;
        _stroke.StrokeWidth = 1;
        canvas.DrawRect(pic, _stroke);
        float x = pic.Right + 12, y = card.Top + 26;
        Text(canvas, Lang.F("Slot {0}", slot), x, y, _label, lit ? SKColors.White : Orange);
        if (slot == _current) Text(canvas, Lang.T(_mode == Mode.Game ? "Playing" : "Selected"), x, y + 18, _small, SKColors.White);
        if (info.Has)
        {
            Text(canvas, info.Scene, x, y + 36, _small, Grey);
            if (info.Saved is DateTime saved) Text(canvas, saved.ToString("MMM d, HH:mm", Lang.Culture), x, y + 52, _small, Grey);
        }
        else Text(canvas, Lang.T("New game"), x, y + 36, _small, Grey);
        if (CanDelete(slot) || CanRestart(slot))
        {
            var del = DeleteBox(card);
            BoxAt(canvas, del, overDelete);
            Centered(canvas, Lang.T(CanRestart(slot) ? "Restart" : "Delete"), del, _small, overDelete ? SKColors.White : Orange);
        }
    }

    void DrawConfirm(SKCanvas canvas, int slot)
    {
        _fill.Color = new SKColor(0, 0, 0, 0x80);
        canvas.DrawRect(Panel, _fill);
        BoxAt(canvas, ConfirmBox, false);
        Text(canvas, Lang.F(_confirmRestart ? "Restart slot {0}?" : "Delete slot {0}?", slot), ConfirmBox.Left + 16, ConfirmBox.Top + 30, _title, SKColors.White);
        Text(canvas, Lang.T(_confirmRestart ? "Its progress is lost; this cannot be undone." : "This cannot be undone."), ConfirmBox.Left + 16, ConfirmBox.Top + 52, _small, Grey);
        foreach (var (box, label) in new[] { (YesBox, Lang.T(_confirmRestart ? "Restart" : "Delete")), (NoBox, Lang.T("Cancel")) })
        {
            bool lit = box.Contains(_mouse);
            BoxAt(canvas, box, lit);
            Centered(canvas, label, box, _label, lit ? SKColors.White : Orange);
        }
    }

    void BoxAt(SKCanvas canvas, SKRect r, bool lit, SKColor? border = null)
    {
        _fill.Color = lit ? BoxLit : Box;
        canvas.DrawRect(r, _fill);
        _stroke.Color = border ?? (lit ? Orange : Grey);
        _stroke.StrokeWidth = 1.5f;
        canvas.DrawRect(r, _stroke);
    }

    void Text(SKCanvas canvas, string s, float x, float y, SKFont font, SKColor color)
    {
        _text.Color = color;
        canvas.DrawText(s, x, y, font, _text);
    }

    void Centered(SKCanvas canvas, string s, SKRect r, SKFont font, SKColor color)
    {
        _text.Color = color;
        MenuGlyphs.DrawFitted(canvas, s, r, font, _text, 4);
    }
}

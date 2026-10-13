using System.Text.Json.Nodes;
using Mnog.Engine.Assets;
using Mnog.Engine.Display;
using SkiaSharp;

namespace Mnog.Engine;

/// <summary>
/// The Help (the bottom bar's ? button, the "help" fscommand): the original support site's pages the game needs,
/// from game/files/help/help.json, in the settings menus' look. An index on the left (General, Tips and Hints, the
/// Bionicle Decoder), the chosen page on the right, scrolled with the wheel. Tips and Hints works as the site did:
/// an area opens its questions, a question its first hint, and "More..." under it the next. Pictures are the game's
/// own frames, drawn live (a scene copy, see LiveBackdrop; the cursors; the decoder's tablet), not images.
/// </summary>
public sealed class HelpMenu
{
    readonly Player _player;
    readonly List<(string Title, string Page)> _index = [];
    readonly Dictionary<string, (string Title, List<JsonObject> Blocks, List<Area> Areas)> _pages = [];
    sealed record Area(string Name, List<(string Question, List<string> Hints)> Questions);

    int _page;
    float _scroll, _contentHeight;
    int? _openArea;
    readonly Dictionary<(int Area, int Question), int> _shown = [];
    SKPoint _mouse;
    readonly List<(SKRect Rect, Action Click)> _hits = [];
    readonly Dictionary<string, MovieClip?> _scenes = [];
    bool _dragging;
    float _dragOffset;

    // Layout (stage coordinates): over the scene, above the bottom bar.
    static readonly SKRect Panel = new(40, 10, 730, 416);
    const float TitleH = 42;
    static readonly SKRect CloseBox = new(Panel.Right - 30, Panel.Top + 10, Panel.Right - 8, Panel.Top + 32);
    static readonly SKRect IndexArea = new(Panel.Left + 12, Panel.Top + TitleH + 10, Panel.Left + 196, Panel.Bottom - 12);
    static readonly SKRect Content = new(Panel.Left + 212, Panel.Top + TitleH + 10, Panel.Right - 22, Panel.Bottom - 10);
    static readonly SKRect ScrollTrack = new(Panel.Right - 15, Content.Top, Panel.Right - 9, Content.Bottom);
    /// <summary>The conversations' font, for the pages' text.</summary>
    TextFont DialogFont => _player.DialogFont(14.5f);

    static readonly SKColor Orange = new(0xFF, 0x99, 0x00), Grey = new(0x99, 0x99, 0x99), Back = new(0x33, 0x33, 0x33, 0xF4),
        Body = new(0xE6, 0xE6, 0xE6), Box = new(0x33, 0x33, 0x33), BoxLit = new(0x4D, 0x4D, 0x4D);

    public HelpMenu(Player player)
    {
        _player = player;
        var file = Path.Combine(player.GameDir, "files", "help", "help.json");
        if (!File.Exists(file) || JsonNode.Parse(File.ReadAllText(file)) is not JsonObject doc) return;
        foreach (var e in doc["index"] as JsonArray ?? [])
            if (e is JsonObject o) _index.Add((o["title"]!.GetValue<string>(), o["page"]!.GetValue<string>()));
        foreach (var (key, value) in doc["pages"] as JsonObject ?? [])
        {
            if (value is not JsonObject p) continue;
            var areas = new List<Area>();
            foreach (var a in p["areas"] as JsonArray ?? [])
                if (a is JsonObject ao)
                    areas.Add(new Area(ao["area"]!.GetValue<string>(), (ao["questions"] as JsonArray ?? []).OfType<JsonObject>()
                        .Select(q => (q["question"]!.GetValue<string>(), (q["hints"] as JsonArray ?? []).Select(h => h!.GetValue<string>()).ToList())).ToList()));
            _pages[key] = (p["title"]?.GetValue<string>() ?? key, (p["blocks"] as JsonArray ?? []).OfType<JsonObject>().ToList(), areas);
        }
    }

    // ---- Input ----

    public void MouseMove(SKPoint p)
    {
        _mouse = p;
        if (_dragging) ScrollTo((p.Y - _dragOffset - ScrollTrack.Top) / ScrollTrack.Height * _contentHeight);
        UpdateCursor();
    }

    /// <summary>The pointing hand over anything that can be clicked (the index, the areas and questions, the close box).</summary>
    /// (Again after each draw: scrolling or opening an area moves what's under it.)
    void UpdateCursor()
    {
        bool hand = !_dragging && (CloseBox.Contains(_mouse) || _hits.Any(h => h.Rect.Contains(_mouse)));
        if (hand == _hand) return;
        _hand = hand;
        _player.Host?.SetHandCursor(hand);
    }
    bool? _hand;

    public void MouseDown(SKPoint p)
    {
        _mouse = p;
        if (CloseBox.Contains(p) || !Panel.Contains(p)) { _player.CloseHelp(); return; }
        if (Thumb() is { } thumb && new SKRect(ScrollTrack.Left - 4, ScrollTrack.Top, ScrollTrack.Right + 4, ScrollTrack.Bottom).Contains(p))
        {
            _dragging = true;
            _dragOffset = thumb.Contains(p) ? p.Y - thumb.Top : thumb.Height / 2;
            MouseMove(p);
            return;
        }
        foreach (var (rect, click) in _hits)
            if (rect.Contains(p)) { click(); return; }
    }

    public void MouseUp() => _dragging = false;

    /// <summary>Opened again: as it was left, but nothing held from before (a drag, the cursor).</summary>
    public void Reopen() { _dragging = false; _hand = null; _mouse = _player.MouseStage; }

    public void Wheel(float dy) => ScrollTo(_scroll - dy * 40);

    public void KeyDown(int code)
    {
        if (code == 27) _player.CloseHelp();
        else if (code == 38) ScrollTo(_scroll - 40);
        else if (code == 40) ScrollTo(_scroll + 40);
        else if (code == 33) ScrollTo(_scroll - Content.Height * 0.9f);
        else if (code == 34) ScrollTo(_scroll + Content.Height * 0.9f);
    }

    void ScrollTo(float y) => _scroll = Math.Clamp(y, 0, Math.Max(0, _contentHeight - Content.Height));

    SKRect? Thumb()
    {
        if (_contentHeight <= Content.Height) return null;
        float h = Math.Max(24, ScrollTrack.Height * Content.Height / _contentHeight);
        float top = ScrollTrack.Top + (ScrollTrack.Height - h) * _scroll / (_contentHeight - Content.Height);
        return new SKRect(ScrollTrack.Left, top, ScrollTrack.Right, top + h);
    }

    void Open(int page)
    {
        _page = page;
        _scroll = 0;
    }

    // ---- Drawing ----

    public void Draw(SKCanvas canvas)
    {
        _hits.Clear();
        using var fill = new SKPaint { IsAntialias = true };
        using var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
        using var text = new SKPaint { IsAntialias = true };
        using var titleFont = new SKFont(_player.MenuTypeface, 22);
        using var indexFont = new SKFont(_player.MenuTypeface, 13);
        fill.Color = Back;
        canvas.DrawRect(Panel, fill);
        stroke.Color = Grey;
        canvas.DrawRect(Panel, stroke);
        canvas.DrawLine(Panel.Left + 8, Panel.Top + TitleH, Panel.Right - 8, Panel.Top + TitleH, stroke);
        canvas.DrawLine(IndexArea.Right + 8, Panel.Top + TitleH + 8, IndexArea.Right + 8, Panel.Bottom - 8, stroke);
        text.Color = SKColors.White;
        canvas.DrawText(Lang.T("Help"), Panel.MidX - titleFont.MeasureText(Lang.T("Help")) / 2, Panel.Top + 30, titleFont, text);
        MenuGlyphs.DrawClose(canvas, CloseBox, CloseBox.Contains(_mouse));

        // The index: a button for each entry, the open one lit.
        float y = IndexArea.Top;
        for (int i = 0; i < _index.Count; i++)
        {
            var lines = Wrap(Lang.T(_index[i].Title), indexFont, IndexArea.Width - 16);
            float h = 12 + lines.Count * 16;
            var r = new SKRect(IndexArea.Left, y, IndexArea.Right, y + h);
            bool open = i == _page, hover = r.Contains(_mouse);
            // Lit when open or pointed at (as the settings' buttons are); the open one's right side a point, towards its page.
            using var tab = new SKPath();
            tab.MoveTo(r.Left, r.Top);
            if (open)
            {
                // (Its box carried on past the column's edge, then the point, over the divider and up to just short of
                // the page's text, which starts at Content.Left.)
                float tip = Content.Left - 3, start = tip - 9;
                tab.LineTo(start, r.Top);
                tab.LineTo(tip, r.MidY);
                tab.LineTo(start, r.Bottom);
            }
            else
            {
                tab.LineTo(r.Right, r.Top);
                tab.LineTo(r.Right, r.Bottom);
            }
            tab.LineTo(r.Left, r.Bottom);
            tab.Close();
            fill.Color = open || hover ? BoxLit : Box;
            canvas.DrawPath(tab, fill);
            stroke.Color = open || hover ? Orange : Grey;
            canvas.DrawPath(tab, stroke);
            text.Color = open || hover ? SKColors.White : Orange;
            for (int l = 0; l < lines.Count; l++)
                canvas.DrawText(lines[l], r.MidX - indexFont.MeasureText(lines[l]) / 2, r.Top + 18 + l * 16, indexFont, text);
            int page = i;
            _hits.Add((r, () => Open(page)));
            y += h + 6;
        }
        DrawGlobe(canvas, new SKRect(IndexArea.Left, y + 14, IndexArea.Right, IndexArea.Bottom - 8));

        // The page, scrolled, inside its area.
        canvas.Save();
        canvas.ClipRect(Content);
        if (_page < _index.Count && _pages.TryGetValue(_index[_page].Page, out var p))
            _contentHeight = DrawPage(canvas, p, Content.Top - _scroll) - (Content.Top - _scroll);
        canvas.Restore();
        ScrollTo(_scroll);
        UpdateCursor();
        if (Thumb() is { } thumb)
        {
            fill.Color = new SKColor(0x1A, 0x1A, 0x1A);
            canvas.DrawRoundRect(ScrollTrack, 3, 3, fill);
            fill.Color = _dragging || thumb.Contains(_mouse) ? SKColors.White : Orange;
            canvas.DrawRoundRect(thumb, 3, 3, fill);
        }
    }

    /// <summary>Draws a page from y (its scrolled top); returns where it ends.</summary>
    float DrawPage(SKCanvas canvas, (string Title, List<JsonObject> Blocks, List<Area> Areas) page, float y)
    {
        using var headingFont = new SKFont(_player.MenuTypeface, 15);
        var heading = new TextFont(headingFont);
        var body = DialogFont;
        using var paint = new SKPaint { IsAntialias = true };
        const float Line = 17, Gap = 9;
        float left = Content.Left, width = Content.Width;
        // A picture set to the left: the text beside it until below it.
        float floatBottom = float.MinValue, floatWidth = 0;
        float Avail(float at) => at < floatBottom ? width - floatWidth : width;
        float Left(float at) => at < floatBottom ? left + floatWidth : left;

        // A paragraph: its words (and inline pictures) wrapped into the space beside any picture to the left.
        float Paragraph(List<(string? Word, JsonObject? Picture)> tokens, float at, TextFont font, SKColor color, SKRect? hit = null, Action? click = null)
        {
            float x = 0, lineTop = at, lineHeight = Line;
            var placed = new List<(float X, float Y, string? Word, JsonObject? Picture, SKSize Size)>();
            float space = font.Measure(" ");
            foreach (var (word, picture) in tokens)
            {
                var size = picture != null ? PictureSize(picture) : new SKSize(font.Measure(word!), Line);
                float w = size.Width;
                if (x > 0 && x + w > Avail(lineTop)) { x = 0; lineTop += lineHeight; lineHeight = Line; }
                // (A picture in the line makes it taller: it sits from the text's top, so the next line goes below it.)
                if (picture != null) lineHeight = Math.Max(lineHeight, size.Height);
                placed.Add((x, lineTop, word, picture, size));
                x += w + space;
            }
            float end = lineTop + lineHeight;
            // Its click area: from its first line's top to its last's descenders (end is where the next line would
            // start: going down to it would overlap the next paragraph's area, and a click there would go to this one).
            var area = new SKRect(Content.Left, at - 13, Content.Right, end - Line + 4);
            bool hover = click != null && area.Contains(_mouse) && Content.Contains(_mouse);
            if (hover)
            {
                // Pointed at: lit as the settings' buttons are, a box behind the whole line.
                var lit = new SKRect(Content.Left - 4, area.Top - 3, Content.Right + 2, area.Bottom + 3);
                using var box = new SKPaint { IsAntialias = true, Color = BoxLit };
                canvas.DrawRoundRect(lit, 3, 3, box);
                box.Style = SKPaintStyle.Stroke; box.StrokeWidth = 1; box.Color = Orange;
                canvas.DrawRoundRect(lit, 3, 3, box);
            }
            paint.Color = hover ? SKColors.White : color;
            foreach (var (px, py, word, picture, size) in placed)
            {
                float lx = Left(py) + px;
                if (picture != null) DrawPicture(canvas, picture, new SKRect(lx, py - 13, lx + size.Width, py - 13 + size.Height));
                else font.Draw(canvas, word!, lx, py, paint);
            }
            if (click != null) _hits.Add((new SKRect(Content.Left, Math.Max(area.Top, Content.Top), Content.Right, Math.Min(area.Bottom, Content.Bottom)), click));
            return end;
        }
        // "More...": a small button under a question's hints, for its next one (and the Report page's link buttons).
        float MoreButton(float at, Action click, string? label = null)
        {
            label ??= Lang.T("More...");
            float w = body.Measure(label) + 16;
            var r = new SKRect(left, at - 12, left + w, at + 6);
            bool hover = r.Contains(_mouse) && Content.Contains(_mouse);
            using var box = new SKPaint { IsAntialias = true, Color = hover ? BoxLit : Box };
            canvas.DrawRoundRect(r, 3, 3, box);
            box.Style = SKPaintStyle.Stroke; box.StrokeWidth = 1; box.Color = hover ? Orange : Grey;
            canvas.DrawRoundRect(r, 3, 3, box);
            paint.Color = hover ? SKColors.White : Orange;
            body.Draw(canvas, label, r.Left + 8, at + 1, paint);
            if (r.Bottom > Content.Top && r.Top < Content.Bottom) _hits.Add((r, click));
            return at + Line + 8;
        }
        static List<(string?, JsonObject?)> Words(string s) => s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(w => ((string?)w, (JsonObject?)null)).ToList();

        y += 14;
        var blocks = page.Blocks;
        for (int i = 0; i < blocks.Count; i++)
        {
            var b = blocks[i];
            if (b["heading"]?.GetValue<string>() is { } head)
            {
                if (i > 0 && blocks[i - 1]["heading"] == null) y += 8; // (a new section: a little apart from the last)
                y = Paragraph(Words(head), y, heading, SKColors.White) + 2;
            }
            else if (b["key"]?.GetValue<string>() is { } keys)
            {
                // A control: its keys in a key cap on the left, what they do beside it.
                const float CapWidth = 150;
                using var capFont = new SKFont(_player.MenuTypeface, 11.5f);
                var cap = new SKRect(left, y - 13, left + CapWidth, y + 7);
                using (var box = new SKPaint { IsAntialias = true, Color = BoxLit })
                {
                    canvas.DrawRoundRect(cap, 4, 4, box);
                    box.Style = SKPaintStyle.Stroke; box.StrokeWidth = 1; box.Color = Grey;
                    canvas.DrawRoundRect(cap, 4, 4, box);
                }
                paint.Color = SKColors.White;
                canvas.DrawText(keys, cap.MidX - capFont.MeasureText(keys) / 2, y + 1, capFont, paint);
                left += CapWidth + 12; width -= CapWidth + 12;
                y = Math.Max(Paragraph(Words(b["text"]?.GetValue<string>() ?? ""), y, body, Body), cap.Bottom + 13) + Gap;
                left -= CapWidth + 12; width += CapWidth + 12;
            }
            else if (b["link"]?.GetValue<string>() is { } linkLabel && b["url"]?.GetValue<string>() is { } url)
            {
                // A link: a button that opens a web page in the browser (the Report page's GitHub forms).
                y = MoreButton(y, () => _player.Host?.OpenUrl(url), linkLabel) + 2;
            }
            else if (b["picture"] is JsonObject pic && b["align"]?.GetValue<string>() == "left")
            {
                if (y - 13 < floatBottom) y = floatBottom + 13;
                var size = PictureSize(pic);
                DrawPicture(canvas, pic, new SKRect(left, y - 13, left + size.Width, y - 13 + size.Height));
                floatWidth = size.Width + 12;
                floatBottom = y - 13 + size.Height + 4;
            }
            else if (b["picture"] is JsonObject wide && (b["align"]?.GetValue<string>() != "inline" || wide["decoder"]?.GetValue<bool>() == true))
            {
                // A picture on its own (the decoder's tablet, across the page).
                var size = PictureSize(wide);
                DrawPicture(canvas, wide, new SKRect(left, y - 13, left + size.Width, y - 13 + size.Height));
                y += size.Height + Gap;
            }
            else
            {
                // Text, with any small pictures in its line (the turn-around symbol) and the text after them: one paragraph.
                var tokens = new List<(string?, JsonObject?)>();
                for (; i < blocks.Count; i++)
                {
                    var t = blocks[i];
                    if (t["text"]?.GetValue<string>() is { } s) tokens.AddRange(Words(s));
                    else if (t["picture"] is JsonObject inline && t["align"]?.GetValue<string>() == "inline" && inline["decoder"] == null) tokens.Add((null, inline));
                    else { i--; break; }
                    // (Joined with what follows only through an inline picture.)
                    bool nextInline = i + 1 < blocks.Count && blocks[i + 1]["picture"] is JsonObject n && blocks[i + 1]["align"]?.GetValue<string>() == "inline" && n["decoder"] == null;
                    bool afterInline = t["picture"] != null && i + 1 < blocks.Count && blocks[i + 1]["text"] != null;
                    if (!nextInline && !afterInline) break;
                }
                y = Paragraph(tokens, y, body, Body) + Gap;
            }
        }
        y = Math.Max(y, floatBottom + 13);

        // Tips and Hints: the areas, the open one's questions, and the hints shown so far for each.
        for (int a = 0; a < page.Areas.Count; a++)
        {
            var area = page.Areas[a];
            int ai = a;
            // (A triangle before it: pointing right while closed, down while open.)
            bool isOpen = _openArea == a;
            float headingAt = y;
            left = Content.Left + 16; width = Content.Width - 16;
            y = Paragraph(Words(area.Name), y, heading, Orange, null, () => { _openArea = _openArea == ai ? null : ai; }) + 4;
            // (After its heading, so the heading's hover box is behind it; white with the heading when that's lit.)
            bool headingLit = paint.Color == SKColors.White;
            using (var tri = new SKPath())
            {
                float tx = Content.Left + 2, ty = headingAt - 10;
                if (isOpen) { tri.MoveTo(tx, ty + 1); tri.LineTo(tx + 10, ty + 1); tri.LineTo(tx + 5, ty + 9); }
                else { tri.MoveTo(tx + 1, ty - 1); tri.LineTo(tx + 9, ty + 4); tri.LineTo(tx + 1, ty + 9); }
                tri.Close();
                paint.Color = headingLit ? SKColors.White : Orange;
                canvas.DrawPath(tri, paint);
            }
            left = Content.Left; width = Content.Width;
            if (_openArea != a) continue;
            for (int q = 0; q < area.Questions.Count; q++)
            {
                var (question, hints) = area.Questions[q];
                int qi = q;
                left = Content.Left + 28; width = Content.Width - 28;
                // (A question opens with its first hint, and closes again; "More..." under it shows the next.)
                y = Paragraph(Words(question), y, body, Orange, SKRect.Empty, () =>
                {
                    _shown.TryGetValue((ai, qi), out int n);
                    _shown[(ai, qi)] = n > 0 ? 0 : Math.Min(1, hints.Count);
                });
                _shown.TryGetValue((a, q), out int shown);
                left = Content.Left + 42; width = Content.Width - 42;
                for (int h = 0; h < shown; h++) y = Paragraph(Words("- " + hints[h]), y, body, Body);
                if (shown > 0 && shown < hints.Count)
                    y = MoreButton(y, () => _shown[(ai, qi)] = Math.Min(_shown.GetValueOrDefault((ai, qi)) + 1, hints.Count));
                left = Content.Left; width = Content.Width;
                y += 5;
            }
            y += 6;
        }
        return y + 10;
    }

    // ---- Pictures: the game's own frames ----

    /// <summary>A picture's size on the page (stage pixels): "width"/"height", or the decoder's tablet across the page.</summary>
    static SKSize PictureSize(JsonObject p)
    {
        if (p["decoder"]?.GetValue<bool>() == true) return new SKSize(Content.Width - 4, (Content.Width - 4) * 0.21f);
        if (p["cursor"]?.GetValue<string>() is { } c) return c == "turn" ? new SKSize(22, 20) : new SKSize(16, 22);
        return new SKSize((float)(p["width"]?.GetValue<double>() ?? 200), (float)(p["height"]?.GetValue<double>() ?? 120));
    }

    void DrawPicture(SKCanvas canvas, JsonObject p, SKRect r)
    {
        if (r.Bottom < Content.Top || r.Top > Content.Bottom) return;
        if (p["decoder"]?.GetValue<bool>() == true) { DrawDecoder(canvas, r); return; }
        if (p["cursor"]?.GetValue<string>() is { } cursor) { DrawCursor(canvas, cursor, r); return; }
        if (p["scene"] is null) return;
        string key = p.ToJsonString();
        if (!_scenes.TryGetValue(key, out var root))
        {
            // (A scene copy as a live backdrop would make it: frame, sky, parts hidden; inert.)
            var b = LiveBackdrop.Parse(p);
            _scenes[key] = root = b?.Build(_player);
        }
        if (root == null) return;
        float zoom = (float)(p["zoom"]?.GetValue<double>() ?? 1), sx = (float)(p["x"]?.GetValue<double>() ?? 0), sy = (float)(p["y"]?.GetValue<double>() ?? 0);
        canvas.Save();
        canvas.ClipRect(r);
        using (var black = new SKPaint { Color = SKColors.Black }) canvas.DrawRect(r, black);
        var ctx = new Mnog.Engine.Render.RenderContext(canvas, canvas.TotalMatrix, _player, 1);
        var world = SKMatrix.CreateTranslation(r.Left, r.Top).PreConcat(SKMatrix.CreateScale(zoom, zoom)).PreConcat(SKMatrix.CreateTranslation(-sx, -sy));
        ctx.DrawContainer(root.Children, world, CxForm.Identity);
        canvas.Restore();
        using var border = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = Grey };
        canvas.DrawRect(r, border);
    }

    /// <summary>The loading screen's globe of Mata Nui (the player movie's clip 34: the island in its ring), still,
    /// fitted into the space below the index. Only its picture: the shading that sweeps over it on the loading screen
    /// (clip 33, its depth 2) is left out here, as its first frame is a dark overlay.</summary>
    void DrawGlobe(SKCanvas canvas, SKRect space)
    {
        if (!_scenes.TryGetValue("globe", out var globe) && _player.GetMovie("player") is { } player && player.Symbol(34) is Mnog.Engine.Assets.SpriteAsset sprite)
        {
            globe = new MovieClip(_player, player, sprite.Timeline) { Inert = true };
            globe.Construct();
            if (globe.Children.TryGetValue(2, out var shading)) shading.Visible = false;
            _scenes["globe"] = globe;
        }
        if (globe == null || space.Height < 40) return;
        var b = globe.LocalBounds;
        if (b.Width <= 0 || b.Height <= 0) return;
        // (No larger than it's drawn on the loading screen.)
        float s = Math.Min(1, Math.Min(space.Width / b.Width, space.Height / b.Height));
        var at = SKMatrix.CreateTranslation(space.MidX - b.MidX * s, space.MidY - b.MidY * s).PreConcat(SKMatrix.CreateScale(s, s));
        canvas.Save();
        var ctx = new Mnog.Engine.Render.RenderContext(canvas, canvas.TotalMatrix, _player, 1);
        ctx.DrawContainer(globe.Children, at, CxForm.Identity);
        canvas.Restore();
    }

    /// <summary>The cursors: the arrow and the pointing finger as the system draws them; the turn-around symbol as the game
    /// draws it (the player movie's mouseturnleft clip).</summary>
    void DrawCursor(SKCanvas canvas, string which, SKRect r)
    {
        if (which == "turn")
        {
            if (!_scenes.TryGetValue("cursor:turn", out var turn) && _player.GetMovie("player") is { } player && player.Symbol(17) is Mnog.Engine.Assets.SpriteAsset sprite)
            {
                turn = new MovieClip(_player, player, sprite.Timeline) { Inert = true };
                turn.Construct();
                _scenes["cursor:turn"] = turn;
            }
            if (turn == null) return;
            // (Its art spans 54 x 49 about its centre.)
            float s = r.Width / 54;
            canvas.Save();
            var ctx = new Mnog.Engine.Render.RenderContext(canvas, canvas.TotalMatrix, _player, 1);
            ctx.DrawContainer(turn.Children, SKMatrix.CreateTranslation(r.MidX, r.MidY).PreConcat(SKMatrix.CreateScale(s, s)), CxForm.Identity);
            canvas.Restore();
            return;
        }
        using var path = new SKPath();
        if (which == "finger")
        {
            // A pointing hand: the raised finger, the folded fingers and the thumb (a 16 x 22 cell).
            path.MoveTo(5, 1.5f); path.CubicTo(5, 0.2f, 7.2f, 0.2f, 7.2f, 1.5f); path.LineTo(7.2f, 8.5f);
            path.CubicTo(7.4f, 7.4f, 9.4f, 7.4f, 9.4f, 8.8f); path.CubicTo(9.6f, 7.8f, 11.6f, 7.8f, 11.6f, 9.4f);
            path.CubicTo(11.8f, 8.5f, 13.8f, 8.5f, 13.8f, 10); path.LineTo(13.8f, 15); path.CubicTo(13.8f, 18, 12.4f, 20.5f, 10, 20.5f);
            path.LineTo(7.2f, 20.5f); path.CubicTo(5.6f, 20.5f, 4.8f, 19.6f, 3.9f, 18.3f); path.LineTo(0.9f, 13.6f);
            path.CubicTo(0.2f, 12.5f, 1.6f, 11.1f, 2.7f, 12); path.LineTo(5, 14); path.Close();
        }
        else
        {
            // The arrow (a 16 x 22 cell).
            path.MoveTo(1, 1); path.LineTo(1, 17); path.LineTo(5, 13.2f); path.LineTo(8, 20); path.LineTo(10.6f, 18.9f);
            path.LineTo(7.7f, 12.3f); path.LineTo(13, 12.3f); path.Close();
        }
        canvas.Save();
        canvas.Translate(r.Left, r.Top);
        canvas.Scale(r.Width / 16, r.Height / 22);
        using (var white = new SKPaint { IsAntialias = true, Color = SKColors.White }) canvas.DrawPath(path, white);
        using (var black = new SKPaint { IsAntialias = true, Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 1, StrokeJoin = SKStrokeJoin.Round })
            canvas.DrawPath(path, black);
        canvas.Restore();
    }

    /// <summary>
    /// The Bionicle decoder: a stone tablet of the alphabet, each letter over its symbol, a circle with lines and small
    /// rings (as the site's decoder picture has them).
    /// </summary>
    void DrawDecoder(SKCanvas canvas, SKRect r)
    {
        using var stone = new SKPaint { IsAntialias = true };
        stone.Shader = SKShader.CreateLinearGradient(new SKPoint(r.Left, r.Top), new SKPoint(r.Right, r.Bottom),
            [new SKColor(0xC9, 0xA2, 0x75), new SKColor(0xB0, 0x87, 0x5A), new SKColor(0xA2, 0x7A, 0x4F)], [0, 0.6f, 1], SKShaderTileMode.Clamp);
        canvas.DrawRoundRect(r, 6, 6, stone);
        using var edge = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, Color = new SKColor(0x6E, 0x4E, 0x2E) };
        canvas.DrawRoundRect(r, 6, 6, edge);
        using var ink = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.2f, Color = new SKColor(0x2A, 0x1E, 0x14) };
        using var letters = new SKPaint { IsAntialias = true, Color = new SKColor(0x2A, 0x1E, 0x14) };
        float cellW = r.Width / 13, rowH = r.Height / 2;
        float radius = Math.Min(cellW * 0.32f, rowH * 0.24f);
        using var font = new SKFont(_player.MenuTypeface, radius * 0.95f);
        for (int i = 0; i < 26; i++)
        {
            char letter = (char)('A' + i);
            float cx = r.Left + cellW * (i % 13 + 0.5f), rowTop = r.Top + rowH * (i / 13);
            float cy = rowTop + rowH * 0.62f;
            canvas.DrawText(letter.ToString(), cx - font.MeasureText(letter.ToString()) / 2, rowTop + rowH * 0.3f, font, letters);
            canvas.DrawCircle(cx, cy, radius, ink);
            SKPoint P(float x, float y) => new(cx + x * radius, cy + y * radius);
            void Line(float x1, float y1, float x2, float y2) => canvas.DrawLine(P(x1, y1), P(x2, y2), ink);
            void Chord(float x) { float h = MathF.Sqrt(1 - x * x); Line(x, -h, x, h); }
            void Across(float y) { float w = MathF.Sqrt(1 - y * y); Line(-w, y, w, y); }
            void Dot(float x, float y) => canvas.DrawCircle(P(x, y), radius * 0.13f, ink);
            const float D = 0.7071f;
            switch (letter)
            {
                case 'A': Dot(-0.35f, 0.55f); break;
                case 'B': Dot(0, -0.55f); Dot(0, 0.55f); break;
                case 'C': Dot(0.55f, 0); break;
                case 'D': Chord(-0.2f); Dot(0.55f, 0); break;
                case 'E': Across(-0.52f); Across(0.52f); break;
                case 'F': Across(-0.45f); break;
                case 'G': Chord(0); Line(0, 0, 1, 0); break;
                case 'H': Chord(-0.5f); Chord(0.5f); Dot(0, -0.63f); Dot(0, 0.63f); break;
                case 'I': Chord(0); break;
                case 'J': Chord(0); Dot(-0.4f, 0.45f); break;
                case 'K': Line(-1, 0, 0, 0); Line(0, 0, 0.766f, -0.643f); Line(0, 0, 0.766f, 0.643f); break;
                case 'L': Chord(0); Dot(0.35f, 0.55f); break;
                case 'M': Chord(-0.47f); Chord(0.47f); break;
                case 'N': Line(-D, -D, D, D); break;
                case 'P': Chord(0); Dot(0.45f, -0.4f); break;
                case 'Q': Dot(0.5f, 0.5f); break;
                case 'R': Chord(0); Line(0, 0, 1, 0); Dot(0.45f, -0.4f); break;
                case 'S': Dot(-0.5f, 0.45f); Dot(0.5f, -0.4f); break;
                case 'T': Across(0); Line(0, 0, 0, 1); break;
                case 'U': Dot(0, -0.55f); break;
                case 'V': Line(-D, -D, D, D); Line(-D, D, D, -D); Dot(0, -0.55f); break;
                case 'W': Line(-D, -D, D, D); Line(-D, D, D, -D); Dot(-0.55f, 0); Dot(0.55f, 0); break;
                case 'X': Line(-D, -D, D, D); Line(-D, D, D, -D); break;
                case 'Y': Line(-D, -D, D, D); Line(0, 0, D, -D); break;
                case 'Z': Line(-D, D, D, -D); break;
            }
        }
    }

    static List<string> Wrap(string s, SKFont font, float width)
    {
        var lines = new List<string>();
        string line = "";
        foreach (var w in s.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            string next = line.Length == 0 ? w : line + " " + w;
            if (line.Length > 0 && font.MeasureText(next) > width) { lines.Add(line); line = w; }
            else line = next;
        }
        if (line.Length > 0) lines.Add(line);
        return lines;
    }
}

using System.Net;
using System.Text.RegularExpressions;
using Mnog.Engine.Assets;
using Mnog.Engine.Script;
using SkiaSharp;

namespace Mnog.Engine.Display;

/// <summary>A dynamic or input text field, optionally bound to a variable.</summary>
public sealed partial class EditTextObject : DisplayObject
{
    public readonly EditTextAsset Field;
    string _text;
    bool _bound;

    public EditTextObject(Player player, EditTextAsset e) : base(player, player.Vm.ObjectPrototype)
    {
        Field = e;
        Asset = e;
        _text = Tweaks.Matoran(e.Doc.Text ?? "");
        if (e.Doc.Html == true) _text = StripHtml(_text);
    }

    public bool Editable => Field.Doc.ReadOnly != true;

    /// <summary>Smooth mode: the text at the previous game frame (the dialog's typing is revealed in between).</summary>
    public string? PrevText;

    /// <summary>The text shown: the bound variable's value if any, else the field's own text.</summary>
    public string Text
    {
        get
        {
            if (Field.Doc.Variable is { Length: > 0 } v && Parent != null)
            {
                var val = Player.Vm.GetVariableAt(Parent, v);
                if (val.IsUndefined)
                {
                    // Flash initializes an unset bound variable with the field's text, once; a variable that
                    // becomes undefined later shows as empty (the backpack's label over an empty slot).
                    if (_bound) return "";
                    _bound = true;
                    if (_text.Length > 0) Player.Vm.SetVariableAt(Parent, v, _text);
                    return _text;
                }
                _bound = true;
                var s = val.ToStr(Parent.Movie.Version);
                return Field.Doc.Html == true ? StripHtml(s) : s;
            }
            return _text;
        }
        set
        {
            if (Field.Doc.MaxLength is int max && value.Length > max) value = value[..max];
            _text = value;
            if (Field.Doc.Variable is { Length: > 0 } v && Parent != null) Player.Vm.SetVariableAt(Parent, v, value);
        }
    }

    public override SKRect LocalBounds => Field.Bounds;

    public override void Render(Mnog.Engine.Render.RenderContext ctx, SKMatrix world, CxForm cx) => ctx.DrawEditText(this, world, cx);

    public override bool HitTest(SKPoint p, SKMatrix world) =>
        world.TryInvert(out var inv) && Field.Bounds.Contains(inv.MapPoint(p));

    public override void OnRemoved()
    {
        base.OnRemoved();
        Player.OnTextRemoved(this);
    }

    public override bool TryGetOwn(string name, out Value v)
    {
        if (name.Equals("text", StringComparison.OrdinalIgnoreCase)) { v = Text; return true; }
        return base.TryGetOwn(name, out v);
    }

    public override void SetOwn(string name, Value v)
    {
        if (name.Equals("text", StringComparison.OrdinalIgnoreCase)) { Text = v.ToStr(6); return; }
        base.SetOwn(name, v);
    }

    public static string StripHtml(string html)
    {
        var s = BreakTags().Replace(html, "\r");
        s = AnyTag().Replace(s, "");
        return WebUtility.HtmlDecode(s);
    }

    [GeneratedRegex("<(br|/p)\\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex BreakTags();

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex AnyTag();
}

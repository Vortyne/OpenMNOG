using Mnog.Engine.Assets;
using Mnog.Engine.Script;
using SkiaSharp;

namespace Mnog.Engine.Display;

/// <summary>
/// Base of everything on the display list. Display objects are also script objects: a movie
/// clip's variables are its properties, and _x/_alpha/... are exposed as properties.
/// </summary>
public abstract class DisplayObject : AsObject
{
    public readonly Player Player;
    public MovieClip? Parent;
    public int Depth;
    public string Name = "";
    public SymbolAsset? Asset;

    SKMatrix _matrix = SKMatrix.Identity;
    public CxForm ColorTransform = CxForm.Identity;
    public float Ratio;
    public int ClipDepth;
    public bool Visible = true;
    public bool Removed;

    /// <summary>Set once a script changes the transform: the timeline then stops moving this object.</summary>
    public bool TransformedByScript;
    /// <summary>Frame on which the timeline placed this instance (for rewinds); 0 for script-created objects.</summary>
    public int PlaceFrame;

    // Decomposed transform, as Flash keeps it for _xscale/_yscale/_rotation.
    double _scaleX = 1, _scaleY = 1, _rotX, _rotY;
    bool _decomposed;

    protected DisplayObject(Player player, AsObject? proto) : base(proto) => Player = player;

    public SKMatrix Matrix
    {
        get => _matrix;
        set { _matrix = value; _decomposed = false; }
    }

    void Decompose()
    {
        if (_decomposed) return;
        var m = _matrix;
        _scaleX = Math.Sqrt(m.ScaleX * m.ScaleX + m.SkewY * m.SkewY);
        _scaleY = Math.Sqrt(m.SkewX * m.SkewX + m.ScaleY * m.ScaleY);
        _rotX = Math.Atan2(m.SkewY, m.ScaleX);
        _rotY = Math.Atan2(-m.SkewX, m.ScaleY);
        _decomposed = true;
    }

    void Recompose()
    {
        _matrix = new SKMatrix(
            (float)(_scaleX * Math.Cos(_rotX)), (float)(-_scaleY * Math.Sin(_rotY)), _matrix.TransX,
            (float)(_scaleX * Math.Sin(_rotX)), (float)(_scaleY * Math.Cos(_rotY)), _matrix.TransY,
            0, 0, 1);
    }

    public double X
    {
        get => _matrix.TransX;
        set { _matrix.TransX = (float)value; TransformedByScript = true; }
    }

    public double Y
    {
        get => _matrix.TransY;
        set { _matrix.TransY = (float)value; TransformedByScript = true; }
    }

    public double XScale
    {
        get { Decompose(); return _scaleX * 100; }
        set { Decompose(); _scaleX = value / 100; Recompose(); TransformedByScript = true; }
    }

    public double YScale
    {
        get { Decompose(); return _scaleY * 100; }
        set { Decompose(); _scaleY = value / 100; Recompose(); TransformedByScript = true; }
    }

    public double Rotation
    {
        get { Decompose(); return Math.Round(_rotX * 180 / Math.PI, 10); }
        set
        {
            Decompose();
            double r = value % 360;
            if (r > 180) r -= 360; else if (r < -180) r += 360;
            double delta = r * Math.PI / 180 - _rotX;
            _rotX += delta;
            _rotY += delta;
            Recompose();
            TransformedByScript = true;
        }
    }

    public double Alpha
    {
        get => ColorTransform.AM * 100;
        set { ColorTransform = ColorTransform with { AM = (float)(value / 100) }; TransformedByScript = true; }
    }

    /// <summary>Bounds in this object's own coordinates.</summary>
    public abstract SKRect LocalBounds { get; }

    public SKRect BoundsInParent => Matrix.MapRect(LocalBounds);

    public double Width
    {
        get => BoundsInParent.Width;
        set
        {
            var w = BoundsInParent.Width;
            if (w > 0) XScale = XScale * value / w;
        }
    }

    public double Height
    {
        get => BoundsInParent.Height;
        set
        {
            var h = BoundsInParent.Height;
            if (h > 0) YScale = YScale * value / h;
        }
    }

    // ---- Smooth mode: drawing between game frames ----

    /// <summary>Transform and color at the start of the current game frame (recorded while smooth mode is on).</summary>
    public SKMatrix PrevMatrix;
    public CxForm PrevColor;
    /// <summary>Whether this frame's change is interpolated when drawing between frames.</summary>
    public bool LerpMatrix, LerpColor;
    /// <summary>Rotation (radians) made this frame, interpolated along with the movement.</summary>
    public float LerpAngle;
    /// <summary>The object existed when the current game frame began.</summary>
    public bool Snapshotted;
    /// <summary>A clip with nothing on its frame when last snapshotted (set by Player.Snapshot).</summary>
    public bool PrevEmpty;

    /// <summary>
    /// Widescreen only: a horizontal stretch about the object's middle (an art fix for a backdrop a little narrower
    /// than the wider view; see Patches.ArtFixes).
    /// </summary>
    public float WidescreenScaleX = 1;
    /// <summary>Widescreen only: a vertical stretch down from the object's top (its bottom reaching further).</summary>
    public float WidescreenScaleY = 1;
    /// <summary>Set: only the part below this y (own coordinates) is stretched by WidescreenScaleY, from there down.</summary>
    public float? WidescreenSplitY;
    /// <summary>Where along its width the stretch is about: 0 keeps the left edge, 1 the right, 0.5 the middle.</summary>
    public float WidescreenAnchor = 0.5f;
    /// <summary>
    /// Set: the stretch is about this x in the object's own coordinates instead (a clip whose bounds change as it
    /// animates, e.g. a beach with waves washing over it, would otherwise be stretched about a moving point).
    /// </summary>
    public float? WidescreenAnchorX;
    /// <summary>
    /// Set: only the part right of this point (a fraction of the width) is stretched, from there; the rest is drawn
    /// as it is (RenderContext draws the two parts), so nothing left of it shifts. A mask is stretched about it.
    /// </summary>
    public float? WidescreenSplit;
    /// <summary>
    /// Set too (with WidescreenSplit): the part left of this point is also stretched, from there leftwards, so both
    /// ends of a backdrop reach further while its middle (where things stand on it) stays as it is.
    /// </summary>
    public float? WidescreenSplitLeft;

    /// <summary>
    /// The extent a widescreen stretch is measured against: the bounds, less any child the game has hidden by moving
    /// it far off the stage (as IV.4-N does with its shut gates, at x 2000), which would otherwise move the split
    /// points and the anchor.
    /// </summary>
    public virtual SKRect StretchBounds => LocalBounds;

    /// <summary>The split points given as x in the object's own coordinates (in place of the fractions).</summary>
    public float? WidescreenSplitAt, WidescreenSplitLeftAt;
    /// <summary>The scale of the part left of the left split, when it differs from the right's.</summary>
    public float? WidescreenScaleLeft;

    /// <summary>The split point in the object's own coordinates.</summary>
    public float SplitX => WidescreenSplitAt ?? StretchBounds.Left + StretchBounds.Width * (WidescreenSplit ?? 0);
    /// <summary>The left split point in the object's own coordinates.</summary>
    public float SplitLeftX => WidescreenSplitLeftAt ?? StretchBounds.Left + StretchBounds.Width * (WidescreenSplitLeft ?? 0);

    /// <summary>Smooth mode, slowed scenes: the whole transform (scale, skew) is blended, not just the position.</summary>
    public bool LerpFull;

    /// <summary>The change made in the previous game frame (position, scale), for telling loop resets apart.</summary>
    public (float X, float Y, float S) LastStep;

    /// <summary>Transform to draw with, at fraction t (0..1) of the way from the previous game frame.</summary>
    public SKMatrix DrawMatrix(float t)
    {
        var m = InterpolatedMatrix(t);
        // About the middle of the object, so it widens to both sides.
        // (With SplitY, the vertical stretch is the lower part's, drawn by RenderContext: the whole is drawn unstretched here.)
        float sy = WidescreenSplitY != null ? 1 : WidescreenScaleY;
        if ((WidescreenScaleX == 1 && sy == 1) || !Player.Widescreen) return m;
        var sb = StretchBounds;
        float anchor = WidescreenSplit != null ? SplitX : WidescreenAnchorX ?? sb.Left + sb.Width * WidescreenAnchor;
        return m.PreConcat(SKMatrix.CreateScale(WidescreenScaleX, sy, anchor, sb.Top));
    }

    /// <summary>The transform to draw with, without the widescreen stretch.</summary>
    public SKMatrix UnstretchedDrawMatrix(float t) => InterpolatedMatrix(t);

    SKMatrix InterpolatedMatrix(float t)
    {
        if (!LerpMatrix || t >= 1) return _matrix;
        var m = _matrix;
        if (LerpAngle != 0)
        {
            // Turned by a script (e.g. the telescope's sky): rotate back by the part of the turn not yet shown.
            var r = SKMatrix.CreateRotation(-(1 - t) * LerpAngle);
            var linear = r.PreConcat(new SKMatrix(m.ScaleX, m.SkewX, 0, m.SkewY, m.ScaleY, 0, 0, 0, 1));
            m.ScaleX = linear.ScaleX; m.SkewX = linear.SkewX; m.SkewY = linear.SkewY; m.ScaleY = linear.ScaleY;
        }
        if (LerpFull)
        {
            m.ScaleX = PrevMatrix.ScaleX + (_matrix.ScaleX - PrevMatrix.ScaleX) * t;
            m.SkewX = PrevMatrix.SkewX + (_matrix.SkewX - PrevMatrix.SkewX) * t;
            m.SkewY = PrevMatrix.SkewY + (_matrix.SkewY - PrevMatrix.SkewY) * t;
            m.ScaleY = PrevMatrix.ScaleY + (_matrix.ScaleY - PrevMatrix.ScaleY) * t;
        }
        m.TransX = PrevMatrix.TransX + (_matrix.TransX - PrevMatrix.TransX) * t;
        m.TransY = PrevMatrix.TransY + (_matrix.TransY - PrevMatrix.TransY) * t;
        return m;
    }

    public CxForm DrawColor(float t)
    {
        if (!LerpColor || t >= 1) return ColorTransform;
        var a = PrevColor;
        var b = ColorTransform;
        float L(float x, float y) => x + (y - x) * t;
        return new CxForm(L(a.RM, b.RM), L(a.GM, b.GM), L(a.BM, b.BM), L(a.AM, b.AM), L(a.RA, b.RA), L(a.GA, b.GA), L(a.BA, b.BA), L(a.AA, b.AA));
    }

    /// <summary>Concatenated transform from this object's coordinates to stage coordinates.</summary>
    public SKMatrix WorldMatrix => Parent == null ? Matrix : Parent.WorldMatrix.PreConcat(Matrix);

    public MovieClip Root
    {
        get
        {
            DisplayObject o = this;
            while (o.Parent != null) o = o.Parent;
            return (MovieClip)o;
        }
    }

    /// <summary>Slash path: "/holder/Middle-Ground", or "_level1/..." for other levels.</summary>
    public string SlashPath
    {
        get
        {
            if (Parent == null)
                return this is MovieClip { Level: int lv } && lv != 0 ? $"_level{lv}" : "/";
            var parent = Parent.SlashPath;
            return parent == "/" ? "/" + Name : parent + "/" + Name;
        }
    }

    /// <summary>Dot path: "_level0.holder.foo".</summary>
    public string DotPath =>
        Parent == null ? $"_level{(this as MovieClip)?.Level ?? 0}" : Parent.DotPath + "." + Name;

    public abstract void Render(Mnog.Engine.Render.RenderContext ctx, SKMatrix world, CxForm cx);

    /// <summary>Hit test against filled geometry; point in stage coordinates.</summary>
    public abstract bool HitTest(SKPoint stagePoint, SKMatrix world);

    /// <summary>Called when the object leaves the display list.</summary>
    public virtual void OnRemoved() => Removed = true;

    // ---- Properties as seen by scripts ----

    public static int PropertyIndex(string name)
    {
        for (int i = 0; i < Mnog.Script.Builtins.Properties.Length; i++)
            if (Mnog.Script.Builtins.Properties[i].Equals(name, StringComparison.OrdinalIgnoreCase)) return i;
        return -1;
    }

    public virtual Value GetProperty(int index, int version)
    {
        switch (index)
        {
            case 0: return Math.Round(X, 2);
            case 1: return Math.Round(Y, 2);
            case 2: return XScale;
            case 3: return YScale;
            case 4: return 1;
            case 5: return 1;
            case 6: return Alpha;
            case 7: return Value.FromBool(Visible, version);
            case 8: return Math.Round(Width, 2);
            case 9: return Math.Round(Height, 2);
            case 10: return Rotation;
            case 11: return SlashPath;
            case 12: return 1;
            case 13: return Name;
            case 14: return "";
            case 15: return Root is MovieClip r ? r.Movie.Doc.Source : "";
            case 16: return 1;
            case 17: return 1;
            case 18: return 5;
            case 19: return "HIGH";
            case 20: return Math.Round(MouseLocal.X, 0);
            case 21: return Math.Round(MouseLocal.Y, 0);
            default: return Value.Undefined;
        }
    }

    public SKPoint MouseLocal => WorldMatrix.TryInvert(out var inv) ? inv.MapPoint(Player.MouseStage) : default;

    public virtual void SetProperty(int index, Value v, int version)
    {
        double N() => v.ToNumber(version);
        switch (index)
        {
            case 0: if (!double.IsNaN(N())) X = N(); break;
            case 1: if (!double.IsNaN(N())) Y = N(); break;
            case 2: if (!double.IsNaN(N())) XScale = N(); break;
            case 3: if (!double.IsNaN(N())) YScale = N(); break;
            case 6: if (!double.IsNaN(N())) Alpha = N(); break;
            case 7: Visible = v.ToBool(version); break;
            case 8: if (!double.IsNaN(N())) Width = N(); break;
            case 9: if (!double.IsNaN(N())) Height = N(); break;
            case 10: if (!double.IsNaN(N())) Rotation = N(); break;
            case 13: Name = v.ToStr(version); break;
        }
    }

    public override bool TryGetOwn(string name, out Value v)
    {
        if (name.Length > 1 && name[0] == '_')
        {
            int idx = PropertyIndex(name);
            if (idx >= 0) { v = GetProperty(idx, Player.Vm.Version); return true; }
            if (name.Equals("_parent", StringComparison.OrdinalIgnoreCase))
            {
                v = Parent != null ? Value.From(Parent) : Value.Undefined;
                return Parent != null;
            }
            if (name.Equals("_root", StringComparison.OrdinalIgnoreCase)) { v = Value.From(Root); return true; }
        }
        return base.TryGetOwn(name, out v);
    }

    public override void SetOwn(string name, Value v)
    {
        if (name.Length > 1 && name[0] == '_')
        {
            int idx = PropertyIndex(name);
            if (idx >= 0) { SetProperty(idx, v, Player.Vm.Version); return; }
        }
        base.SetOwn(name, v);
    }

    public override Value DefaultValue(Hint hint, int version) => DotPath;
}

/// <summary>Shapes, morph shapes and static text: drawn from their asset, no behavior.</summary>
public sealed class GraphicObject : DisplayObject
{
    public GraphicObject(Player player, SymbolAsset asset) : base(player, null) => Asset = asset;

    public override SKRect LocalBounds => Asset switch
    {
        ShapeAsset s => Player.VectorImages ? s.VectorBounds : s.Bounds,
        MorphAsset m => Lerp(m.StartBounds, m.EndBounds, Ratio),
        TextAsset t => TranslatedBounds(t),
        _ => SKRect.Empty,
    };

    /// <summary>A label's bounds, wide enough for its translation (RenderContext.DrawTranslated draws it centred where the
    /// English was): the drawing and the layer cache keep to the bounds, which would cut a longer one off.</summary>
    SKRect TranslatedBounds(TextAsset t)
    {
        if (t.Doc.Runs.Count != 1 || t.Movie.Name is not ("overlay" or "title") || t.Doc.Runs[0].Text is not { Length: > 0 } english
            || !Lang.Has(english, out var text)) return t.Bounds;
        if (t.TranslatedWidth is not { } known || known.Text != text)
        {
            using var font = new SKFont(Player.MenuTypeface, (float)t.Doc.Runs[0].Size * 1.25f);
            t.TranslatedWidth = known = (text, font.MeasureText(text));
        }
        // (And a little more: a tooltip's box widens to its label and past it, RenderContext.TooltipBox.)
        float grow = Math.Max(0, (known.Width - t.Bounds.Width) / 2) + 8;
        return new SKRect(t.Bounds.Left - grow, t.Bounds.Top - 4, t.Bounds.Right + grow, t.Bounds.Bottom + 2);
    }

    static SKRect Lerp(SKRect a, SKRect b, float t) => new(
        a.Left + (b.Left - a.Left) * t, a.Top + (b.Top - a.Top) * t,
        a.Right + (b.Right - a.Right) * t, a.Bottom + (b.Bottom - a.Bottom) * t);

    public override void Render(Mnog.Engine.Render.RenderContext ctx, SKMatrix world, CxForm cx) => ctx.DrawGraphic(this, world, cx);

    public override bool HitTest(SKPoint p, SKMatrix world) => Mnog.Engine.Render.RenderContext.HitGraphic(this, p, world);
}

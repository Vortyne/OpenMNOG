using Mnog.Engine.Assets;
using SkiaSharp;

namespace Mnog.Engine.Display;

public enum ButtonState { Up, Over, Down }

public sealed class ButtonObject : DisplayObject
{
    public readonly ButtonAsset Button;
    public ButtonState State { get; private set; } = ButtonState.Up;
    /// <summary>Not pressable (the mouse passes over it): e.g. the chronicle's music button for an entry without music.</summary>
    public bool Disabled;
    readonly SortedList<int, DisplayObject> _children = [];
    /// <summary>The objects of the current state (what's drawn).</summary>
    public IList<DisplayObject> StateChildren => _children.Values;
    // Hit-area geometry is never displayed, so it's built once.
    readonly List<DisplayObject> _hit = [];
    readonly MovieClip _host;

    public ButtonObject(Player player, ButtonAsset b) : base(player, player.Vm.ObjectPrototype)
    {
        Button = b;
        Asset = b;
        // State children live in a private clip so they can be built with the normal factory.
        _host = new MovieClip(player, b.Movie, new TimelineAsset(b.Movie, new Format.TimelineDoc { FrameCount = 1 }, ""));
        foreach (var r in b.Records.Where(r => r.Hit))
        {
            var o = Build(r);
            if (o != null) _hit.Add(o);
        }
        SetState(ButtonState.Up, force: true);
    }

    DisplayObject? Build(ButtonAsset.Record r)
    {
        var o = _host.Create(r.Id, r.Depth);
        if (o == null) return null;
        _host.Children.Remove(r.Depth);
        o.Parent = _host;
        o.Matrix = r.Matrix;
        o.ColorTransform = r.Cx;
        if (o is MovieClip mc) mc.Construct();
        return o;
    }

    /// <summary>State children hang off a private host clip that mirrors this button's place in the tree.</summary>
    void SyncHost()
    {
        _host.Parent = Parent;
        _host.Matrix = Matrix;
        _host.Name = Name;
    }

    public void SetState(ButtonState s, bool force = false)
    {
        if (s == State && !force) return;
        SyncHost();
        State = s;
        foreach (var c in _children.Values) c.OnRemoved();
        _children.Clear();
        foreach (var r in Button.Records)
        {
            if (!(s == ButtonState.Up ? r.Up : s == ButtonState.Over ? r.Over : r.Down)) continue;
            var o = Build(r);
            if (o != null) _children[r.Depth] = o;
        }
    }

    public void RunFrame()
    {
        SyncHost();
        foreach (var c in _children.Values.ToArray())
            if (c is MovieClip mc && !mc.Removed) mc.RunFrame();
    }

    public override SKRect LocalBounds
    {
        get
        {
            var r = SKRect.Empty;
            foreach (var c in _children.Values)
            {
                var b = c.BoundsInParent;
                if (!b.IsEmpty) r = r.IsEmpty ? b : SKRect.Union(r, b);
            }
            return r;
        }
    }

    public override void Render(Mnog.Engine.Render.RenderContext ctx, SKMatrix world, CxForm cx) => ctx.DrawContainer(_children, world, cx);

    /// <summary>Buttons are hit-tested against their hit-state shapes only.</summary>
    public override bool HitTest(SKPoint p, SKMatrix world)
    {
        foreach (var c in _hit)
            if (c.HitTest(p, world.PreConcat(c.Matrix))) return true;
        return false;
    }

    public override void OnRemoved()
    {
        base.OnRemoved();
        foreach (var c in _children.Values) c.OnRemoved();
        Player.OnButtonRemoved(this);
    }
}

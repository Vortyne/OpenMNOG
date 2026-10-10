using Mnog.Engine.Assets;
using SkiaSharp;

namespace Mnog.Engine.Display;

/// <summary>An image loaded with loadMovie("x.jpg"), drawn at the clip's origin.</summary>
public sealed class BitmapObject(Player player, SKImage image) : DisplayObject(player, null)
{
    public readonly SKImage Image = image;

    public override SKRect LocalBounds => new(0, 0, Image.Width, Image.Height);

    public override void Render(Mnog.Engine.Render.RenderContext ctx, SKMatrix world, CxForm cx) => ctx.DrawBitmap(Image, world, cx);

    public override bool HitTest(SKPoint p, SKMatrix world) =>
        world.TryInvert(out var inv) && LocalBounds.Contains(inv.MapPoint(p));
}

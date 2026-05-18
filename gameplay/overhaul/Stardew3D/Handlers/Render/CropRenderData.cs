using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using StardewValley;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class CropRenderData : RenderData<CropRenderer>
{
    public CropRenderData(RenderContext ctx, CropRenderer parent)
        : base( ctx, parent)
    {
    }

    public override void Update(RenderContext ctx)
    {
        base.Update(ctx);

        if (instance == null)
        {
            HoeDirtRenderData.cropDirts.TryGetValue(Parent.Object, out var hd);

            Parent.Object.updateDrawMath(hd.Tile);
            if (Parent.Object.currentPhase.Value == 0)
            {
                ctx.WorldSpriteBatch.Begin(Parent.Object.drawPosition, ctx.WorldTransform, orientationOverride: Matrix.CreateLookAt(Vector3.Zero, Vector3.Up, Vector3.Forward) * Matrix.CreateTranslation(Vector3.Up * 0.02f), sameY3d: false);
                Parent.Object.layerDepth = 0;
                Parent.Object.coloredLayerDepth = 0;
            }
            else
                ctx.WorldSpriteBatch.Begin(Parent.Object.drawPosition, ctx.WorldTransform);
            Parent.Object.draw(ctx.WorldSpriteBatch, Parent.Object.tilePosition, (hd?.state.Value == 1 && Parent.Object.currentPhase.Value == 0 && Parent.Object.shouldDrawDarkWhenWatered()) ? (new Color(180, 100, 200) * 1f) : Color.White, hd?.shakeRotation ?? 0);
            ctx.WorldSpriteBatch.End(ctx.WorldBatch);
        }
    }
}

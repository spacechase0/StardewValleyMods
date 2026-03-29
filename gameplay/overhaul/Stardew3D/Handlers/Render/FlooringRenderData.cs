using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using Stardew3D.GameModes;
using Stardew3D.Utilities;
using StardewValley;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class FlooringRenderData : RenderData<FlooringRenderer>
{
    public FlooringRenderData(RenderContext ctx, FlooringRenderer parent)
        : base( ctx, parent )
    {
    }

    public override void Update(RenderContext ctx)
    {
        base.Update(ctx);

        if (instance == null)
        {
            ctx.WorldSpriteBatch.Begin(Parent.Object.getBoundingBox().Center.ToVector2(), ctx.WorldTransform, orientationOverride: Matrix.CreateLookAt(Vector3.Zero, Vector3.Up, Vector3.Forward) * Matrix.CreateTranslation(Vector3.Up * 0.01f), sameY3d: false);
            Parent.Object.draw(ctx.WorldSpriteBatch);
            ctx.WorldSpriteBatch.End(ctx.WorldBatch);
        }
    }
}

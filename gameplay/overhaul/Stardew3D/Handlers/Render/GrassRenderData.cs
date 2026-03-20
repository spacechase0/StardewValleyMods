using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using StardewValley;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class GrassRenderData : RenderData<GrassRenderer>
{
    public GrassRenderData(RenderContext ctx, GrassRenderer parent)
        : base( ctx, parent)
    {
    }

    public override void Update(RenderContext ctx)
    {
        base.Update(ctx);

        if (instance == null)
        {
            ctx.WorldSpriteBatch.Begin(Parent.Object.getBoundingBox().Center.ToVector2(), Matrix.CreateTranslation(0, 0, -0.5f) * ctx.WorldTransform, sameY3d: false);
            Parent.Object.draw(ctx.WorldSpriteBatch);
            ctx.WorldSpriteBatch.End(ctx.WorldBatch);
        }
    }
}

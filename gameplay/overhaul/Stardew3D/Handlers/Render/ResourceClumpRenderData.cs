using Stardew3D.DataModels;
using StardewValley;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class ResourceClumpRenderData : RenderData<ResourceClumpRenderer>
{
    public ResourceClumpRenderData(RenderContext ctx, ResourceClumpRenderer parent)
        : base( ctx, parent)
    {
    }

    public override void Update(RenderContext ctx)
    {
        base.Update(ctx);

        if (instance == null)
        {
            ctx.WorldSpriteBatch.Begin(Parent.Object.getBoundingBox().Center.ToVector2(), ctx.WorldTransform);
            Parent.Object.draw(ctx.WorldSpriteBatch);
            ctx.WorldSpriteBatch.End(ctx.WorldBatch);
        }
    }
}

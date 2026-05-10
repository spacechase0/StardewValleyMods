using Stardew3D.DataModels;
using StardewValley;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;
public class FenceRenderer : ItemRenderer<ModelData, Fence>
{
    public FenceRenderer(Fence obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new FenceRenderData(ctx, this);
    }
}

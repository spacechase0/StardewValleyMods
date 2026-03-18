using Stardew3D.DataModels;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;
public class ObjectRenderer : ItemRenderer<ModelData, StardewValley.Object>
{
    public ObjectRenderer(StardewValley.Object obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new ObjectRenderData(ctx, this);
    }
}

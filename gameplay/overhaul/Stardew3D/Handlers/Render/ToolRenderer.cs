using Stardew3D.DataModels;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;
public class ToolRenderer : ItemRenderer<ModelData, StardewValley.Tool>
{
    public ToolRenderer(StardewValley.Tool obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new ToolRenderData(ctx, this);
    }
}

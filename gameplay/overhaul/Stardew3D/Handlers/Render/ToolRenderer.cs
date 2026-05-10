using Stardew3D.DataModels;
using StardewValley;

namespace Stardew3D.Handlers.Render;
public class ToolRenderer : ItemRenderer<ModelData, Tool>
{
    public ToolRenderer(Tool item)
        : base(item)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new ToolRenderData(ctx, this);
    }
}

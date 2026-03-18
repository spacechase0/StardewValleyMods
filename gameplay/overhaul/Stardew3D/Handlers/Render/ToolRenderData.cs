using Stardew3D.DataModels;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class ToolRenderData : RenderDataWithPlaceholder<ModelData, StardewValley.Tool>
{
    private int nonInstanced = -1;

    public ToolRenderData(RenderContext ctx, ToolRenderer parent)
        : base( ctx, parent)
    {
    }
}

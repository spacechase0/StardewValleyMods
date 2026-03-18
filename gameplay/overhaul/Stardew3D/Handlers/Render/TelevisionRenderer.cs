using Stardew3D.DataModels;
using StardewValley.Objects;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;
public class TelevisionRenderer : ItemRenderer<ModelData, TV>
{
    public TelevisionRenderer(TV obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new TelevisionRenderData(ctx, this);
    }
}

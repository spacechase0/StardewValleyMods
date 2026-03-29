using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.DataModels;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Stardew3D.Handlers.Render;
public class ResourceClumpRenderer : RendererFor<ModelData, ResourceClump>
{
    public ResourceClumpRenderer(ResourceClump obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new ResourceClumpRenderData(ctx, this);
    }
}

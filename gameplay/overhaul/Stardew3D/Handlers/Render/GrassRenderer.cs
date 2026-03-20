using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.DataModels;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Stardew3D.Handlers.Render;

public class GrassRenderer : RendererFor<ModelData, Grass>
{
    public GrassRenderer(Grass obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new GrassRenderData(ctx, this);
    }
}

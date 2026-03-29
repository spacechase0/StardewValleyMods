using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using StardewValley.GameData.FloorsAndPaths;
using StardewValley.TerrainFeatures;

namespace Stardew3D.Handlers.Render;
public class FlooringRenderer : RendererFor<ModelData, Flooring>
{
    public FlooringRenderer(Flooring obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new FlooringRenderData(ctx, this);
    }
}

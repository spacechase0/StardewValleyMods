using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using StardewValley;
using StardewValley.TerrainFeatures;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;
public class HoeDirtRenderer : RendererFor<ModelData, HoeDirt>
{
    public HoeDirtRenderer(HoeDirt obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new HoeDirtRenderData(ctx, this);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.DataModels;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;

namespace Stardew3D.Handlers.Render;
public class BuildingRenderer : RendererFor<ModelData, Building>
{
    public BuildingRenderer(Building obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new BuildingRenderData(ctx, this);
    }
}

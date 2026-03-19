using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.DataModels;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Stardew3D.Handlers.Render;
public class TreeRenderer : RendererFor<ModelData, Tree>
{
    public TreeRenderer(Tree obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new TreeRenderData(ctx, this);
    }
}

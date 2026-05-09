using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using StardewValley;

namespace Stardew3D.Handlers.Render;
public class DebrisRenderer : RendererFor<ModelData, Debris>
{
    internal GameLocation ParentLocation;

    public DebrisRenderer(Debris obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new DebrisRenderData(ctx, this);
    }
}

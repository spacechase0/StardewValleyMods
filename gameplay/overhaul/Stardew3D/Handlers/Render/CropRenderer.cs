using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using StardewValley;

namespace Stardew3D.Handlers.Render;
public class CropRenderer : RendererFor<ModelData, Crop>
{
    public CropRenderer(Crop obj)
        : base(obj)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new CropRenderData(ctx, this);
    }
}

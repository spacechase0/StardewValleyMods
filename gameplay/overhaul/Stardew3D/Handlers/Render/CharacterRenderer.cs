using Stardew3D.DataModels;
using StardewValley;

namespace Stardew3D.Handlers.Render;
public class CharacterRenderer : RendererFor<ModelData, Character>
{
    public CharacterRenderer(Character item)
        : base(item)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new CharacterRenderData(ctx, this);
    }
}

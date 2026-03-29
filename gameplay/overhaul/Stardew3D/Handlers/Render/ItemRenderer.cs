using Stardew3D.DataModels;
using StardewValley;

namespace Stardew3D.Handlers.Render;
public class ItemRenderer<TData, TItem> : RendererFor<TData, TItem>
    where TData : ModelData
    where TItem : Item
{
    public ItemRenderer(TItem item)
        : base(item)
    {
    }

    protected override RenderDataBase CreateInitialRenderData(IRenderHandler.RenderContext ctx)
    {
        return new ItemRenderData<TData, TItem>(ctx, this);
    }
}

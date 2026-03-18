using Stardew3D.DataModels;
using StardewValley;

namespace Stardew3D.Handlers.Render;
public class ItemRenderer<TData, TItem> : RendererWithPlaceholder<TData, TItem>
    where TData : ModelData
    where TItem : Item
{
    private PlaceholderData[] placeholders;
    public override PlaceholderData[] Placeholders => placeholders;

    public ItemRenderer(TItem item)
        : base(item)
    {
        placeholders =
        [
            new()
            {
                Texture = ItemRegistry.GetDataOrErrorItem(Object.QualifiedItemId).GetTexture(),
                TextureRegion = ItemRegistry.GetDataOrErrorItem(Object.QualifiedItemId).GetSourceRect()
            }
        ];
    }
}

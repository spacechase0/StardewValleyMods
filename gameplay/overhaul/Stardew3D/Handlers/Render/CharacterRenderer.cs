using Stardew3D.DataModels;
using StardewValley;

namespace Stardew3D.Handlers.Render;
public class CharacterRenderer<TData, TItem> : RendererWithPlaceholder<TData, TItem>
    where TData : ModelData
    where TItem : Character
{
    private PlaceholderData[] placeholders;
    public override PlaceholderData[] Placeholders => placeholders;

    public CharacterRenderer(TItem item)
        : base(item)
    {
        if (Object is Farmer)
        {
            placeholders = [];
            return;
        }

        placeholders =
        [
            new()
            {
                Texture = Object.Sprite.Texture,
                TextureRegion = Object.Sprite.SourceRect,
            }
        ];
    }
}

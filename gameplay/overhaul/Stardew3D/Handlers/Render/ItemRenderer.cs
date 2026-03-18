using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.DataModels;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

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

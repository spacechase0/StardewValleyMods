using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.Data;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace Stardew3D.Handlers.Render;
public class ItemRenderer<TData, TItem> : RendererWithPlaceholder<TData, TItem>
    where TData : ModelData
    where TItem : Item
{
    public override PlaceholderData[] Placeholders =>
    [
        new()
        {
            Texture = ItemRegistry.GetDataOrErrorItem(Object.QualifiedItemId).GetTexture(),
            TextureRegion = ItemRegistry.GetDataOrErrorItem(Object.QualifiedItemId).GetSourceRect()
        }
    ];

    public ItemRenderer(TItem item)
        : base(item.QualifiedItemId, item)
    {
    }
}

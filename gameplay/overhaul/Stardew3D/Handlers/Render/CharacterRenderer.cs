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

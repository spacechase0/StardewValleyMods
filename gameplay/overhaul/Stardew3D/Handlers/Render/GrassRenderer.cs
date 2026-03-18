using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.DataModels;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;
public class GrassRenderer : RendererWithPlaceholder<ModelData, Grass>
{
    private PlaceholderData[] placeholders;
    public override PlaceholderData[] Placeholders => placeholders;

    public GrassRenderer(Grass obj)
        : base(obj)
    {
        obj.loadSprite();

        List<PlaceholderData> placeholders = new();
        PlaceholderData template = new();
        template.Texture = Object.texture.Value;

        for (int i_ = 0; i_ < 4; ++i_)
        {
            int i = i_;

            Vector2 pos = Vector2.Zero;
            if (i != 4)
                pos = new Vector2((float)(i % 2 * 64 / 2 + Object.offset3[i] * 4 - 4) + 30f, i / 2 * 64 / 2 + Object.offset4[i] * 4 + 40);
            else
                pos = new Vector2((float)(16 + Object.offset1[i] * 4 - 4) + 30f, 16 + Object.offset2[i] * 4 + 40);
            pos /= Game1.tileSize;

            PlaceholderData weed = template;
            weed.TextureRegion = new(Object.whichWeed[i] * 15, Object.grassSourceOffset.Value, 15, 20);
            //weed.DefaultDisplaySizeScale = weed.TextureRegion.Width / (float)16;
            weed.OffsetOverride = weed.DefaultOffset + new Vector3(pos.X + -7.5f / Game1.tileSize, 0, pos.Y + -17.5f / Game1.tileSize);
            weed.DisplayCondition = () => i < Object.numberOfWeeds.Value && !Object.flip[i];
            placeholders.Add(weed);

            weed.Effects = SpriteEffects.FlipHorizontally;
            weed.DisplayCondition = () => i < Object.numberOfWeeds.Value && Object.flip[i];
            placeholders.Add(weed);
        }
        this.placeholders = placeholders.ToArray();
    }
}

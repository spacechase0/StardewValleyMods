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
public class TreeRenderer : RendererWithPlaceholder<ModelData, Tree>
{
    private PlaceholderData[] placeholders;
    public override PlaceholderData[] Placeholders => placeholders;

    public TreeRenderer(Tree obj)
        : base(obj)
    {
        obj.loadSprite();

        List<PlaceholderData> placeholders = new();
        PlaceholderData template = new();
        template.Texture = Object.texture.Value;

        bool[] flippedVals = { false, true };
        foreach (var flippedVal_ in flippedVals)
        {
            var flippedVal = flippedVal_;
            template.Effects = flippedVal ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (template.Texture == null || !Tree.TryGetData(Object.treeType.Value, out var data))
            {
                PlaceholderData error = template;
                error.Texture = ItemRegistry.RequireTypeDefinition("(O)").GetErrorTexture();
                error.TextureRegion = ItemRegistry.RequireTypeDefinition("(O)").GetErrorSourceRect();
                error.DisplayCondition = () => Object.flipped.Value == flippedVal;
                placeholders.Add(error);
            }
            else
            {
                for (int i_ = 0; i_ < 4; ++i_)
                {
                    int i = i_;

                    PlaceholderData growing = template;
                    growing.TextureRegion = i switch
                    {
                        0 => new Rectangle(32, 128, 16, 16),
                        1 => new Rectangle(0, 128, 16, 16),
                        2 => new Rectangle(16, 128, 16, 16),
                        _ => new Rectangle(0, 96, 16, 32),
                    };
                    //growing.Color = Object.fertilized.Value ? Color.HotPink : Color.White;
                    //growing.DefaultDisplaySizeScale = growing.TextureRegion.Width / (float)16;
                    growing.DisplayCondition = () => Object.flipped.Value == flippedVal && Object.growthStage.Value == i;
                    placeholders.Add(growing);
                }

                PlaceholderData stump = template;
                stump.TextureRegion = Tree.stumpSourceRect;
                if (Object.hasMoss.Value)
                    stump.TextureRegion = new(new(stump.TextureRegion.X + 96, stump.TextureRegion.Y), stump.TextureRegion.Size);
                //stump.DefaultDisplaySizeScale = stump.TextureRegion.Width / (float)16;
                //stump.Offset = new Vector3(0, -32 + 96, 0) / Game1.tileSize;
                stump.DisplayCondition = () => Object.flipped.Value == flippedVal && Object.growthStage.Value >= 5 && (Object.health.Value > 1 || (!Object.falling.Value && Object.health.Value > -99));
                placeholders.Add(stump);

                PlaceholderData top = template;
                top.TextureRegion = Tree.treeTopSourceRect;
                int topRectX = top.TextureRegion.X;
                if ((data.UseAlternateSpriteWhenSeedReady && Object.hasSeed.Value) || (data.UseAlternateSpriteWhenNotShaken && !Object.wasShakenToday.Value))
                    topRectX = 48;
                else
                    topRectX = 0;
                if (Object.hasMoss.Value)
                    topRectX = 96;
                top.TextureRegion = new(new(topRectX, top.TextureRegion.Y), top.TextureRegion.Size);
                //top.DefaultDisplaySizeScale = top.TextureRegion.Width / (float)16;
                top.OffsetOverride = top.DefaultOffset + new Vector3(0, stump.DisplaySize.Y / 2*0, 0);
                top.DisplayCondition = () => Object.flipped.Value == flippedVal && Object.growthStage.Value >= 5 && (!Object.stump.Value || Object.falling.Value);
                placeholders.Add(top);
            }
        }
       this.placeholders = placeholders.ToArray();
    }
}

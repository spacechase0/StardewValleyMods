using System.Linq;
using System.Xml.Serialization;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PicturePortraits
{
    [XmlType("Mods_spacechase0.CameraTool")]
    public class CameraTool : Tool
    {
        private static readonly Texture2D Texture = Game1.content.Load<Texture2D>("Maps\\townInterior");
        private static readonly Rectangle CameraRect = new Rectangle(191, 688, 16, 16);

        public CameraTool()
        {
            this.Name = "Camera";
            this.InstantUse = true;
            this.numAttachmentSlots.Value = 1;
            this.attachments.SetCount(1);
        }

        protected override string loadDisplayName()
        {
            return Mod.instance.Helper.Translation.Get("camera.name");
        }

        protected override string loadDescription()
        {
            return Mod.instance.Helper.Translation.Get("camera.description");
        }

        public override int attachmentSlots()
        {
            return 1;
        }

        public override bool canThisBeAttached(StardewValley.Object o)
        {
            return (o is IDGAItem item && item.FullId == $"{Mod.instance.ModManifest.UniqueID}/Film");
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
        {
            who.CanMove = true;
            who.UsingTool = false;
            who.canReleaseTool = true;

            if (this.attachments[0] == null)
            {
                Game1.playSound("shiny4");
                return;
            }

            var checkRect = new Rectangle(who.getStandingX() - Game1.tileSize, who.getStandingY() - Game1.tileSize, Game1.tileSize * 2, Game1.tileSize * 2);

            foreach (var character in location.characters)
            {
                var charRect = character.GetBoundingBox();
                SpaceShared.Log.Debug("Checking " + character.Name + " " + charRect + " " + checkRect);
                if (charRect.Intersects(checkRect))
                {
                    SpaceShared.Log.Debug("Found " + character.Name);
                    foreach (var pack in DynamicGameAssets.Mod.GetPacks())
                    {
                        foreach (var furniture in pack.GetItems().OfType<FurniturePackData>())
                        {
                            if (furniture.ExtensionData.TryGetValue($"{Mod.instance.ModManifest.UniqueID}.PortraitFor", out object val))
                            {
                                if (character.Name == val.ToString())
                                {
                                    Game1.flashAlpha = 1f;
                                    Game1.playSound("cameraNoise");

                                    who.addItemByMenuIfNecessary(furniture.ToItem());
                                    if (this.attachments[0].Stack > 1)
                                        this.attachments[0].Stack--;
                                    else
                                        this.attachments[0] = null;
                                    return;
                                }
                            }
                        }
                    }
                    Game1.drawObjectDialogue(Mod.instance.Helper.Translation.Get("error.no-matching-portrait"));
                    return;
                }
            }
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(CameraTool.Texture, location, CameraTool.CameraRect, Color.White * transparency, 0, Vector2.Zero, scaleSize * 4, SpriteEffects.None, layerDepth);
        }

        public override void drawAttachments(SpriteBatch b, int x, int y)
        {
            if (this.attachments[0] == null)
            {
                b.Draw(Game1.menuTexture, new Vector2(x, y), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 43), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
                return;
            }
            b.Draw(Game1.menuTexture, new Vector2(x, y), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
            this.attachments[0].drawInMenu(b, new Vector2(x, y), 1f);
        }

        public override Item getOne()
        {
            return new CameraTool();
        }
    }
}

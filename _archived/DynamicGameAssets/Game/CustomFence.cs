using System;
using System.Text;
using System.Xml.Serialization;
using DynamicGameAssets.Framework;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.Tools;

namespace DynamicGameAssets.Game
{
    [XmlType("Mods_DGAFence")]
    public partial class CustomFence : Fence
    {
        partial void DoInit()
        {
            this.fenceTexture = new Lazy<Texture2D>(() => this.Data.pack.GetTexture(this.Data.PlacedTilesheet, 48, 325).Texture);
        }

        partial void DoInit(FencePackData data)
        {
            this.Name = this.Id;
            this.whichType.Value = this.FullId.GetDeterministicHashCode();
            this.ResetHealth(0);

            this.CanBeSetDown = true;
            this.CanBeGrabbed = true;
            this.Type = "Crafting";
        }

        public CustomFence(FencePackData data, Vector2 tileLocation)
            : this(data)
        {
            this.tileLocation.Value = tileLocation;
            this.boundingBox.Value = new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields(this.NetSourcePack, this.NetId);
        }

        protected override string loadDisplayName()
        {
            return this.Data.Name;
        }

        public override string getDescription()
        {
            return Game1.parseText(this.Data.Description, Game1.smallFont, this.getDescriptionWidth());
        }

        public override void ResetHealth(float amount_adjustment)
        {
            this.maxHealth.Value = this.health.Value = this.Data.MaxHealth;
        }

        public override void dropItem(GameLocation location, Vector2 origin, Vector2 destination)
        {
            location.debris.Add(new Debris(this.getOne(), origin, destination));
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            Vector2 placementTile = new Vector2(x / 64, y / 64);
            if (location.objects.ContainsKey(placementTile))
                return false;
            location.objects.Add(placementTile, new CustomFence(this.Data, placementTile));
            location.playSound(this.Data.PlacementSound);
            return true;
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool performToolAction(Tool t, GameLocation location)
        {
            var Game1_multiplayer = Mod.instance.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            if (this.heldObject.Value != null && t is not (null or MeleeWeapon) && t.isHeavyHitter())
            {
                StardewValley.Object value = this.heldObject.Value;
                this.heldObject.Value.performRemoveAction(this.tileLocation, location);
                this.heldObject.Value = null;
                Game1.createItemDebris(value.getOne(), this.TileLocation * 64f, -1);
                location.playSound("axchop");
                return false;
            }
            if ((bool)this.isGate && t is Axe or Pickaxe)
            {
                location.playSound("axchop");
                Game1.createObjectDebris(325, (int)this.tileLocation.X, (int)this.tileLocation.Y, Game1.player.UniqueMultiplayerID, Game1.player.currentLocation);
                location.objects.Remove(this.tileLocation);
                Game1.createRadialDebris(location, 12, (int)this.tileLocation.X, (int)this.tileLocation.Y, 6, resource: false);
                Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(this.tileLocation.X * 64f, this.tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
            }
            if (t == null || (t is Pickaxe && this.Data.BreakTool == FencePackData.ToolType.Pickaxe) || (t is Axe && this.Data.BreakTool == FencePackData.ToolType.Axe))
            {
                location.playSound(this.Data.BreakTool == FencePackData.ToolType.Axe ? "axchop" : "hammer");
                location.objects.Remove(this.tileLocation);
                for (int i = 0; i < 4; i++)
                {
                    location.temporarySprites.Add(new CosmeticDebris(this.fenceTexture.Value, new Vector2(this.tileLocation.X * 64f + 32f, this.tileLocation.Y * 64f + 32f), (float)Game1.random.Next(-5, 5) / 100f, (float)Game1.random.Next(-64, 64) / 30f, (float)Game1.random.Next(-800, -100) / 100f, (int)((this.tileLocation.Y + 1f) * 64f), new Rectangle(32 + Game1.random.Next(2) * 16 / 2, 96 + Game1.random.Next(2) * 16 / 2, 8, 8), Color.White, (Game1.soundBank != null) ? Game1.soundBank.GetCue("shiny4") : null, null, 0, 200));
                }
                Game1.createRadialDebris(location, 12, (int)this.tileLocation.X, (int)this.tileLocation.Y, 6, resource: false);
                Game1_multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(this.tileLocation.X * 64f, this.tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));

                location.debris.Add(new Debris(this.getOne(), this.tileLocation.Value * 64f + new Vector2(32f, 32f)));
            }
            return false;
        }

        public bool CanRepairWithThisItem(Item item)
        {
            if (this.health.Value > 1f)
                return false;

            if (this.Data.RepairMaterial.Matches(item))
                return true;

            return false;
        }

        public override string GetRepairSound()
        {
            return this.Data.RepairSound;
        }

        public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
        {
            var ret = base.getExtraSpaceNeededForTooltipSpecialIcons(font, minWidth, horizontalBuffer, startingHeight, descriptionText, boldTitleText, moneyAmountToDisplayAtBottom);
            ret.Y = startingHeight;
            ret.Y += 48;
            return ret;
        }

        public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
        {
            base.drawTooltip(spriteBatch, ref x, ref y, font, alpha, overrideText);
            string str = I18n.ItemTooltip_AddedByMod(this.Data.pack.smapiPack.Manifest.Name);
            Utility.drawTextWithShadow(spriteBatch, Game1.parseText(str, Game1.smallFont, this.getDescriptionWidth()), font, new Vector2(x + 16, y + 16 + 4), new Color(100, 100, 100));
            y += (int)font.MeasureString(Game1.parseText(str, Game1.smallFont, this.getDescriptionWidth())).Y + 10;
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            spriteBatch.Draw(this.Data.GetTexture().Texture, objectPosition - new Vector2(0f, 0), this.Data.GetTexture().Rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(f.getStandingY() + 1) / 10000f);
        }


        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;

            if ((int)this.parentSheetIndex != 590 && drawShadow)
            {
                spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), Game1.shadowTexture.Bounds, color * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
            }
            var tex = this.Data.pack.GetTexture(this.Data.ObjectTexture, 16, 16);
            spriteBatch.Draw(tex.Texture, location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)), tex.Rect, color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
            if (shouldDrawStackNumber)
            {
                Utility.drawTinyDigits(this.stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(this.stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 1f), 3f * scaleSize, 1f, color);
            }
        }

        public override Item getOne()
        {
            var ret = new CustomFence(this.Data);
            // TODO: All the other fields objects does??
            ret.Stack = 1;
            ret._GetOneFrom(this);
            return ret;
        }

        public override bool canStackWith(ISalable other)
        {
            if (other is not CustomFence fence)
                return false;

            return fence.FullId == this.FullId && base.canStackWith(fence);
        }
    }
}

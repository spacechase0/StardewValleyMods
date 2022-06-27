using System.Collections.Generic;
using System.Xml.Serialization;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets.Game
{
    // Literally a copy+paste of CustomBasicFurniture subclassing from a different type (with small TV-specific changes)
    [XmlType("Mods_DGATVFurniture")]
    public partial class CustomTVFurniture : TV, ISittable
    {
        private FurniturePackData.FurnitureConfiguration GetCurrentConfiguration()
        {
            return this.Data.Configurations.Count > this.currentRotation.Value ? this.Data.Configurations[this.currentRotation.Value] : new FurniturePackData.FurnitureConfiguration();
        }

        partial void DoInit()
        {
            Mod.instance.Helper.Reflection.GetField<int>(this, "_placementRestriction").SetValue(2);
        }

        partial void DoInit(FurniturePackData data)
        {
            this.name = this.FullId;
            this.furniture_type.Value = data.GetVanillaFurnitureType();
            this.defaultSourceRect.Value = this.sourceRect.Value = data.GetTexture().Rect ?? new Rectangle(0, 0, data.GetTexture().Texture.Width, data.GetTexture().Texture.Height);
            this.boundingBox.Value = new Rectangle(0, (int)(data.Configurations[0].DisplaySize.Y - data.Configurations[0].CollisionHeight) * Game1.tileSize, (int)data.Configurations[0].DisplaySize.X * Game1.tileSize, (int)data.Configurations[0].CollisionHeight * Game1.tileSize);
            this.rotations.Value = data.Configurations.Count;
            this.UpdateRotation();
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields(this.NetSourcePack, this.NetId);
        }

        public void UpdateRotation()
        {
            this.flipped.Value = false;

            var newConf = this.GetCurrentConfiguration();
            var newTex = this.Data.pack.GetTexture(newConf.Texture, (int)newConf.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)newConf.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom);

            this.boundingBox.Width = (int)newConf.DisplaySize.X * Game1.tileSize;
            this.boundingBox.Height = newConf.CollisionHeight * Game1.tileSize;
            this.sourceRect.Value = newTex.Rect ?? new Rectangle(0, 0, newTex.Texture.Width, newTex.Texture.Height);
            this.flipped.Value = newConf.Flipped;

            this.updateDrawPosition();
        }

        protected override string loadDisplayName()
        {
            return this.Data.Name;
        }

        public override string getDescription()
        {
            return this.Data.Description;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(this.Data.GetTexture().Texture, location + new Vector2(32f, 32f), this.Data.GetTexture().Rect, color * transparency, 0f, new Vector2(this.defaultSourceRect.Width / 2, this.defaultSourceRect.Height / 2), 1f * this.getScaleSize() * scaleSize, SpriteEffects.None, layerDepth);
            if (((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue)
            {
                Utility.drawTinyDigits(this.stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(this.stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
            }
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (this.isTemporarilyInvisible)
            {
                return;
            }

            var currConfig = this.GetCurrentConfiguration();
            TexturedRect currTex = null;
            if (Game1.isDarkOut() && currConfig.NightTexture != null)
            {
                currTex = this.Data.pack.GetTexture(currConfig.NightTexture, (int)currConfig.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)currConfig.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom);
            }
            else
            {
                currTex = this.Data.pack.GetTexture(currConfig.Texture, (int)currConfig.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)currConfig.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom);
            }
            var frontTex = currConfig.FrontTexture != null ? this.Data.pack.GetTexture(currConfig.FrontTexture, (int)currConfig.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)currConfig.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom) : null;

            if (Furniture.isDrawingLocationFurniture)
            {
                spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, this.drawPosition + ((this.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), currTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.boundingBox.Value.Top + 16) / 10000f);
                if (frontTex != null && this.sourceRect.Right <= Furniture.furnitureFrontTexture.Width && this.sourceRect.Bottom <= Furniture.furnitureFrontTexture.Height)
                {
                    spriteBatch.Draw(frontTex.Texture, Game1.GlobalToLocal(Game1.viewport, this.drawPosition + ((this.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), frontTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.boundingBox.Value.Bottom + 16) / 10000f);
                }
            }
            else
            {
                spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 - (this.sourceRect.Height * 4 - this.boundingBox.Height) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), currTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((int)this.furniture_type == 12) ? (2E-09f + this.tileLocation.Y / 100000f) : ((float)(this.boundingBox.Value.Bottom - (((int)this.furniture_type == 6 || (int)this.furniture_type == 17 || (int)this.furniture_type == 13) ? 48 : 8)) / 10000f));
            }
            if (this.heldObject.Value != null)
            {
                if (this.heldObject.Value is Furniture)
                {
                    (this.heldObject.Value as Furniture).drawAtNonTileSpot(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.boundingBox.Center.X - 32, this.boundingBox.Center.Y - (this.heldObject.Value as Furniture).sourceRect.Height * 4 - (this.drawHeldObjectLow ? (-16) : 16))), (float)(this.boundingBox.Bottom - 7) / 10000f, alpha);
                }
                else
                {
                    spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.boundingBox.Center.X - 32, this.boundingBox.Center.Y - (this.drawHeldObjectLow.Value ? 32 : 85))) + new Vector2(32f, 53f), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)this.boundingBox.Bottom / 10000f);
                    if (this.heldObject.Value is CustomObject customObject)
                    {
                        Vector2 position = new Vector2(this.boundingBox.Center.X - 32, this.boundingBox.Center.Y - (this.drawHeldObjectLow.Value ? 32 : 85));
                        customObject.draw(spriteBatch, (int)position.X, (int)position.Y, (float)(this.boundingBox.Bottom + 1) / 10000f + 0.5f, alpha);
                    }
                    else
                    {
                        spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.boundingBox.Center.X - 32, this.boundingBox.Center.Y - (this.drawHeldObjectLow.Value ? 32 : 85))), GameLocation.getSourceRectForObject(this.heldObject.Value.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(this.boundingBox.Bottom + 1) / 10000f);

                    }
                }
            }
            if ((bool)this.isOn && (int)this.furniture_type == 14)
            {
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.boundingBox.Center.X - 12, this.boundingBox.Center.Y - 64)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3047) + (double)(y * 88)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(this.getBoundingBox(new Vector2(x, y)).Bottom - 2) / 10000f);
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.boundingBox.Center.X - 32 - 4, this.boundingBox.Center.Y - 64)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 2047) + (double)(y * 98)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(this.getBoundingBox(new Vector2(x, y)).Bottom - 1) / 10000f);
            }
            else if ((bool)this.isOn && (int)this.furniture_type == 16)
            {
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.boundingBox.Center.X - 20, (float)this.boundingBox.Center.Y - 105.6f)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3047) + (double)(y * 88)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(this.getBoundingBox(new Vector2(x, y)).Bottom - 2) / 10000f);
            }
            if (Game1.debugMode)
            {
                spriteBatch.DrawString(Game1.smallFont, this.parentSheetIndex?.ToString() ?? "", Game1.GlobalToLocal(Game1.viewport, this.drawPosition), Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }

            var screen = Mod.instance.Helper.Reflection.GetField<TemporaryAnimatedSprite>(this, "screen").GetValue();
            var screenOverlay = Mod.instance.Helper.Reflection.GetField<TemporaryAnimatedSprite>(this, "screenOverlay").GetValue();

            if (screen != null)
            {
                screen.update(Game1.currentGameTime);
                screen.draw(spriteBatch);
                if (screenOverlay != null)
                {
                    screenOverlay.update(Game1.currentGameTime);
                    screenOverlay.draw(spriteBatch);
                }
            }
        }

        public override void drawAtNonTileSpot(SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
        {
            var currConfig = this.GetCurrentConfiguration();
            var currTex = this.Data.pack.GetTexture(currConfig.Texture, (int)currConfig.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)currConfig.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom);

            spriteBatch.Draw(currTex.Texture, location, currTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }
        public override Vector2 getScreenPosition()
        {
            var currConfig = this.GetCurrentConfiguration();
            return new Vector2(this.boundingBox.X + this.Data.ScreenPosition.X * Game1.pixelZoom, this.boundingBox.Y + (currConfig.CollisionHeight - currConfig.DisplaySize.Y) * Game1.tileSize + this.Data.ScreenPosition.Y * Game1.pixelZoom);
        }
        public override float getScreenSizeModifier()
        {
            return this.Data.ScreenSize;
        }

        public override bool DoesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
        {
            var currConfig = this.GetCurrentConfiguration();
            Vector2 key = new Vector2((int)(tile_x - this.tileLocation.X), (int)(tile_y - this.tileLocation.Y));
            if (currConfig.TileProperties.ContainsKey(key) && currConfig.TileProperties[key].ContainsKey(layer_name))
            {
                if (currConfig.TileProperties[key][layer_name].ContainsKey(property_name))
                {
                    property_value = currConfig.TileProperties[key][layer_name][property_name];
                    return true;
                }
            }
            return false;
        }

        public override int GetSeatCapacity()
        {
            return this.GetCurrentConfiguration().Seats.Count;
        }

        public override List<Vector2> GetSeatPositions(bool ignore_offsets = false)
        {
            var ret = new List<Vector2>();

            foreach (var seat in this.GetCurrentConfiguration().Seats)
            {
                ret.Add(this.TileLocation + seat);
            }

            return ret;
        }

        public int GetSittingDirection()
        {
            return this.GetCurrentConfiguration().SittingDirection == FurniturePackData.FurnitureConfiguration.SeatDirection.Any ?
                   Game1.player.FacingDirection : (int)this.GetCurrentConfiguration().SittingDirection;
        }

        public override Item getOne()
        {
            var ret = new CustomTVFurniture(this.Data);
            ret._GetOneFrom(this);
            return ret;
        }

        private float getScaleSize()
        {
            int tilesWide = this.defaultSourceRect.Width / 16;
            int tilesHigh = this.defaultSourceRect.Height / 16;
            if (tilesWide >= 7)
            {
                return 0.5f;
            }
            if (tilesWide >= 6)
            {
                return 0.66f;
            }
            if (tilesWide >= 5)
            {
                return 0.75f;
            }
            if (tilesHigh >= 5)
            {
                return 0.8f;
            }
            if (tilesHigh >= 3)
            {
                return 1f;
            }
            if (tilesWide <= 2)
            {
                return 2f;
            }
            if (tilesWide <= 4)
            {
                return 1f;
            }
            return 0.1f;
        }
    }
}

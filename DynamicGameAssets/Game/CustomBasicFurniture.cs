using System.Collections.Generic;
using System.Xml.Serialization;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets.Game
{
    [XmlType("Mods_DGABasicFurniture")]
    public partial class CustomBasicFurniture : Furniture, ISittable
    {
        public HashSet<Vector2> lightGlowPositionList = new HashSet<Vector2>();

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

        public override string getCategoryName()
        {
            return this.Data.CategoryTextOverride ?? base.getCategoryName();
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
                if (this.HasSittingFarmers())
                {
                    spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, this.drawPosition + ((this.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), currTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.boundingBox.Value.Top + 16) / 10000f);
                    if (frontTex != null && this.sourceRect.Right <= Furniture.furnitureFrontTexture.Width && this.sourceRect.Bottom <= Furniture.furnitureFrontTexture.Height)
                    {
                        spriteBatch.Draw(frontTex.Texture, Game1.GlobalToLocal(Game1.viewport, this.drawPosition + ((this.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), frontTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.boundingBox.Value.Bottom - 8) / 10000f);
                    }
                }
                else
                {
                    spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, this.drawPosition + ((this.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), currTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((int)this.furniture_type == 12) ? (2E-09f + this.tileLocation.Y / 100000f) : ((float)(this.boundingBox.Value.Bottom - (((int)this.furniture_type == 6 || (int)this.furniture_type == 17 || (int)this.furniture_type == 13) ? 48 : 8)) / 10000f));
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
        }

        public override void drawAtNonTileSpot(SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
        {
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

            spriteBatch.Draw(currTex.Texture, location, currTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }

        /// <inheritdoc />
        public override void AddLightGlow(GameLocation location)
        {
            // equivalent to the base logic, but correct lighting position for custom furniture
            if (this.lightGlowPositionList.Count == 0)
            {
                Vector2 furniturePixel = this.TileLocation * Game1.tileSize;
                Vector2 glowPixel = furniturePixel + new Vector2((float)Game1.tileSize / 2, Game1.tileSize);

                for (int i = 0; i < this.getTilesWide(); i++)
                {
                    if (!location.lightGlows.Contains(glowPixel))
                    {
                        this.lightGlowPositionList.Add(glowPixel);
                        location.lightGlows.Add(glowPixel);

                        glowPixel.X += 64f;
                    }
                }
            }
        }

        public override void RemoveLightGlow(GameLocation location)
        {
            foreach (Vector2 pos in this.lightGlowPositionList)
            {
                location.lightGlows.Remove(pos);
            }
            this.lightGlowPositionList.Clear();
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
            if (this.GetCurrentConfiguration().Seats.Count == 0 && (this.furniture_type == 0 || this.furniture_type == 1 || this.furniture_type == 3))
            {
                return base.GetSeatCapacity();
            }
            if (this.GetCurrentConfiguration().Seats.Count == 0 && this.furniture_type == 2)
            {
                return this.defaultSourceRect.Width / 16 - 1;
            }
            return this.GetCurrentConfiguration().Seats.Count;
        }

        public override List<Vector2> GetSeatPositions(bool ignore_offsets = false)
        {
            if (this.GetCurrentConfiguration().Seats.Count == 0 && (this.furniture_type == 0 || this.furniture_type == 1 || this.furniture_type == 3))
            {
                return base.GetSeatPositions();
            }

            var ret = new List<Vector2>();

            if (this.GetCurrentConfiguration().Seats.Count == 0 && this.furniture_type == 2)
            {
                int width = this.defaultSourceRect.Width / 16 - 1;
                if ((int)this.currentRotation == 0 || (int)this.currentRotation == 2)
                {
                    ret.Add(base.TileLocation + new Vector2(0.5f, 0f));
                    for (int i = 1; i < width - 1; i++)
                    {
                        ret.Add(base.TileLocation + new Vector2((float)i + 0.5f, 0f));
                    }
                    ret.Add(base.TileLocation + new Vector2((float)(width - 1) + 0.5f, 0f));
                }
                else if ((int)this.currentRotation == 1)
                {
                    for (int k = 0; k < width; k++)
                    {
                        ret.Add(base.TileLocation + new Vector2(1f, k));
                    }
                }
                else
                {
                    for (int j = 0; j < width; j++)
                    {
                        ret.Add(base.TileLocation + new Vector2(0f, j));
                    }
                }
                return ret;
            }

            foreach (var seat in this.GetCurrentConfiguration().Seats)
            {
                ret.Add(this.TileLocation + seat);
            }

            return ret;
        }

        public int GetSittingDirection()
        {
            if (this.GetCurrentConfiguration().Seats.Count == 0 && this.furniture_type == 0 || this.furniture_type == 1 || this.furniture_type == 2 || this.furniture_type == 3)
            {
                return base.GetSittingDirection();
            }

            return this.GetCurrentConfiguration().SittingDirection == FurniturePackData.FurnitureConfiguration.SeatDirection.Any ?
                   Game1.player.FacingDirection : (int)this.GetCurrentConfiguration().SittingDirection;
        }

        public override Item getOne()
        {
            var ret = new CustomBasicFurniture(this.Data);
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

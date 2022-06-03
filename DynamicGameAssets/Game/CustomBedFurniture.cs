using System.Collections.Generic;
using System.Xml.Serialization;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets.Game
{
    // Literally a copy+paste of CustomBasicFurniture subclassing from a different type (with small bed-specific changes)
    [XmlType("Mods_DGABedFurniture")]
    public partial class CustomBedFurniture : BedFurniture, ISittable
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
            this.defaultBoundingBox.Value = this.boundingBox.Value = new Rectangle(0, (int)(data.Configurations[0].DisplaySize.Y - data.Configurations[0].CollisionHeight) * Game1.tileSize, (int)data.Configurations[0].DisplaySize.X * Game1.tileSize, (int)data.Configurations[0].CollisionHeight * Game1.tileSize);
            this.rotations.Value = data.Configurations.Count;
            this.UpdateRotation();
            this.bedType = data.BedType;
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
            var currConfig = this.GetCurrentConfiguration();
            var currTex = this.Data.pack.GetTexture(currConfig.Texture, (int)currConfig.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)currConfig.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom);
            var frontTex = currConfig.FrontTexture != null ? this.Data.pack.GetTexture(currConfig.FrontTexture, (int)currConfig.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)currConfig.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom) : null;

            if (!this.isTemporarilyInvisible)
            {
                if (Furniture.isDrawingLocationFurniture)
                {
                    Rectangle? drawn_rect = currTex.Rect;
                    spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, this.drawPosition + ((this.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), drawn_rect, Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.boundingBox.Value.Top + 1) / 10000f);
                    drawn_rect = frontTex.Rect;
                    spriteBatch.Draw(frontTex.Texture, Game1.GlobalToLocal(Game1.viewport, this.drawPosition + ((this.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), drawn_rect, Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.boundingBox.Value.Bottom - 1) / 10000f);
                }
                else
                {
                    spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 - (this.sourceRect.Height * 4 - this.boundingBox.Height) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), currTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((int)this.furniture_type == 12) ? (2E-09f + this.tileLocation.Y / 100000f) : ((float)(this.boundingBox.Value.Bottom - (((int)this.furniture_type == 6 || (int)this.furniture_type == 17 || (int)this.furniture_type == 13) ? 48 : 8)) / 10000f));
                }
            }
        }

        public override void drawAtNonTileSpot(SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
        {
            var currConfig = this.GetCurrentConfiguration();
            var currTex = this.Data.pack.GetTexture(currConfig.Texture, (int)currConfig.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)currConfig.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom);

            spriteBatch.Draw(currTex.Texture, location, currTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
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

            if (base.DoesTileHaveProperty(tile_x, tile_y, property_name, layer_name, ref property_value))
                return true;

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
            var ret = new CustomBedFurniture(this.Data);
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

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceShared;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets.Game
{
    // Literally a copy+paste of CustomBasicFurniture subclassing from a different type (with small TV-specific changes)
    [XmlType( "Mods_DGATVFurniture" )]
    [Mixin( typeof( CustomItemMixin<FurniturePackData> ) )]
    public partial class CustomTVFurniture : TV, ISittable
    {
        private FurniturePackData.FurnitureConfiguration GetCurrentConfiguration()
        {
            return Data.Configurations.Count > this.currentRotation.Value ? Data.Configurations[ this.currentRotation.Value ] : new FurniturePackData.FurnitureConfiguration();
        }

        partial void DoInit()
        {
            Mod.instance.Helper.Reflection.GetField<int>( this, "_placementRestriction" ).SetValue( 2 );
        }

        partial void DoInit( FurniturePackData data )
        {
            name = FullId;
            furniture_type.Value = data.GetVanillaFurnitureType();
            defaultSourceRect.Value = sourceRect.Value = data.GetTexture().Rect ?? new Rectangle( 0, 0, data.GetTexture().Texture.Width, data.GetTexture().Texture.Height );
            boundingBox.Value = new Rectangle( 0, (int)(data.Configurations[0].DisplaySize.Y - data.Configurations[0].CollisionHeight) * Game1.tileSize, ( int ) data.Configurations[0].DisplaySize.X * Game1.tileSize, ( int ) data.Configurations[0].CollisionHeight * Game1.tileSize );
            rotations.Value = data.Configurations.Count;
            UpdateRotation();
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields(_sourcePack, _id);
        }

        public void UpdateRotation()
        {
            flipped.Value = false;

            var newConf = GetCurrentConfiguration();
            var newTex = Data.parent.GetTexture( newConf.Texture, ( int ) newConf.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, ( int ) newConf.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom );

            boundingBox.Width = ( int ) newConf.DisplaySize.X * Game1.tileSize;
            boundingBox.Height = newConf.CollisionHeight * Game1.tileSize;
            sourceRect.Value = newTex.Rect ?? new Rectangle( 0, 0, newTex.Texture.Width, newTex.Texture.Height );
            flipped.Value = newConf.Flipped;

            updateDrawPosition();
        }

        protected override string loadDisplayName()
        {
            return Data.Name;
        }

        public override string getDescription()
        {
            return Data.Description;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(Data.GetTexture().Texture, location + new Vector2(32f, 32f), Data.GetTexture().Rect, color * transparency, 0f, new Vector2(this.defaultSourceRect.Width / 2, this.defaultSourceRect.Height / 2), 1f * this.getScaleSize() * scaleSize, SpriteEffects.None, layerDepth);
            if (((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue)
            {
                Utility.drawTinyDigits(base.stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(base.stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
            }
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (base.isTemporarilyInvisible)
            {
                return;
            }

            var currConfig = GetCurrentConfiguration();
            var currTex = Data.parent.GetTexture(currConfig.Texture, (int)currConfig.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)currConfig.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom);
            var frontTex = currConfig.FrontTexture != null ? Data.parent.GetTexture(currConfig.FrontTexture, (int)currConfig.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)currConfig.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom) : null;

            if (Furniture.isDrawingLocationFurniture)
            {
                if (this.HasSittingFarmers() && this.sourceRect.Right <= Furniture.furnitureFrontTexture.Width && this.sourceRect.Bottom <= Furniture.furnitureFrontTexture.Height)
                {
                    spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, this.drawPosition + ((base.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), currTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, base.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(base.boundingBox.Value.Top + 16) / 10000f);
                    spriteBatch.Draw(frontTex.Texture, Game1.GlobalToLocal(Game1.viewport, this.drawPosition + ((base.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), frontTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, base.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(base.boundingBox.Value.Bottom - 8) / 10000f);
                }
                else
                {
                    spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, this.drawPosition + ((base.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), currTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, base.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((int)this.furniture_type == 12) ? (2E-09f + base.tileLocation.Y / 100000f) : ((float)(base.boundingBox.Value.Bottom - (((int)this.furniture_type == 6 || (int)this.furniture_type == 17 || (int)this.furniture_type == 13) ? 48 : 8)) / 10000f));
                }
            }
            else
            {
                spriteBatch.Draw(currTex.Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 - (this.sourceRect.Height * 4 - base.boundingBox.Height) + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), currTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, base.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ((int)this.furniture_type == 12) ? (2E-09f + base.tileLocation.Y / 100000f) : ((float)(base.boundingBox.Value.Bottom - (((int)this.furniture_type == 6 || (int)this.furniture_type == 17 || (int)this.furniture_type == 13) ? 48 : 8)) / 10000f));
            }
            if (base.heldObject.Value != null)
            {
                if (base.heldObject.Value is Furniture)
                {
                    (base.heldObject.Value as Furniture).drawAtNonTileSpot(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(base.boundingBox.Center.X - 32, base.boundingBox.Center.Y - (base.heldObject.Value as Furniture).sourceRect.Height * 4 - (this.drawHeldObjectLow ? (-16) : 16))), (float)(base.boundingBox.Bottom - 7) / 10000f, alpha);
                }
                else
                {
                    spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(base.boundingBox.Center.X - 32, base.boundingBox.Center.Y - (this.drawHeldObjectLow ? 32 : 85))) + new Vector2(32f, 53f), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)base.boundingBox.Bottom / 10000f);
                    spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(base.boundingBox.Center.X - 32, base.boundingBox.Center.Y - (this.drawHeldObjectLow ? 32 : 85))), GameLocation.getSourceRectForObject(base.heldObject.Value.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(base.boundingBox.Bottom + 1) / 10000f);
                }
            }
            if ((bool)base.isOn && (int)this.furniture_type == 14)
            {
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(base.boundingBox.Center.X - 12, base.boundingBox.Center.Y - 64)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3047) + (double)(y * 88)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(this.getBoundingBox(new Vector2(x, y)).Bottom - 2) / 10000f);
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(base.boundingBox.Center.X - 32 - 4, base.boundingBox.Center.Y - 64)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 2047) + (double)(y * 98)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(this.getBoundingBox(new Vector2(x, y)).Bottom - 1) / 10000f);
            }
            else if ((bool)base.isOn && (int)this.furniture_type == 16)
            {
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(base.boundingBox.Center.X - 20, (float)base.boundingBox.Center.Y - 105.6f)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3047) + (double)(y * 88)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(this.getBoundingBox(new Vector2(x, y)).Bottom - 2) / 10000f);
            }
            if (Game1.debugMode)
            {
                spriteBatch.DrawString(Game1.smallFont, base.parentSheetIndex?.ToString() ?? "", Game1.GlobalToLocal(Game1.viewport, this.drawPosition), Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }

            var screen = Mod.instance.Helper.Reflection.GetField< TemporaryAnimatedSprite >( this, "screen" ).GetValue();
            var screenOverlay = Mod.instance.Helper.Reflection.GetField< TemporaryAnimatedSprite >( this, "screenOverlay" ).GetValue();

            if ( screen != null )
            {
                screen.update( Game1.currentGameTime );
                screen.draw( spriteBatch );
                if ( screenOverlay != null )
                {
                    screenOverlay.update( Game1.currentGameTime );
                    screenOverlay.draw( spriteBatch );
                }
            }
        }

        public void drawAtNonTileSpot(SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
        {
            var currConfig = GetCurrentConfiguration();
            var currTex = Data.parent.GetTexture(currConfig.Texture, (int)currConfig.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)currConfig.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom);

            spriteBatch.Draw(currTex.Texture, location, currTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, base.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }
        public override Vector2 getScreenPosition()
        {
            var currConfig = GetCurrentConfiguration();
            return new Vector2( base.boundingBox.X + Data.ScreenPosition.X * Game1.pixelZoom, base.boundingBox.Y + ( currConfig.CollisionHeight - currConfig.DisplaySize.Y ) * Game1.tileSize + Data.ScreenPosition.Y * Game1.pixelZoom );
        }
        public override float getScreenSizeModifier()
        {
            return Data.ScreenSize;
        }

        public override bool DoesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
        {
            var currConfig = GetCurrentConfiguration();
            Vector2 key = new Vector2((int)(tile_x - tileLocation.X), (int)(tile_y - tileLocation.Y));
            if ( currConfig.TileProperties.ContainsKey( key ) && currConfig.TileProperties[ key ].ContainsKey( layer_name ) )
            {
                if ( currConfig.TileProperties[ key ][ layer_name ].ContainsKey( property_name ) )
                {
                    property_value = currConfig.TileProperties[key][ layer_name ][ property_name ];
                    return true;
                }
            }
            return false;
        }

        public override int GetSeatCapacity()
        {
            return GetCurrentConfiguration().Seats.Count;
        }

        public override List<Vector2> GetSeatPositions( bool ignore_offsets = false )
        {
            var ret = new List<Vector2>();

            foreach ( var seat in GetCurrentConfiguration().Seats )
            {
                ret.Add( TileLocation + seat );
            }

            return ret;
        }

        public int GetSittingDirection()
        {
            return GetCurrentConfiguration().SittingDirection == FurniturePackData.FurnitureConfiguration.SeatDirection.Any ?
                   Game1.player.FacingDirection : ( int ) GetCurrentConfiguration().SittingDirection;
        }

        public override Item getOne()
        {
            var ret = new CustomTVFurniture(Data);
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

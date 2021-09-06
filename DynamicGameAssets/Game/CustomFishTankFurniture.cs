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
    // Literally a copy+paste of CustomBasicFurniture subclassing from a different type (with small fish tank-specific changes)
    [XmlType( "Mods_DGAFishTankFurniture" )]
    [Mixin( typeof( CustomItemMixin<FurniturePackData> ) )]
    public partial class CustomFishTankFurniture : FishTankFurniture, ISittable
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
            var newTex = Data.pack.GetTexture( newConf.Texture, ( int ) newConf.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, ( int ) newConf.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom );

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

        public int GetCapacityForCategory( FishTankCategories category )
        {
            switch ( category )
            {
                case FishTankCategories.Swim:
                    return Data.TankSwimmingCapacity;
                case FishTankCategories.Ground:
                    return Data.TankGroundCapacity;
                case FishTankCategories.Decoration:
                    return Data.TankDecorationCapacity;
                default:
                    return 0;
            }
        }
        public override Rectangle GetTankBounds()
        {
            var ret = base.GetTankBounds();
            ret.Y -= 64 * 3;
            return ret;
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
            var currTex = Data.pack.GetTexture(currConfig.Texture, (int)currConfig.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)currConfig.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom);
            var frontTex = currConfig.FrontTexture != null ? Data.pack.GetTexture(currConfig.FrontTexture, (int)currConfig.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)currConfig.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom) : null;

            Vector2 shake = Vector2.Zero;
            Vector2 draw_position = base.drawPosition.Value;
            if ( !Furniture.isDrawingLocationFurniture )
            {
                draw_position = new Vector2( x, y ) * 64f;
                draw_position.Y -= base.sourceRect.Height * 4 - base.boundingBox.Height;
            }
            if ( base.shakeTimer > 0 )
            {
                shake = new Vector2( Game1.random.Next( -1, 2 ), Game1.random.Next( -1, 2 ) );
            }
            spriteBatch.Draw( currTex.Texture, Game1.GlobalToLocal( Game1.viewport, draw_position + shake ), currTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, base.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, this.GetGlassDrawLayer() );
            if ( Furniture.isDrawingLocationFurniture )
            {
                int hatsDrawn = 0;
                for ( int i = 0; i < this.tankFish.Count; i++ )
                {
                    TankFish fish = this.tankFish[i];
                    float fish_layer = Utility.Lerp(this.GetFishSortRegion().Y, this.GetFishSortRegion().X, fish.zPosition / 20f);
                    fish_layer += 1E-07f * ( float ) i;
                    fish.Draw( spriteBatch, alpha, fish_layer );
                    if ( fish.fishIndex != 86 )
                    {
                        continue;
                    }
                    int hatsSoFar = 0;
                    foreach ( Item h in base.heldItems )
                    {
                        if ( h is Hat )
                        {
                            if ( hatsSoFar == hatsDrawn )
                            {
                                h.drawInMenu( spriteBatch, Game1.GlobalToLocal( fish.GetWorldPosition() + new Vector2( -30 + ( fish.facingLeft ? ( -4 ) : 0 ), -55f ) ), 0.75f, 1f, fish_layer + 1E-08f, StackDrawType.Hide );
                                hatsDrawn++;
                                break;
                            }
                            hatsSoFar++;
                        }
                    }
                }
                for ( int j = 0; j < this.floorDecorations.Count; j++ )
                {
                    if ( this.floorDecorations[ j ].HasValue )
                    {
                        KeyValuePair<Rectangle, Vector2> decoration = this.floorDecorations[j].Value;
                        Vector2 decoration_position = decoration.Value;
                        Rectangle decoration_source_rect = decoration.Key;
                        float decoration_layer = Utility.Lerp(this.GetFishSortRegion().Y, this.GetFishSortRegion().X, decoration_position.Y / 20f) - 1E-06f;
                        spriteBatch.Draw( this.GetAquariumTexture(), Game1.GlobalToLocal( new Vector2( ( float ) this.GetTankBounds().Left + decoration_position.X * 4f, ( float ) ( this.GetTankBounds().Bottom - 4 ) - decoration_position.Y * 4f ) ), decoration_source_rect, Color.White * alpha, 0f, new Vector2( decoration_source_rect.Width / 2, decoration_source_rect.Height - 4 ), 4f, SpriteEffects.None, decoration_layer );
                    }
                }
                foreach ( Vector4 bubble in this.bubbles )
                {
                    float layer = Utility.Lerp(this.GetFishSortRegion().Y, this.GetFishSortRegion().X, bubble.Z / 20f) - 1E-06f;
                    spriteBatch.Draw( this.GetAquariumTexture(), Game1.GlobalToLocal( new Vector2( ( float ) this.GetTankBounds().Left + bubble.X, ( float ) ( this.GetTankBounds().Bottom - 4 ) - bubble.Y - bubble.Z * 4f ) ), new Rectangle( 0, 240, 16, 16 ), Color.White * alpha, 0f, new Vector2( 8f, 8f ), 4f * bubble.W, SpriteEffects.None, layer );
                }
            }

            if ( frontTex != null )
            {
                if ( Furniture.isDrawingLocationFurniture )
                {
                    spriteBatch.Draw( frontTex.Texture, Game1.GlobalToLocal( Game1.viewport, this.drawPosition + ( ( base.shakeTimer > 0 ) ? new Vector2( Game1.random.Next( -1, 2 ), Game1.random.Next( -1, 2 ) ) : Vector2.Zero ) ), frontTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, base.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ( ( int ) this.furniture_type == 12 ) ? ( 2E-09f + base.tileLocation.Y / 100000f ) : ( ( float ) ( base.boundingBox.Value.Bottom - ( ( ( int ) this.furniture_type == 6 || ( int ) this.furniture_type == 17 || ( int ) this.furniture_type == 13 ) ? 48 : 8 ) ) / 10000f ) );
                }
                else
                {
                    spriteBatch.Draw( frontTex.Texture, Game1.GlobalToLocal( Game1.viewport, new Vector2( x * 64 + ( ( base.shakeTimer > 0 ) ? Game1.random.Next( -1, 2 ) : 0 ), y * 64 - ( this.sourceRect.Height * 4 - base.boundingBox.Height ) + ( ( base.shakeTimer > 0 ) ? Game1.random.Next( -1, 2 ) : 0 ) ) ), frontTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, base.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, ( ( int ) this.furniture_type == 12 ) ? ( 2E-09f + base.tileLocation.Y / 100000f ) : ( ( float ) ( base.boundingBox.Value.Bottom - ( ( ( int ) this.furniture_type == 6 || ( int ) this.furniture_type == 17 || ( int ) this.furniture_type == 13 ) ? 48 : 8 ) ) / 10000f ) );
                }
            }
        }

        public void drawAtNonTileSpot(SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
        {
            var currConfig = GetCurrentConfiguration();
            var currTex = Data.pack.GetTexture(currConfig.Texture, (int)currConfig.DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)currConfig.DisplaySize.Y * Game1.tileSize / Game1.pixelZoom);

            spriteBatch.Draw(currTex.Texture, location, currTex.Rect, Color.White * alpha, 0f, Vector2.Zero, 4f, base.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
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
            var ret = new CustomFishTankFurniture(Data);
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

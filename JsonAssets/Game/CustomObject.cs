using JsonAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JsonAssets.Game
{
    [XmlType( "Mods_JAObject" )]
    public class CustomObject : StardewValley.Object
    {
        public readonly NetString _sourcePack = new NetString();
        public readonly NetString _id = new NetString();

        [XmlIgnore]
        public string SourcePack => _sourcePack.Value;
        [XmlIgnore]
        public string Id => _id.Value;
        [XmlIgnore]
        public string FullId => $"{SourcePack}/{Id}";
        [XmlIgnore]
        public ObjectPackData Data => Mod.Find( FullId ) as ObjectPackData;

        public override string DisplayName { get => loadDisplayName(); set { } }

        public CustomObject() { }
        public CustomObject( ObjectPackData obj )
        {
            _sourcePack.Value = obj.parent.smapiPack.Manifest.UniqueID;
            _id.Value = obj.ID;

            ParentSheetIndex = Mod.BaseFakeObjectId;
            name = obj.ID;
            if ( obj.SellPrice.HasValue )
                price.Value = obj.SellPrice.Value;
            edibility.Value = obj.Edibility;
            type.Value = "Basic";
            category.Value = (int) obj.Category;
            price.Value = obj.SellPrice ?? 0;
            fragility.Value = fragility_Removable;

            canBeSetDown.Value = true;
            canBeGrabbed.Value = true;
            isHoedirt.Value = false;
            isSpawnedObject.Value = false;
            boundingBox.Value = new Rectangle( 0, 0, 64, 64 );
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields( _sourcePack, _id );
        }

        protected override string loadDisplayName()
        {
            return Data.Name;
        }

        public override bool canBeShipped()
        {
            return Data.SellPrice.HasValue;
        }

        public override bool canBeTrashed()
        {
            return Data.CanTrash;
        }

        public override bool canBeGivenAsGift()
        {
            return Data.IsGiftable;
        }

        public override string[] ModifyItemBuffs( string[] buffs )
        {
            var buffData = Data.EdibleBuffs ?? new ObjectPackData.FoodBuffsData();
            string[] buffStr = new string[]
            {
                buffData.Farming.ToString(),
                buffData.Fishing.ToString(),
                buffData.Mining.ToString(),
                "0",
                buffData.Luck.ToString(),
                buffData.Foraging.ToString(),
                "0",
                buffData.MaxStamina.ToString(),
                buffData.MagnetRadius.ToString(),
                buffData.Speed.ToString(),
                buffData.Defense.ToString(),
                buffData.Attack.ToString()
            };
            for ( int i = 0; i < Math.Min( buffStr.Length, buffs.Length ); ++i )
            {
                buffs[ i ] = buffStr[ i ];
            }

            return base.ModifyItemBuffs( buffs );
        }

        public override string getCategoryName()
        {
            return Data.CategoryTextOverride ?? base.getCategoryName();
        }

        public override Color getCategoryColor()
        {
            if ( Data.CategoryColorOverride.HasValue )
                return Data.CategoryColorOverride.Value;
            return base.getCategoryColor();
        }

        public override int healthRecoveredOnConsumption()
        {
            if ( Data.EatenHealthRestoredOverride.HasValue )
                return Data.EatenHealthRestoredOverride.Value;
            return base.healthRecoveredOnConsumption();
        }

        public override int staminaRecoveredOnConsumption()
        {
            if ( Data.EatenStaminaRestoredOverride.HasValue )
                return Data.EatenStaminaRestoredOverride.Value;
            return base.staminaRecoveredOnConsumption();
        }

        public override void drawTooltip( SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText )
        {
            base.drawTooltip( spriteBatch, ref x, ref y, font, alpha, overrideText );
            string str = "Mod: " + Data.parent.smapiPack.Manifest.Name;
            Utility.drawTextWithShadow( spriteBatch, Game1.parseText( str, Game1.smallFont, this.getDescriptionWidth() ), font, new Vector2( x + 16, y + 16 + 4 ), new Color( 100, 100, 100 ) );
            y += ( int ) font.MeasureString( Game1.parseText( str, Game1.smallFont, this.getDescriptionWidth() ) ).Y + 10;
        }

        public override Point getExtraSpaceNeededForTooltipSpecialIcons( SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom )
        {
            var ret = base.getExtraSpaceNeededForTooltipSpecialIcons(font, minWidth, horizontalBuffer, startingHeight, descriptionText, boldTitleText, moneyAmountToDisplayAtBottom );
            ret.Y = startingHeight;
            ret.Y += 48;
            return ret;
        }

        public override void drawWhenHeld( SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f )
        {
            var tex = Data.parent.GetTexture( Data.Texture, 16, 16 );
            spriteBatch.Draw( tex.Texture, objectPosition, tex.Rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max( 0f, ( float ) ( f.getStandingY() + 3 ) / 10000f ) );
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            if ( ( bool ) this.isRecipe )
            {
                transparency = 0.5f;
                scaleSize *= 0.75f;
            }
            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;
            
            if ( ( int ) base.parentSheetIndex != 590 && drawShadow )
            {
                spriteBatch.Draw( Game1.shadowTexture, location + new Vector2( 32f, 48f ), Game1.shadowTexture.Bounds, color * 0.5f, 0f, new Vector2( Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y ), 3f, SpriteEffects.None, layerDepth - 0.0001f );
            }
            var tex = Data.parent.GetTexture( Data.Texture, 16, 16 );
            spriteBatch.Draw( tex.Texture, location + new Vector2( ( int ) ( 32f * scaleSize ), ( int ) ( 32f * scaleSize ) ), tex.Rect, color * transparency, 0f, new Vector2( 8f, 8f ) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth );
            if ( shouldDrawStackNumber )
            {
                Utility.drawTinyDigits( this.stack, spriteBatch, location + new Vector2( ( float ) ( 64 - Utility.getWidthOfTinyDigitString( this.stack, 3f * scaleSize ) ) + 3f * scaleSize, 64f - 18f * scaleSize + 1f ), 3f * scaleSize, 1f, color );
            }
            if ( drawStackNumber != 0 && ( int ) this.quality > 0 )
            {
                Microsoft.Xna.Framework.Rectangle quality_rect = ((int)this.quality < 4) ? new Microsoft.Xna.Framework.Rectangle(338 + ((int)this.quality - 1) * 8, 400, 8, 8) : new Microsoft.Xna.Framework.Rectangle(346, 392, 8, 8);
                Texture2D quality_sheet = Game1.mouseCursors;
                float yOffset = ((int)this.quality < 4) ? 0f : (((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1f) * 0.05f);
                spriteBatch.Draw( quality_sheet, location + new Vector2( 12f, 52f + yOffset ), quality_rect, color * transparency, 0f, new Vector2( 4f, 4f ), 3f * scaleSize * ( 1f + yOffset ), SpriteEffects.None, layerDepth );
            }
        }

        public override void drawAsProp( SpriteBatch b )
        {
            if ( this.isTemporarilyInvisible )
            {
                return;
            }
            int x = (int)this.tileLocation.X;
            int y = (int)this.tileLocation.Y;

            b.Draw(Game1.shadowTexture, this.getLocalPosition(Game1.viewport) + new Vector2(32f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)this.getBoundingBox(new Vector2(x, y)).Bottom / 15000f);
            var tex = Data.parent.GetTexture( Data.Texture, 16, 16 );
            Texture2D objectSpriteSheet = tex.Texture;
            Vector2 position2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 32));
            Microsoft.Xna.Framework.Rectangle? sourceRectangle = tex.Rect;// GameLocation.getSourceRectForObject(base.ParentSheetIndex);
            Color white = Color.White;
            Vector2 origin = new Vector2(8f, 8f);
            _ = this.scale;
            b.Draw(objectSpriteSheet, position2, sourceRectangle, white, 0f, origin, (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)this.getBoundingBox(new Vector2(x, y)).Bottom / 10000f);
        }

        public override void draw( SpriteBatch spriteBatch, int x, int y, float alpha = 1 )
        {
            if ( isTemporarilyInvisible )
                return;

            if ( !Game1.eventUp || !Game1.CurrentEvent.isTileWalkedOn( x, y ) )
            {
                spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 51 + 4)), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)this.getBoundingBox(new Vector2(x, y)).Bottom / 15000f);
                var tex = Data.parent.GetTexture( Data.Texture, 16, 16 );
                Texture2D objectSpriteSheet = tex.Texture;
                Vector2 position3 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
                Microsoft.Xna.Framework.Rectangle? sourceRectangle2 = tex.Rect; // GameLocation.getSourceRectForObject(base.ParentSheetIndex);
                Color color2 = Color.White * alpha;
                Vector2 origin2 = new Vector2(8f, 8f);
                _ = this.scale;
                spriteBatch.Draw(objectSpriteSheet, position3, sourceRectangle2, color2, 0f, origin2, (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(this.isPassable() ? this.getBoundingBox(new Vector2(x, y)).Top : this.getBoundingBox(new Vector2(x, y)).Bottom) / 10000f);
            }
        }

        public override void draw( SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1 )
        {
            if ( isTemporarilyInvisible )
                return;

            if ( !Game1.eventUp || !Game1.CurrentEvent.isTileWalkedOn( xNonTile / 64, yNonTile / 64 ) )
            {
                spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32, yNonTile + 51 + 4)), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, layerDepth - 1E-06f);
                var tex = Data.parent.GetTexture( Data.Texture, 16, 16 );
                Texture2D objectSpriteSheet = tex.Texture;
                Vector2 position2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), yNonTile + 32 + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
                Microsoft.Xna.Framework.Rectangle? sourceRectangle = tex.Rect; // GameLocation.getSourceRectForObject(base.ParentSheetIndex);
                Color color = Color.White * alpha;
                Vector2 origin = new Vector2(8f, 8f);
                _ = this.scale;
                spriteBatch.Draw(objectSpriteSheet, position2, sourceRectangle, color, 0f, origin, (this.scale.Y > 1f) ? this.getScale().Y : 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
            }
        }

        public override Item getOne()
        {
            var ret = new CustomObject( Data );
            // TODO: All the other fields objects does??
            ret.Quality = Quality;
            ret.Stack = 1;
            ret.Price = Price;
            ret._GetOneFrom( this );
            return ret;
        }

        public override bool canStackWith( ISalable other )
        {
            if ( !( other is CustomObject obj ) )
                return false;

            return obj.FullId == FullId && base.canStackWith( other );
        }

        public override bool canBePlacedHere( GameLocation l, Vector2 tile )
        {
            return isPlaceable() && !l.isTileOccupiedForPlacement( tile, this );
        }

        public override bool isPlaceable()
        {
            return Data.Placeable;
        }

        public override bool performToolAction( Tool t, GameLocation location )
        {
            if ( t == null )
                return false;

            if ( !( t is MeleeWeapon ) && t.isHeavyHitter() )
            {
                location.playSound( "hammer" );
                location.debris.Add( new Debris( this, this.TileLocation * Game1.tileSize + new Vector2( Game1.tileSize / 2, Game1.tileSize / 2 ) ) );
                return true;
            }

            return false;
        }

        public override bool needsToBeDonated()
        {
            return false; // TODO: Implement as future feature?
        }

        public override string getDescription()
        {
            return Data.Description;
        }

        public override int salePrice()
        {
            return Data.ForcePriceOnAllInstances ? (Data.SellPrice ?? 0) : Price;
        }

        protected override void _PopulateContextTags( HashSet<string> tags )
        {
            base._PopulateContextTags( tags );
            foreach ( var tag in Data.ContextTags )
                tags.Add( tag );
        }
    }
}

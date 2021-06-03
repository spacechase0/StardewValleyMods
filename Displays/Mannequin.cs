using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceCore;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Displays
{
    [XmlType("Mods_spacechase0_Mannequin")]
    public class Mannequin : StardewValley.Object
    {
        public static Texture2D tex = Mod.instance.Helper.Content.Load< Texture2D >( "assets/mannequin-base.png" );
        public static Texture2D texM = Mod.instance.Helper.Content.Load< Texture2D >( "assets/mannequin-male.png" );
        public static Texture2D texF = Mod.instance.Helper.Content.Load< Texture2D >( "assets/mannequin-female.png" );

        public enum MannequinType
        {
            Plain,
            Magic,
        }
        public enum MannequinGender
        {
            Male,
            Female,
        }

        public readonly NetEnum< MannequinType > mannType = new NetEnum<MannequinType>( MannequinType.Plain );
        public readonly NetEnum< MannequinGender > mannGender = new NetEnum<MannequinGender>( MannequinGender.Male );
        public readonly NetInt facing = new NetInt( Game1.down );

        public readonly NetRef<Hat> hat = new NetRef<Hat>();
        public readonly NetRef<Clothing> shirt = new NetRef<Clothing>();
        public readonly NetRef<Clothing> pants = new NetRef<Clothing>();
        public readonly NetRef<Boots> boots = new NetRef<Boots>();

        // TODO: Magic properties (skin color, eye color, hair stuff, accessories)

        [XmlIgnore]
        private Farmer farmerForRendering;

        public override string DisplayName
        {
            get => name;
            set { }
        }

        public Mannequin() { }
        public Mannequin( MannequinType type, MannequinGender gender, Vector2 placement )
        {
            mannType.Value = type;
            mannGender.Value = gender;
            //ParentSheetIndex = MODID;
            name = loadDisplayName();
            DisplayName = loadDisplayName();
            bigCraftable.Value = true;
            Type = "Crafting"; // Makes performObjectDropInAction work for non-objects

            TileLocation = placement;
            boundingBox.Value = new Rectangle( ( int ) placement.X * 64, ( int ) placement.Y * 64, 64, 64 );
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields( mannType, mannGender, facing, hat, shirt, pants, boots );

            mannType.fieldChangeEvent += ( field, oldVal, newVal ) => CacheFarmerSprite();
            facing.fieldChangeEvent += ( field, oldVal, newVal ) => CacheFarmerSprite();
            hat.fieldChangeEvent += ( field, oldVal, newVal ) => CacheFarmerSprite();
            shirt.fieldChangeEvent += ( field, oldVal, newVal ) => CacheFarmerSprite();
            pants.fieldChangeEvent += ( field, oldVal, newVal ) => CacheFarmerSprite();
            boots.fieldChangeEvent += ( field, oldVal, newVal ) => CacheFarmerSprite();
        }

        private void CacheFarmerSprite()
        {
            if ( farmerForRendering == null )
                farmerForRendering = new Farmer();
            farmerForRendering.changeGender( mannGender == MannequinGender.Male );
            if ( mannGender == MannequinGender.Female )
                farmerForRendering.changeHairStyle( 16 );
            farmerForRendering.faceDirection( facing.Value );
            farmerForRendering.hat.Value = hat.Value;
            farmerForRendering.shirtItem.Value = shirt.Value;
            if ( shirt.Value != null )
            {
                farmerForRendering.shirt.Value = mannGender.Value == MannequinGender.Male ? shirt.Value.indexInTileSheetMale.Value : shirt.Value.indexInTileSheetFemale.Value;
            }
            farmerForRendering.pantsItem.Value = pants.Value;
            if ( pants.Value != null )
            {
                farmerForRendering.pants.Value = mannGender.Value == MannequinGender.Male ? pants.Value.indexInTileSheetMale.Value : pants.Value.indexInTileSheetFemale.Value;
                farmerForRendering.pantsColor.Value = pants.Value.clothesColor.Value;
            }
            farmerForRendering.boots.Value = boots.Value;
            if ( boots.Value != null )
            {
                farmerForRendering.changeShoeColor( boots.Value.indexInColorSheet.Value );
            }
            if ( mannType == MannequinType.Plain )
            {
                farmerForRendering.changeHairColor( Color.Transparent );
                farmerForRendering.FarmerRenderer.textureName.Value = "Characters\\Farmer\\farmer_transparent";
            }
        }

        protected override string loadDisplayName()
        {
            string type = Mod.instance.Helper.Translation.Get( "mannequin.type." + mannType.Value.ToString() );
            string gender = Mod.instance.Helper.Translation.Get( "mannequin.gender." + mannGender.Value.ToString() );
            return Mod.instance.Helper.Translation.Get( "mannequin.name", new { type = type, gender = gender } );
        }

        public override string getDescription()
        {
            return Mod.instance.Helper.Translation.Get( "mannequin.desc" );
        }

        public override bool canStackWith( ISalable other )
        {
            if ( other is Mannequin m )
            {
                return m.mannType.Value == mannType.Value && m.mannGender.Value == mannGender.Value;
            }
            return false;
        }

        public override Item getOne()
        {
            var ret = new Mannequin( mannType.Value, mannGender.Value, Vector2.Zero );
            ret.hat.Value = (Hat) hat.Value?.getOne();
            ret.shirt.Value = (Clothing) shirt.Value?.getOne();
            ret.pants.Value = (Clothing) pants.Value?.getOne();
            ret.boots.Value = (Boots) boots.Value?.getOne();
            ret._GetOneFrom( this );
            return ret;
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool placementAction( GameLocation location, int x, int y, Farmer who = null )
        {
            Vector2 placementTile = new Vector2(x / 64, y / 64);
            var m = new Mannequin( mannType.Value, mannGender.Value, placementTile );
            if ( who != null )
                ;// m.facing.Value = who.FacingDirection;
            location.Objects.Add( placementTile, m );
            location.playSound( "woodyStep" );
            return true;
        }

        public override bool performToolAction( Tool t, GameLocation location )
        {
            if ( t == null )
                return false;

            if ( !( t is MeleeWeapon ) && t.isHeavyHitter() )
            {
                if ( hat.Value != null || shirt.Value != null || pants.Value != null || boots.Value != null )
                {
                    if ( hat.Value != null )
                    {
                        location.debris.Add( new Debris( this.hat.Value, new Vector2( ( TileLocation.X + 0.5f ) * Game1.tileSize, ( TileLocation.Y + 0.5f ) * Game1.tileSize ) ) );
                        hat.Value = null;
                    }
                    else if ( shirt.Value != null )
                    {
                        location.debris.Add( new Debris( this.shirt.Value, new Vector2( ( TileLocation.X + 0.5f ) * Game1.tileSize, ( TileLocation.Y + 0.5f ) * Game1.tileSize ) ) );
                        shirt.Value = null;
                    }
                    else if ( pants.Value != null )
                    {
                        location.debris.Add( new Debris( this.pants.Value, new Vector2( ( TileLocation.X + 0.5f ) * Game1.tileSize, ( TileLocation.Y + 0.5f ) * Game1.tileSize ) ) );
                        pants.Value = null;
                    }
                    else if ( boots.Value != null )
                    {
                        location.debris.Add( new Debris( this.boots.Value, new Vector2( ( TileLocation.X + 0.5f ) * Game1.tileSize, ( TileLocation.Y + 0.5f ) * Game1.tileSize ) ) );
                        boots.Value = null;
                    }
                    location.playSound( "hammer" );
                    this.shakeTimer = 100;
                    return false;
                }
                location.objects.Remove( TileLocation );
                location.debris.Add( new Debris( new Mannequin( mannType.Value, mannGender.Value, Vector2.Zero ), new Vector2( ( TileLocation.X + 0.5f ) * Game1.tileSize, ( TileLocation.Y + 0.5f ) * Game1.tileSize ) ) );
                return false;
            }

            return false;
        }

        public override bool checkForAction( Farmer who, bool justCheckingForActivity = false )
        {
            if ( who.CurrentItem == null )
            {
                if ( justCheckingForActivity )
                    return true;

                //if ( who.hat.Value != null )
                {
                    var tmp = hat.Value;
                    hat.Value = who.hat.Value;
                    who.hat.Value = tmp;
                }
                //if ( who.shirtItem.Value != null )
                {
                    var tmp = shirt.Value;
                    shirt.Value = who.shirtItem.Value;
                    who.shirtItem.Value = tmp;
                }
                //if ( who.pantsItem.Value != null )
                {
                    var tmp = pants.Value;
                    pants.Value = who.pantsItem.Value;
                    who.pantsItem.Value = tmp;
                }
                if ( who.boots.Value != null )
                {
                    var tmp = boots.Value;
                    boots.Value = who.boots.Value;
                    who.boots.Value = tmp;
                }
                return true;
            }

            return false;

        }

        public override bool performObjectDropInAction( Item dropInItem, bool probe, Farmer who )
        {
            if ( probe && ( dropInItem is Hat || dropInItem is Clothing || dropInItem is Boots ) )
                return true;

            if ( dropInItem is Hat hat )
            {
                if ( this.hat.Value != null )
                    who.currentLocation.debris.Add( new Debris( this.hat.Value, new Vector2( ( TileLocation.X + 0.5f ) * Game1.tileSize, ( TileLocation.Y + 0.5f ) * Game1.tileSize ) ) );
                this.hat.Value = hat;
                return true;
            }
            else if ( dropInItem is Clothing clothing )
            {
                if ( clothing.clothesType.Value == (int) Clothing.ClothesType.SHIRT )
                {
                    if ( this.shirt.Value != null )
                        who.currentLocation.debris.Add( new Debris( this.shirt.Value, new Vector2( ( TileLocation.X + 0.5f ) * Game1.tileSize, ( TileLocation.Y + 0.5f ) * Game1.tileSize ) ) );
                    this.shirt.Value = clothing;
                    return true;
                }
                else if ( clothing.clothesType.Value == ( int ) Clothing.ClothesType.PANTS )
                {
                    if ( this.pants.Value != null )
                        who.currentLocation.debris.Add( new Debris( this.pants.Value, new Vector2( ( TileLocation.X + 0.5f ) * Game1.tileSize, ( TileLocation.Y + 0.5f ) * Game1.tileSize ) ) );
                    this.pants.Value = clothing;
                    return true;
                }
            }
            else if ( dropInItem is Boots boots )
            {
                if ( this.boots.Value != null )
                    who.currentLocation.debris.Add( new Debris( this.boots.Value, new Vector2( ( TileLocation.X + 0.5f ) * Game1.tileSize, ( TileLocation.Y + 0.5f ) * Game1.tileSize ) ) );
                this.boots.Value = boots;
                return true;
            }

            return false;
        }

        public override void drawWhenHeld( SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f )
        {
            var tex = Mannequin.tex;
            if ( mannGender.Value == MannequinGender.Male )
                tex = texM;
            else if ( mannGender.Value == MannequinGender.Female )
                tex = texF;

            spriteBatch.Draw( tex, objectPosition, null, Color.White, 0, Vector2.Zero, 4f, SpriteEffects.None, Math.Max( 0f, ( float ) ( f.getStandingY() + 3 ) / 10000f ) );
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;

            var tex = Mannequin.tex;
            if ( mannGender.Value == MannequinGender.Male )
                tex = texM;
            else if ( mannGender.Value == MannequinGender.Female )
                tex = texF;

            spriteBatch.Draw( tex, location + new Vector2( 32f, 32f ), null, color * transparency, 0f, new Vector2( 8f, 16f ), 4f * ( ( ( double ) scaleSize < 0.2 ) ? scaleSize : ( scaleSize / 2f ) ), SpriteEffects.None, layerDepth );
            if ( shouldDrawStackNumber )
            {
                Utility.drawTinyDigits( this.Stack, spriteBatch, location + new Vector2( ( float ) ( 64 - Utility.getWidthOfTinyDigitString( this.Stack, 3f * scaleSize ) ) + 3f * scaleSize, 64f - 18f * scaleSize + 2f ), 3f * scaleSize, 1f, color );
            }
        }

        public override void draw( SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1 )
        {
            var tex = Mannequin.tex;
            if ( mannGender.Value == MannequinGender.Male )
                tex = texM;
            else if ( mannGender.Value == MannequinGender.Female )
                tex = texF;

            Vector2 scaleFactor = this.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
            Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            spriteBatch.Draw( tex, destination, null, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth );
        }

        public override void draw( SpriteBatch spriteBatch, int x, int y, float alpha = 1 )
        {
            var tex = Mannequin.tex;
            if ( mannGender.Value == MannequinGender.Male )
                tex = texM;
            else if ( mannGender.Value == MannequinGender.Female )
                tex = texF;

            Vector2 scaleFactor = this.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
            Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
            spriteBatch.Draw( tex, destination, null, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer );
            
            if ( farmerForRendering == null )
                CacheFarmerSprite();
            //SpaceShared.Log.trace( "meow!? " + farmerForRendering.shirtItem.Value + " " + farmerForRendering.pantsItem.Value + " " + farmerForRendering.hat.Value );
            farmerForRendering.position.Value = new Vector2( x * 64, y * 64 + 12 );
            farmerForRendering.FarmerRenderer.draw( spriteBatch, farmerForRendering.FarmerSprite, farmerForRendering.FarmerSprite.sourceRect, farmerForRendering.getLocalPosition( Game1.viewport ), new Vector2( 0, farmerForRendering.GetBoundingBox().Height ), draw_layer + 0.001f, Color.White, 0, farmerForRendering );
        }
    }
}

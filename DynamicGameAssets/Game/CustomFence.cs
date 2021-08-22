using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceShared;
using StardewValley;
using StardewValley.Tools;

namespace DynamicGameAssets.Game
{
    [XmlType( "Mods_DGAFence" )]
    public class CustomFence : Fence, IDGAItem
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
        public FencePackData Data => Mod.Find( FullId ) as FencePackData;

        public CustomFence()
        {
            this.fenceTexture = new Lazy<Texture2D>( () => Data.parent.GetTexture( Data.PlacedTilesheet, 48, 325 ).Texture );
        }

        public CustomFence( FencePackData data )
        :   this()
        {
            _sourcePack.Value = data.parent.smapiPack.Manifest.UniqueID;
            _id.Value = data.ID;

            this.Name = Id;
            this.whichType.Value = FullId.GetHashCode();
            this.ResetHealth( 0 );

            this.CanBeSetDown = true;
            this.CanBeGrabbed = true;
            this.Type = "Crafting";
        }
        public CustomFence( FencePackData data, Vector2 tileLocation )
        :   this( data )
        {
            this.tileLocation.Value = tileLocation;
            base.boundingBox.Value = new Rectangle( ( int ) tileLocation.X * 64, ( int ) tileLocation.Y * 64, 64, 64 );
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields( _sourcePack, _id );
        }

        protected override string loadDisplayName()
        {
            return Data.Name;
        }

        public override string getDescription()
        {
            return Game1.parseText( Data.Description, Game1.smallFont, this.getDescriptionWidth() );
        }

        public override void ResetHealth( float amount_adjustment )
        {
            this.maxHealth.Value = this.health.Value = Data.MaxHealth;
        }

        public override void dropItem( GameLocation location, Vector2 origin, Vector2 destination )
        {
            location.debris.Add( new Debris( this.getOne(), origin, destination ) );
        }

        public override bool placementAction( GameLocation location, int x, int y, Farmer who = null )
        {
            Vector2 placementTile = new Vector2(x / 64, y / 64);
            if ( location.objects.ContainsKey( placementTile ) )
                return false;
            location.objects.Add( placementTile, new CustomFence( Data, placementTile ) );
            location.playSound( Data.PlacementSound );
            return true;
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool performToolAction( Tool t, GameLocation location )
        {
            var Game1_multiplayer = Mod.instance.Helper.Reflection.GetField< Multiplayer >( typeof( Game1 ), "multiplayer" ).GetValue();

            if ( base.heldObject.Value != null && t != null && !( t is MeleeWeapon ) && t.isHeavyHitter() )
            {
                StardewValley.Object value = base.heldObject.Value;
                base.heldObject.Value.performRemoveAction( base.tileLocation, location );
                base.heldObject.Value = null;
                Game1.createItemDebris( value.getOne(), base.TileLocation * 64f, -1 );
                location.playSound( "axchop" );
                return false;
            }
            if ( ( bool ) this.isGate && t != null && ( t is Axe || t is Pickaxe ) )
            {
                location.playSound( "axchop" );
                Game1.createObjectDebris( 325, ( int ) base.tileLocation.X, ( int ) base.tileLocation.Y, Game1.player.UniqueMultiplayerID, Game1.player.currentLocation );
                location.objects.Remove( base.tileLocation );
                Game1.createRadialDebris( location, 12, ( int ) base.tileLocation.X, ( int ) base.tileLocation.Y, 6, resource: false );
                Game1_multiplayer.broadcastSprites( location, new TemporaryAnimatedSprite( 12, new Vector2( base.tileLocation.X * 64f, base.tileLocation.Y * 64f ), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f ) );
            }
            if ( t == null || ( t is Pickaxe && Data.BreakTool == FencePackData.ToolType.Pickaxe ) || ( t is Axe && Data.BreakTool == FencePackData.ToolType.Axe ) )
            {
                location.playSound( Data.BreakTool == FencePackData.ToolType.Axe ? "axchop" : "hammer" );
                location.objects.Remove( base.tileLocation );
                for ( int i = 0; i < 4; i++ )
                {
                    location.temporarySprites.Add( new CosmeticDebris( this.fenceTexture.Value, new Vector2( base.tileLocation.X * 64f + 32f, base.tileLocation.Y * 64f + 32f ), ( float ) Game1.random.Next( -5, 5 ) / 100f, ( float ) Game1.random.Next( -64, 64 ) / 30f, ( float ) Game1.random.Next( -800, -100 ) / 100f, ( int ) ( ( base.tileLocation.Y + 1f ) * 64f ), new Rectangle( 32 + Game1.random.Next( 2 ) * 16 / 2, 96 + Game1.random.Next( 2 ) * 16 / 2, 8, 8 ), Color.White, ( Game1.soundBank != null ) ? Game1.soundBank.GetCue( "shiny4" ) : null, null, 0, 200 ) );
                }
                Game1.createRadialDebris( location, 12, ( int ) base.tileLocation.X, ( int ) base.tileLocation.Y, 6, resource: false );
                Game1_multiplayer.broadcastSprites( location, new TemporaryAnimatedSprite( 12, new Vector2( base.tileLocation.X * 64f, base.tileLocation.Y * 64f ), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f ) );

                location.debris.Add( new Debris( this.getOne(), base.tileLocation.Value * 64f + new Vector2( 32f, 32f ) ) );
            }
            return false;
        }

        public bool CanRepairWithThisItem( Item item )
        {
            if ( this.health.Value > 1f )
                return false;

            if ( Data.RepairMaterial.Matches( item ) )
                return true;

            return false;
        }

        public override string GetRepairSound()
        {
            return Data.RepairSound;
        }

        public override Point getExtraSpaceNeededForTooltipSpecialIcons( SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom )
        {
            var ret = base.getExtraSpaceNeededForTooltipSpecialIcons(font, minWidth, horizontalBuffer, startingHeight, descriptionText, boldTitleText, moneyAmountToDisplayAtBottom );
            ret.Y = startingHeight;
            ret.Y += 48;
            return ret;
        }

        public override void drawTooltip( SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText )
        {
            base.drawTooltip( spriteBatch, ref x, ref y, font, alpha, overrideText );
            string str = "Mod: " + Data.parent.smapiPack.Manifest.Name;
            Utility.drawTextWithShadow( spriteBatch, Game1.parseText( str, Game1.smallFont, this.getDescriptionWidth() ), font, new Vector2( x + 16, y + 16 + 4 ), new Color( 100, 100, 100 ) );
            y += ( int ) font.MeasureString( Game1.parseText( str, Game1.smallFont, this.getDescriptionWidth() ) ).Y + 10;
        }

        public override void drawWhenHeld( SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f )
        {
            spriteBatch.Draw( Data.GetTexture().Texture, objectPosition - new Vector2( 0f, 0 ), Data.GetTexture().Rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, ( float ) ( f.getStandingY() + 1 ) / 10000f );
        }


        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;

            if ( ( int ) base.parentSheetIndex != 590 && drawShadow )
            {
                spriteBatch.Draw( Game1.shadowTexture, location + new Vector2( 32f, 48f ), Game1.shadowTexture.Bounds, color * 0.5f, 0f, new Vector2( Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y ), 3f, SpriteEffects.None, layerDepth - 0.0001f );
            }
            var tex = Data.parent.GetTexture( Data.ObjectTexture, 16, 16 );
            spriteBatch.Draw( tex.Texture, location + new Vector2( ( int ) ( 32f * scaleSize ), ( int ) ( 32f * scaleSize ) ), tex.Rect, color * transparency, 0f, new Vector2( 8f, 8f ) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth );
            if ( shouldDrawStackNumber )
            {
                Utility.drawTinyDigits( this.stack, spriteBatch, location + new Vector2( ( float ) ( 64 - Utility.getWidthOfTinyDigitString( this.stack, 3f * scaleSize ) ) + 3f * scaleSize, 64f - 18f * scaleSize + 1f ), 3f * scaleSize, 1f, color );
            }
        }

        public override Item getOne()
        {
            var ret = new CustomFence( Data );
            // TODO: All the other fields objects does??
            ret.Stack = 1;
            ret._GetOneFrom( this );
            return ret;
        }
    }
}

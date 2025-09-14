using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MisappliedPhysicalities.VirtualProperties;
using Netcode;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;

namespace MisappliedPhysicalities.Game.Objects
{
    [XmlType("Mods_spacechase0_MisappliedPhysicalities_ConveyorBelt")]
    public class ConveyorBelt : StardewValley.Object, IUpdatesEvenWithoutFarmer, ILogicObject
    {
        public readonly NetEnum< Side > facing = new();
        public readonly NetRef< Item > item = new(); // heldObject only works with `Object`s
        public readonly NetFloat progress = new();
        public readonly NetEnum< Layer > onLayer = new();
        public readonly NetBool reversed = new(); // from logic signals

        public ConveyorBelt()
            : this( Vector2.Zero, Layer.GroundLevel )
        {
        }

        public ConveyorBelt( Vector2 placement, Layer layer )
        {
            name = DisplayName = loadDisplayName();
            bigCraftable.Value = true;
            Type = "Crafting";

            TileLocation = placement;
            onLayer.Value = layer;
            boundingBox.Value = new Rectangle( ( int ) placement.X * Game1.tileSize, ( int ) placement.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize );
        }

        public Side GetActualFacing()
        {
            if ( reversed.Value )
                return facing.Value.GetOpposite();
            return facing.Value;
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields( facing, item, progress, onLayer, reversed );
        }

        protected override string loadDisplayName()
        {
            return Mod.instance.Helper.Translation.Get( "item.conveyor.name" );
        }

        public override string getDescription()
        {
            return Mod.instance.Helper.Translation.Get( "item.conveyor.description" );
        }

        public override bool canStackWith( ISalable other )
        {
            return other is ConveyorBelt;
        }

        public override Item getOne()
        {
            var ret = new ConveyorBelt( Vector2.Zero, Layer.GroundLevel );
            ret._GetOneFrom( this );
            return ret;
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool placementAction( GameLocation location, int x, int y, Farmer who = null )
        {
            Vector2 tile = new Vector2( x / Game1.tileSize, y / Game1.tileSize );

            Layer layer = Layer.GroundLevel;

            if ( who != null )
            {
                if ( Mod.dga.GetDGAItemId( who.hat.Value ) == Items.XrayGogglesId )
                {
                    layer = Layer.Underground;
                }
            }

            var target = location.GetContainerForLayer( layer );
            if ( target.ContainsKey( tile ) )
            {
                if ( target[ tile ] is NullObject )
                    target.Remove( tile );
                else
                {
                    // TODO: play sound, message
                    return false;
                }
            }


            var c = new ConveyorBelt( tile, layer );
            if ( who != null )
                c.facing.Value = who.FacingDirection.GetSideFromFacingDirection().Value;

            target.Add( tile, c );
            location.playSound( "woodyStep" );

            return true;
        }

        public void DropItem( GameLocation loc )
        {
            if ( item.Value != null )
            {
                loc.debris.Add( new Debris( item.Value, ( TileLocation + GetRelativeItemPos() ) * Game1.tileSize ) );
                item.Value = null;
            }
        }

        public override bool performToolAction( Tool t, GameLocation location )
        {
            if ( t == null )
                return false;

            if ( onLayer.Value != Layer.GroundLevel || ( t is not MeleeWeapon && t.isHeavyHitter() ) )
            {
                DropItem( location );
                location.GetContainerForLayer( onLayer.Value ).Remove( TileLocation );
                location.debris.Add( new Debris( new ConveyorBelt(), new Vector2( tileLocation.X + 0.5f, tileLocation.Y + 0.5f ) * Game1.tileSize ) );
                return false;
            }

            return false;
        }

        private Vector2 GetRelativeItemPos()
        {
            switch ( GetActualFacing() )
            {
                case Side.Up: return new Vector2( 0.5f, 1 - progress.Value );
                case Side.Down: return new Vector2( 0.5f, progress.Value );
                case Side.Left: return new Vector2( 1 - progress.Value, 0.5f );
                case Side.Right: return new Vector2( progress.Value, 0.5f );
            }

            return new Vector2( 0.5f, 0.5f );
        }

        public override bool checkForAction( Farmer who, bool justCheckingForActivity = false )
        {
            if ( item.Value != null )
            {
                if ( justCheckingForActivity )
                    return true;

                DropItem( who.currentLocation );
                return true;
            }

            return false;
        }

        public override bool performObjectDropInAction( Item dropInItem, bool probe, Farmer who )
        {
            // These items respawn at the beginning of the day if the game can't find them,
            // and it doesn't know to check in conveyor belts, so don't allow them on here.
            // Maybe in the future patch that logic?
            if ( dropInItem is Pickaxe || dropInItem is Hoe ||
                 dropInItem is Axe || dropInItem is WateringCan ||
                 dropInItem is Wand || dropInItem is MeleeWeapon mw && mw.Name.Equals( "Scythe" ) )
                return false;

            if ( dropInItem != null && item.Value == null )
            {
                if ( !probe )
                {
                    item.Value = dropInItem.getOne();
                    progress.Value = 0.5f;
                }
                return true;
            }

            return false;
        }

        public override void drawWhenHeld( SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f )
        {
            var texRect = new Rectangle( 0, 0, 16, 32 );
            spriteBatch.Draw( Assets.ConveyorBelt, objectPosition, texRect, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, ( f.getStandingY() + 3 ) / 1000f );
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;

            var texRect = new Rectangle( 0, 0, 16, 32 );
            spriteBatch.Draw( Assets.ConveyorBelt, location + new Vector2( 32f, 32f ), texRect, color * transparency, 0f, new Vector2( 8f, 16f ), 4f * ( ( ( double ) scaleSize < 0.2 ) ? scaleSize : ( scaleSize / 2f ) ), SpriteEffects.None, layerDepth );
            if ( shouldDrawStackNumber )
            {
                Utility.drawTinyDigits( this.Stack, spriteBatch, location + new Vector2( ( float ) ( 64 - Utility.getWidthOfTinyDigitString( this.Stack, 3f * scaleSize ) ) + 3f * scaleSize, 64f - 18f * scaleSize + 2f ), 3f * scaleSize, 1f, color );
            }
        }

        public override void draw( SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1 )
        {
            var texRect = new Rectangle( 0, 0, 16, 32 );

            Vector2 scaleFactor = this.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
            Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            spriteBatch.Draw( Assets.ConveyorBelt, destination, texRect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth );
        }

        private Rectangle GetMovingPartTexRect()
        {
            //float p = Math.Min( progress.Value, 1 );
            float p = Game1.ticks % 60 / 60f;
            switch ( GetActualFacing() )
            {
                case Side.Down: return new Rectangle( 16, 16 - ( int ) ( p * 16 ), 16, 16 );
                case Side.Up: return new Rectangle( 16, 0 + ( int ) ( p * 16 ), 16, 16 );
                case Side.Right: return new Rectangle( 48 - ( int ) ( p * 16 ), 0, 16, 16 );
                case Side.Left: return new Rectangle( 32 + ( int ) ( p * 16 ), 0, 16, 16 );
            }
            return new Rectangle( 16, 0, 16, 16 );
        }

        public override void draw( SpriteBatch spriteBatch, int x, int y, float alpha = 1 )
        {
            var texRect = new Rectangle( 0, 0, 16, 32 );

            Vector2 scaleFactor = this.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
            Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
            spriteBatch.Draw( Assets.ConveyorBelt, destination, texRect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer - 0.001f );

            texRect = GetMovingPartTexRect();
            destination.Y += destination.Height / 2;
            destination.Y -= 4 * Game1.pixelZoom;
            destination.Height /= 2;
            spriteBatch.Draw( Assets.ConveyorBelt, destination, texRect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer );

            if ( item.Value != null )
            {
                position = new Vector2( position.X - 32, position.Y + 32 ) + GetRelativeItemPos() * Game1.tileSize;
                if ( GetActualFacing() == Side.Left || GetActualFacing() == Side.Right )
                    position.Y -= 32;
                item.Value.drawInMenu( spriteBatch, position, 1f, 1, draw_layer + 0.01f, StackDrawType.Hide, Color.White, true );
            }
        }

        private Vector2 GetNextTile()
        {
            switch ( GetActualFacing() )
            {
                case Side.Up: return TileLocation + new Vector2( 0, -1 );
                case Side.Down: return TileLocation + new Vector2( 0, 1 );
                case Side.Left: return TileLocation + new Vector2( -1, 0 );
                case Side.Right: return TileLocation + new Vector2( 1, 0 );
            }
            return TileLocation;
        }

        public void UpdateEvenWithoutFarmer( GameLocation loc, GameTime time )
        {
            if ( item.Value != null )
            {
                progress.Value += ( float ) time.ElapsedGameTime.TotalSeconds;
                if ( progress.Value >= 1 )
                {
                    var objs = loc.GetContainerForLayer( onLayer.Value );
                    var next = objs.ContainsKey( GetNextTile() ) ? objs[ GetNextTile() ] : null;

                    if ( next == null && onLayer.Value.GetBelow() != null )
                    {
                        objs = loc.GetContainerForLayer( onLayer.Value.GetBelow().Value );
                        next = objs.ContainsKey( GetNextTile() ) ? objs[ GetNextTile() ] : null;
                    }
                    if ( next == null && onLayer.Value.GetAbove() != null )
                    {
                        objs = loc.GetContainerForLayer( onLayer.Value.GetAbove().Value );
                        next = objs.ContainsKey( GetNextTile() ) ? objs[ GetNextTile() ] : null;
                    }

                    if ( next is ConveyorBelt c )
                    {
                        if ( c.item.Value == null )
                        {
                            c.item.Value = item.Value;
                            c.progress.Value = 0;
                            item.Value = null;
                            progress.Value = 0;

                            if ( GetActualFacing() != c.GetActualFacing() )
                                c.progress.Value = 0.25f;
                        }
                        else
                        {
                            progress.Value = 1;
                        }
                    }
                    else if ( next is Chest chest1 )
                    {
                        var remaining = chest1.addItem( item.Value );
                        item.Value = remaining;
                        DropItem( loc );
                    }
                    else if ( next is StardewValley.Object obj && obj.heldObject.Value is Chest chest2 )
                    {
                        var remaining = chest2.addItem( item.Value );
                        item.Value = remaining;
                        DropItem( loc );
                    }
                    else
                    {
                        DropItem( loc );
                        progress.Value = 0;
                    }
                }
            }
        }

        public InOutType GetLogicTypeForSide( Side side )
        {
            if ( side == facing.Value || side == facing.Value.GetOpposite() || side == Side.Above )
                return InOutType.None;

            if ( side == Side.Below )
                return InOutType.Input;

            return InOutType.Output;
        }

        public double GetLogicFrom( Side side )
        {
            if ( side == facing.Value || side == facing.Value.GetOpposite() ||
                 side == Side.Above || side == Side.Below )
                return 0;

            if ( item.Value != null )
                return 0.5 + progress.Value / 2;

            return 0;
        }

        public void SendLogicTo( Side side, double signal )
        {
            if ( side == Side.Below )
            {
                bool oldRev = reversed.Value;
                reversed.Value = signal >= 0.5;
                if ( oldRev != reversed.Value && item.Value != null )
                {
                    progress.Value = 1 - progress.Value;
                }
            }
        }
    }
}

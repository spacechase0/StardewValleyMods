using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MisappliedPhysicalities.VirtualProperties;
using StardewValley;
using StardewValley.Objects;

namespace MisappliedPhysicalities.Game.Objects
{
    [XmlType( "Mods_spacechase0_MisappliedPhysicalities_Unhopper" )]
    public class Unhopper : StardewValley.Object
    {
        public Unhopper()
        {
            name = DisplayName = loadDisplayName();
            canBeSetDown.Value = true;
            canBeGrabbed.Value = true;
            bigCraftable.Value = true;
            edibility.Value = StardewValley.Object.inedible;
            type.Value = "Crafting";
            Category = -9;
            setOutdoors.Value = setIndoors.Value = true;
            fragility.Value = StardewValley.Object.fragility_Removable;

            heldObject.Value = new Chest();
        }

        public Unhopper( Vector2 tileLocation )
        :   this()
        {
            this.tileLocation.Value = tileLocation;
            boundingBox.Value = new Rectangle( ( int ) tileLocation.X * Game1.tileSize, ( int ) tileLocation.X * Game1.tileSize, Game1.tileSize, Game1.tileSize );
        }

        protected override string loadDisplayName()
        {
            return Mod.instance.Helper.Translation.Get( "item.unhopper.name" );
        }

        public override string getDescription()
        {
            return Mod.instance.Helper.Translation.Get( "item.unhopper.description" );
        }

        public override bool canStackWith( ISalable other )
        {
            return other is ConveyorBelt;
        }

        public override Item getOne()
        {
            var ret = new Unhopper();
            this._GetOneFrom( ret );
            return ret;
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool checkForAction( Farmer who, bool justCheckingForActivity = false )
        {
            var chest = heldObject.Value as Chest;
            if ( justCheckingForActivity )
                return true;

            if ( !Game1.didPlayerJustRightClick( ignoreNonMouseHeldInput: true ) )
            {
                return false;
            }
            chest.GetMutex().RequestLock( delegate
            {
                chest.ShowMenu();
            } );

            return true;
        }
        public override bool performToolAction( Tool t, GameLocation location )
        {
            if ( t != null && t.getLastFarmerToUse() != null && t.getLastFarmerToUse() != Game1.player )
                return false;
            if ( t == null || !t.isHeavyHitter() )
                return false;

            var chest = heldObject.Value as Chest;

            if ( base.performToolAction( t, location ) )
            {
                chest.GetMutex().RequestLock( () =>
                {
                    chest.clearNulls();
                    if ( chest.isEmpty() )
                    {
                        location.Objects.Remove( TileLocation );
                        location.debris.Add( new Debris( new Unhopper(), TileLocation * Game1.tileSize ) );
                    }
                    else
                    {
                        location.playSound( "hammer" );
                        shakeTimer = 100;
                    }
                    chest.GetMutex().ReleaseLock();
                } );
            }

            return false;
        }

        public override bool placementAction( GameLocation location, int x, int y, Farmer who = null )
        {
            Vector2 tile = new Vector2( x / Game1.tileSize, y / Game1.tileSize );

            location.Objects.Add( tile, new Unhopper( tile ) );
            location.playSound( "woodyStep" );

            return true;
        }

        public override void drawWhenHeld( SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f )
        {
            var texRect = new Rectangle( 0, 0, 16, 32 );
            spriteBatch.Draw( Assets.Unhopper, objectPosition, texRect, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, ( f.getStandingY() + 3 ) / 1000f );
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;

            var texRect = new Rectangle( 0, 0, 16, 32 );
            spriteBatch.Draw( Assets.Unhopper, location + new Vector2( 32f, 32f ), texRect, color * transparency, 0f, new Vector2( 8f, 16f ), 4f * ( ( ( double ) scaleSize < 0.2 ) ? scaleSize : ( scaleSize / 2f ) ), SpriteEffects.None, layerDepth );
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
            spriteBatch.Draw( Assets.Unhopper, destination, texRect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth );
        }

        public override void draw( SpriteBatch spriteBatch, int x, int y, float alpha = 1 )
        {
            var texRect = new Rectangle( 0, 0, 16, 32 );

            Vector2 scaleFactor = Vector2.Zero;//this.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
            Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
            spriteBatch.Draw( Assets.Unhopper, destination, texRect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer - 0.001f );
        }

        public override bool minutesElapsed( int minutes, GameLocation environment )
        {
            var myChest = heldObject.Value as Chest;
            myChest.GetMutex().RequestLock( () =>
            {
                int[] idBlacklist = new int[] { 231, 247, 165, 239, 238 };
                string[] nameBlacklist = new string[] { "Prairie King Arcade System", "Junimo Kart Arcade System", "Staircase", "Slime Ball", "Feed Hopper" };

                if ( environment.Objects.TryGetValue( TileLocation + new Vector2( 0, -1 ), out StardewValley.Object machine ) )
                {
                    if ( machine is Chest chest )
                    {
                        var items = chest.GetItemsForPlayer( chest.owner.Value );
                        for ( int i = 0; i < items.Count; ++i )
                        {
                            var item = items[ i ];
                            if ( item != null )
                            {
                                items[ i ] = ( heldObject.Value as Chest ).addItem( items[ i ] );
                                chest.clearNulls();
                                break;
                            }
                        }
                    }
                    if ( machine.bigCraftable.Value && machine.readyForHarvest /*&& machine.MinutesUntilReady == 0*/ && machine.heldObject.Value != null )
                    {
                        if ( machine.heldObject.Value is Chest heldChest )
                        {
                            var items = heldChest.GetItemsForPlayer( heldChest.owner.Value );
                            for ( int i = 0; i < items.Count; ++i )
                            {
                                var item = items[ i ];
                                if ( item != null )
                                {
                                    items[ i ] = ( heldObject.Value as Chest ).addItem( items[ i ] );
                                    heldChest.clearNulls();
                                    break;
                                }
                            }
                        }
                        else if ( !idBlacklist.Contains( machine.ParentSheetIndex ) && !nameBlacklist.Contains( machine.name ) && !machine.IsSprinkler() && !machine.IsScarecrow() )
                        {
                            Farmer fake = new Farmer();
                            fake.UniqueMultiplayerID = Game1.player.UniqueMultiplayerID;
                            fake.currentLocation = environment;
                            while ( fake.items.Count > 0 )
                                fake.items.RemoveAt( 0 );
                            for ( int i = 0; i < 36; ++i )
                                fake.items.Add( null );

                            var msgs = Game1.hudMessages;
                            Game1.hudMessages = new();
                            machine.checkForAction( fake, false );
                            Game1.hudMessages = msgs;

                            for ( int i = 0; i < fake.items.Count; ++i )
                            {
                                if ( fake.items[ i ] == null )
                                    continue;

                                fake.items[ i ] = ( heldObject.Value as Chest ).addItem( fake.items[ i ] );
                            }
                            for ( int i = 0; i < fake.items.Count; ++i )
                            {
                                if ( fake.items[ i ] == null )
                                    continue;

                                environment.debris.Add( new Debris( fake.items[ i ], ( TileLocation + new Vector2( 0.5f, 0.5f ) ) * Game1.tileSize ) );
                            }

                            /*
                            c.item.Value = machine.heldObject.Value;
                            machine.heldObject.Value = null;
                            */
                        }
                    }
                }

                ConveyorBelt c = null;
                if ( environment.get_BelowGroundObjects().TryGetValue( TileLocation, out StardewValley.Object beneath ) && beneath is ConveyorBelt cBeneath && cBeneath.item.Value == null )
                    c = cBeneath;
                else if ( environment.Objects.TryGetValue( TileLocation + new Vector2( -1, 0 ), out StardewValley.Object left ) && left is ConveyorBelt cLeft && cLeft.item.Value == null )
                    c = cLeft;
                else if ( environment.Objects.TryGetValue( TileLocation + new Vector2( 0, 1 ), out StardewValley.Object below ) && below is ConveyorBelt cBelow && cBelow.item.Value == null )
                    c = cBelow;
                else if ( environment.Objects.TryGetValue( TileLocation + new Vector2( 1, 0 ), out StardewValley.Object right ) && right is ConveyorBelt cRight && cRight.item.Value == null )
                    c = cRight;
                else if ( environment.get_ElevatedObjects().TryGetValue( TileLocation, out StardewValley.Object above ) && above is ConveyorBelt cAbove && cAbove.item.Value == null )
                    c = cAbove;

                if ( c == null )
                    return;

                var myItems = myChest.GetItemsForPlayer( myChest.owner.Value );
                for ( int i = 0; i < myItems.Count; ++i )
                {
                    if ( myItems[ i ] == null )
                        continue;

                    c.item.Value = myItems[ i ];
                    c.progress.Value = 0;
                    myItems[ i ] = null;
                    break;
                }
                myChest.clearNulls();
            } );

            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MisappliedPhysicalities.Game.Network;
using MisappliedPhysicalities.VirtualProperties;
using Netcode;
using StardewValley;

namespace MisappliedPhysicalities.Game.Objects
{
    [XmlType( "Mods_spacechase0_MisappliedPhysicalities_ConnectorBase" )]
    public abstract class ConnectorBase : StardewValley.Object 
    {
        public readonly NetEnum< Layer > onLayer = new();
        public readonly NetEnum<Side> connectionSide = new();
        public readonly NetList<ConnectorConnection, NetRef< ConnectorConnection > > connections = new();

        public ConnectorBase()
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
        }

        public ConnectorBase( Vector2 tileLocation, Layer layer, Side connectionSide )
        : this()
        {
            onLayer.Value = layer;
            this.tileLocation.Value = tileLocation;
            boundingBox.Value = new Rectangle( ( int ) tileLocation.X * Game1.tileSize, ( int ) tileLocation.X * Game1.tileSize, Game1.tileSize, Game1.tileSize );
            this.connectionSide.Value = connectionSide;
        }
        public abstract ConnectorType GetConnectorType();
        public abstract InOutType GetInOutType( GameLocation loc );
        public abstract Texture2D GetTexture();
        public abstract ConnectorBase MakeMe( Vector2 tile, Layer layer, Side connect );
        public ConnectorConnection GetAttachedPosition()
        {
            Layer layer = onLayer.Value;
            Vector3 offset = connectionSide.Value.GetOffset();
            if ( offset.Z != 0 )
            {
                if ( offset.Z == -1 )
                    layer = layer.GetBelow().Value;
                else if ( offset.Z == 1 )
                    layer = layer.GetAbove().Value;
            }
            Vector2 spot = TileLocation + new Vector2( offset.X, offset.Y );

            return new ConnectorConnection()
            {
                OtherLayer = layer,
                OtherPoint = new Point( ( int ) spot.X, ( int ) spot.Y )
            };
        }

        public StardewValley.Object GetAttachedMachine( GameLocation loc )
        {
            var spot = GetAttachedPosition();
            var container = loc.GetContainerForLayer( spot.OtherLayer );
            if ( !container.TryGetValue( new Vector2( spot.OtherPoint.X, spot.OtherPoint.Y ), out StardewValley.Object machine ) )
                return null;
            return machine;
        }

        public Vector2 GetLocalGraphicalWirePoint()
        {
            switch ( connectionSide.Value )
            {
                case Side.Up: return new Vector2( 8, 24 );
                case Side.Right: return new Vector2( 10, 18 );
                case Side.Down: return new Vector2( 8, 19 );
                case Side.Left: return new Vector2( 6, 19 );
                case Side.Above: return new Vector2( 7, 13 );
                case Side.Below: return new Vector2( 8, 20 );
            }

            return Vector2.Zero;
        }

        public void Connect( Layer layer, Point point )
        {
            ConnectorConnection them = new() { OtherLayer = layer, OtherPoint = point };
            foreach ( var conn in connections )
            {
                if ( conn == them )
                    return;
            }
            connections.Add( them );
        }

        public void Disconnect( Layer layer, Point point )
        {
            ConnectorConnection them = new() { OtherLayer = layer, OtherPoint = point };
            for ( int i = 0; i < connections.Count; ++i )
            {
                if ( connections[ i ].Equals( them ) )
                {
                    connections.RemoveAt( i );
                    break;
                }
            }
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool performToolAction( Tool t, GameLocation location )
        {
            if ( t != null && t.getLastFarmerToUse() != null && t.getLastFarmerToUse() != Game1.player )
                return false;
            if ( t == null || !t.isHeavyHitter() )
                return false;

            if ( connections.Count > 0 )
            {
                foreach ( ConnectorConnection conn in connections.ToList() )
                {
                    if ( location.GetContainerForLayer( conn.OtherLayer ).TryGetValue( new Vector2( conn.OtherPoint.X, conn.OtherPoint.Y ), out StardewValley.Object otherConnector ) )
                    {
                        if ( otherConnector is ConnectorBase otherConn )
                        {
                            otherConn.Disconnect( onLayer.Value, new Point( ( int ) tileLocation.X, ( int ) tileLocation.Y ) );
                        }
                    }
                    Disconnect( conn.OtherLayer, conn.OtherPoint );
                }
                return false;
            }
            else
            {
                location.GetContainerForLayer( onLayer.Value ).Remove( TileLocation );
                location.debris.Add( new Debris( MakeMe( default( Vector2 ), default( Layer ), default( Side ) ), new Vector2( tileLocation.X + 0.5f, tileLocation.Y + 0.5f ) * Game1.tileSize ) );
                return false;
            }
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

            Side connectTo = who?.FacingDirection.GetSideFromFacingDirection() ?? Side.Below;
            if ( Game1.isOneOfTheseKeysDown( Game1.GetKeyboardState(), Game1.options.runButton ) )
                connectTo = Side.Below;
            if ( Mod.Config.PlacementModifier.IsDown() )
                connectTo = Side.Above;

            var c = MakeMe( tile, layer, connectTo );

            target.Add( tile, c );
            location.playSound( "woodyStep" );

            return true;
        }

        public override void drawWhenHeld( SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f )
        {
            var texRect = new Rectangle( 80, 0, 16, 32 );
            spriteBatch.Draw( GetTexture(), objectPosition, texRect, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, ( f.getStandingY() + 3 ) / 1000f );
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;

            var texRect = new Rectangle( 80, 0, 16, 32 );
            spriteBatch.Draw( GetTexture(), location + new Vector2( 32f, 32f ), texRect, color * transparency, 0f, new Vector2( 8f, 16f ), 4f * ( ( ( double ) scaleSize < 0.2 ) ? scaleSize : ( scaleSize / 2f ) ), SpriteEffects.None, layerDepth );
            if ( shouldDrawStackNumber )
            {
                Utility.drawTinyDigits( this.Stack, spriteBatch, location + new Vector2( ( float ) ( 64 - Utility.getWidthOfTinyDigitString( this.Stack, 3f * scaleSize ) ) + 3f * scaleSize, 64f - 18f * scaleSize + 2f ), 3f * scaleSize, 1f, color );
            }
        }

        public override void draw( SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1 )
        {
            var texRect = new Rectangle( 80, 0, 16, 32 );

            Vector2 scaleFactor = this.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
            Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            spriteBatch.Draw( GetTexture(), destination, texRect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth );
        }

        public override void draw( SpriteBatch spriteBatch, int x, int y, float alpha = 1 )
        {
            var texRect = new Rectangle( ( ( int ) connectionSide.Value ) * 16, 0, 16, 32 );

            Vector2 scaleFactor = Vector2.Zero;//this.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
            Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
            spriteBatch.Draw( GetTexture(), destination, texRect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer - 0.001f );

            foreach ( var conn in connections )
            {
                //if ( ( int ) onLayer.Value > ( int ) conn.OtherLayer || onLayer.Value == conn.OtherLayer && y * 1000 + x < conn.OtherPoint.Y * 1000 + conn.OtherPoint.X )
                {
                    ConnectorBase them = ( Game1.currentLocation.GetContainerForLayer( conn.OtherLayer )[ new Vector2( conn.OtherPoint.X, conn.OtherPoint.Y ) ] as ConnectorBase);
                    if ( them == null )
                        continue;

                    Vector2 mySpot = position + GetLocalGraphicalWirePoint() * Game1.pixelZoom;
                    Vector2 theirPosition = Game1.GlobalToLocal(Game1.viewport, new Vector2(conn.OtherPoint.X * 64, conn.OtherPoint.Y * 64 - 64));
                    Vector2 theirSpot = theirPosition + them.GetLocalGraphicalWirePoint() * Game1.pixelZoom;

                    Vector2 diff = ( mySpot - theirSpot );
                    float len = diff.Length();
                    diff.Normalize();
                    float angle =  ( float )( Math.Atan2( diff.Y, diff.X ) + Math.PI );

                    if ( onLayer.Value != conn.OtherLayer && ( them.onLayer.Value == Layer.Underground || onLayer.Value == Layer.Underground ) )
                        len /= 2;

                    spriteBatch.Draw( Game1.staminaRect, new Rectangle( (int) mySpot.X, (int) mySpot.Y, ( int ) len, 4 ), null, GetConnectorType().GetConnectorWireColor(), angle, Vector2.Zero, SpriteEffects.None, 1 /* TODO */ );
                }
            }
        }
    }
}

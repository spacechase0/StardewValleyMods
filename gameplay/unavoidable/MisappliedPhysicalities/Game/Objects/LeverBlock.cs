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
    [XmlType( "Mods_spacechase0_MisappliedPhysicalities_LeverBlock" )]
    public class LeverBlock : StardewValley.Object, ILogicObject
    {
        public readonly NetBool on = new();

        public LeverBlock()
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

        public LeverBlock( Vector2 tileLocation )
        : this()
        {
            this.tileLocation.Value = tileLocation;
            boundingBox.Value = new Rectangle( ( int ) tileLocation.X * Game1.tileSize, ( int ) tileLocation.X * Game1.tileSize, Game1.tileSize, Game1.tileSize );
        }
        protected override string loadDisplayName()
        {
            return Mod.instance.Helper.Translation.Get( "item.lever-block.name" );
        }

        public override string getDescription()
        {
            return Mod.instance.Helper.Translation.Get( "item.lever-block.description" );
        }

        public override bool canStackWith( ISalable other )
        {
            return other is LeverBlock;
        }

        public override Item getOne()
        {
            var ret = new LeverBlock();
            this._GetOneFrom( ret );
            return ret;
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool checkForAction( Farmer who, bool justCheckingForActivity = false )
        {
            if ( justCheckingForActivity )
                return true;

            on.Value = !on.Value;
            who.currentLocation.playSound( "woodyStep" );

            return false;
        }

        public override bool performToolAction( Tool t, GameLocation location )
        {
            if ( t != null && t.getLastFarmerToUse() != null && t.getLastFarmerToUse() != Game1.player )
                return false;
            if ( t == null || !t.isHeavyHitter() )
                return false;

            location.netObjects.Remove( TileLocation );
            location.debris.Add( new Debris( new LeverBlock(), new Vector2( tileLocation.X + 0.5f, tileLocation.Y + 0.5f ) * Game1.tileSize ) );
            return false;
        }

        public override bool placementAction( GameLocation location, int x, int y, Farmer who = null )
        {
            Vector2 tile = new Vector2( x / Game1.tileSize, y / Game1.tileSize );

            Layer layer = Layer.GroundLevel;
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

            var c = new LeverBlock( tile );
            target.Add( tile, c );
            location.playSound( "woodyStep" );

            return true;
        }

        public override void drawWhenHeld( SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f )
        {
            var texRect = new Rectangle( on.Value ? 16 : 0, 0, 16, 32 );
            spriteBatch.Draw( Assets.LeverBlock, objectPosition, texRect, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, ( f.getStandingY() + 3 ) / 1000f );
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && this.maximumStackSize() > 1 && this.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && this.Stack != int.MaxValue;

            var texRect = new Rectangle( on.Value ? 16 : 0, 0, 16, 32 );
            spriteBatch.Draw( Assets.LeverBlock, location + new Vector2( 32f, 32f ), texRect, color * transparency, 0f, new Vector2( 8f, 16f ), 4f * ( ( ( double ) scaleSize < 0.2 ) ? scaleSize : ( scaleSize / 2f ) ), SpriteEffects.None, layerDepth );
            if ( shouldDrawStackNumber )
            {
                Utility.drawTinyDigits( this.Stack, spriteBatch, location + new Vector2( ( float ) ( 64 - Utility.getWidthOfTinyDigitString( this.Stack, 3f * scaleSize ) ) + 3f * scaleSize, 64f - 18f * scaleSize + 2f ), 3f * scaleSize, 1f, color );
            }
        }

        public override void draw( SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1 )
        {
            var texRect = new Rectangle( on.Value ? 16 : 0, 0, 16, 32 );

            Vector2 scaleFactor = this.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
            Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            spriteBatch.Draw( Assets.LeverBlock, destination, texRect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth );
        }

        public override void draw( SpriteBatch spriteBatch, int x, int y, float alpha = 1 )
        {
            var texRect = new Rectangle( on.Value ? 16 : 0, 0, 16, 32 );

            Vector2 scaleFactor = Vector2.Zero;//this.getScale();
            scaleFactor *= 4f;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
            Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
            float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
            spriteBatch.Draw( Assets.LeverBlock, destination, texRect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer - 0.001f );
        }

        public InOutType GetLogicTypeForSide( Side side )
        {
            return InOutType.Output;
        }

        public double GetLogicFrom( Side side )
        {
            return on.Value ? 1 : 0;
        }

        public void SendLogicTo( Side side, double signal )
        {
            throw new NotImplementedException();
        }
    }
}

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

namespace MisappliedPhysicalities.Game.Objects
{
    [XmlType( "Mods_spacechase0_MisappliedPhysicalities_WireCutter" )]
    public class WireCutter : Tool
    {
        private readonly NetString cutStartLocation = new();
        private readonly NetEnum<Layer> cutStartLayer = new();
        private readonly NetPoint cutStartPoint = new();

        public WireCutter()
        {
            this.Name = "Wire Cutter";
            this.InstantUse = true;
            numAttachmentSlots.Value = 1;
            attachments.SetCount( 1 );
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields( cutStartLocation, cutStartLayer, cutStartPoint );
        }

        protected override string loadDisplayName()
        {
            return Mod.instance.Helper.Translation.Get( "tool.wire-cutter.name" );
        }

        protected override string loadDescription()
        {
            return Mod.instance.Helper.Translation.Get( "tool.wire-cutter.description" );
        }

        public override void DoFunction( GameLocation location, int x, int y, int power, Farmer who )
        {
            who.CanMove = true;
            who.UsingTool = false;
            who.canReleaseTool = true;

            if ( attachments[ 0 ] == null )
            {
                Game1.addHUDMessage( new HUDMessage( Mod.instance.Helper.Translation.Get( "message.wire-cutter.need-wire" ) ) );
                return;
            }

            ConnectorType wire = attachments[ 0 ].ParentSheetIndex.GetConnectorTypeFromWireItemId().Value;

            Layer layer = Layer.GroundLevel;
            if ( Mod.dga.GetDGAItemId( who.hat.Value ) == Items.XrayGogglesId )
            {
                layer = Layer.Underground;
            }

            var objs = location.GetContainerForLayer( layer );
            //Vector2 spot = new Vector2( x / Game1.tileSize, y / Game1.tileSize );
            // Tools are so weird! Don't use the vanilla way of getting the coordinates
            Vector2 spot = Mod.instance.Helper.Input.GetCursorPosition().Tile;

            if ( !objs.ContainsKey( spot ) || objs[ spot ] is not ConnectorBase conn )
            {
                Game1.addHUDMessage( new HUDMessage( "meow1" ) );
                return;
            }
            if ( conn.GetConnectorType() != wire )
            {
                Game1.addHUDMessage( new HUDMessage( Mod.instance.Helper.Translation.Get( "message.wire-cutter.wrong-wire" ) ) );
                return;
            }

            var otherObjs = location.GetContainerForLayer( cutStartLayer.Value );
            var otherSpot = new Vector2( cutStartPoint.Value.X, cutStartPoint.Value.Y );
            if ( string.IsNullOrEmpty( cutStartLocation.Value ) ||
                 cutStartLocation.Value != location.NameOrUniqueName ||
                 !otherObjs.ContainsKey( otherSpot ) ||
                 otherObjs[ otherSpot ] is not ConnectorBase otherConn ||
                 otherConn.GetConnectorType() != wire )
            {
                cutStartLocation.Value = location.NameOrUniqueName;
                cutStartLayer.Value = layer;
                cutStartPoint.Value = new Point( ( int ) spot.X, ( int ) spot.Y );
                Game1.addHUDMessage( new HUDMessage( "message.wire-cutter.prep" ) );
            }
            else
            {
                conn.Connect( cutStartLayer.Value, cutStartPoint.Value );
                otherConn.Connect( layer, new Point( ( int ) spot.X, ( int ) spot.Y ) );
                attachments[ 0 ].Stack--;
                if ( attachments[ 0 ].Stack <= 0 )
                    attachments[ 0 ] = null;
                cutStartLocation.Value = "";
                Game1.addHUDMessage( new HUDMessage( "message.wire-cutter.placed" ) );
            }
            // TODO: Play sound
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            spriteBatch.Draw( Assets.WireCutter, location, null, Color.White * transparency, 0, Vector2.Zero, scaleSize * 4, SpriteEffects.None, layerDepth );
            // TODO: Draw a wire for when something is actively being used
        }

        public override Item getOne()
        {
            return new WireCutter();
        }

        public override bool canThisBeAttached( StardewValley.Object o )
        {
            if ( o == null )
                return true;
            if ( o.ParentSheetIndex.GetConnectorTypeFromWireItemId().HasValue )
                return true;

            return false;
        }

        public override StardewValley.Object attach( StardewValley.Object o )
        {
            if ( o == null )
            {
                var tmp = attachments[ 0 ];
                attachments[ 0 ] = null;
                return attachments[ 0 ];
            }

            if ( attachments[ 0 ] == null )
            {
                attachments[ 0 ] = o;
                return null;
            }

            if ( attachments[ 0 ].canStackWith( o ) )
            {
                o.Stack = attachments[ 0 ].addToStack( o );
                if ( o.Stack <= 0 )
                    o = null;
                return o;
            }
            else
            {
                var swap = attachments[ 0 ];
                attachments[ 0 ] = o;
                return swap;
            }
        }

        public override void drawAttachments( SpriteBatch b, int x, int y )
        {
            b.Draw( Game1.menuTexture, new Vector2( x, y ), Game1.getSourceRectForStandardTileSheet( Game1.menuTexture, 10 ), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f );
            if ( base.attachments[ 0 ] != null )
            {
                base.attachments[ 0 ].drawInMenu( b, new Vector2( x, y ), 1f );
            }
        }
    }
}

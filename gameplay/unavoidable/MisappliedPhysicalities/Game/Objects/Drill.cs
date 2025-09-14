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

namespace MisappliedPhysicalities.Game.Objects
{
    [XmlType( "Mods_spacechase0_MisappliedPhysicalities_Drill" )]
    public class Drill : Tool
    {
        public Drill()
        {
            this.Name = "Drill";
            this.InstantUse = true;
        }

        protected override string loadDisplayName()
        {
            return Mod.instance.Helper.Translation.Get( "tool.drill.name" );
        }

        protected override string loadDescription()
        {
            return Mod.instance.Helper.Translation.Get( "tool.drill.description" );
        }

        public override void DoFunction( GameLocation location, int x, int y, int power, Farmer who )
        {
            who.CanMove = true;
            who.UsingTool = false;
            who.canReleaseTool = true;

            if ( Mod.dga.GetDGAItemId( who.hat.Value ) != Items.XrayGogglesId )
            {
                Game1.addHUDMessage( new HUDMessage( Mod.instance.Helper.Translation.Get( "message.drill.need-goggles" ) ) );
                return;
            }


            var below = location.get_BelowGroundObjects();
            //Vector2 spot = new Vector2( x / Game1.tileSize, y / Game1.tileSize );
            // Tools are so weird! Don't use the vanilla way of getting the coordinates
            Vector2 spot = Mod.instance.Helper.Input.GetCursorPosition().Tile;

            if ( !below.ContainsKey( spot ) )
            {
                // TODO: play drill sound effect
                below.Add( spot, new NullObject() );
            }
            else if ( below[ spot ] != null )
            {
                var obj = below[ spot ];
                if ( obj is not NullObject )
                {
                    // todo - remove
                }
            }
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            spriteBatch.Draw( Assets.Drill, location, null, Color.White * transparency, 0, Vector2.Zero, scaleSize * 4, SpriteEffects.None, layerDepth );
        }

        public override Item getOne()
        {
            return new Drill();
        }
    }
}

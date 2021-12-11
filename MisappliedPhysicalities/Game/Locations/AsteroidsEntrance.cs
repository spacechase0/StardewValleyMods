using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using xTile.Dimensions;

namespace MisappliedPhysicalities.Game.Locations
{
    [XmlType( "Mods_spacechase0_MisappliedPhysicalities_AsteroidsEntrance" )]
    public class AsteroidsEntrance : LunarLocation
    {
        public AsteroidsEntrance() { }
        public AsteroidsEntrance( IContentHelper content )
        :   base( content, "MoonAsteroidsEntrance", "Custom_MP_MoonAsteroidsEntrance" )
        {
        }

        public override bool performAction( string action, Farmer who, Location tileLocation )
        {
            if ( action == "GargoyleEntrance" )
            {
                createQuestionDialogue( Mod.instance.Helper.Translation.Get( "message.gargoyle" ), createYesNoResponses(), "GargoyleEntrance" );
            }
            else if ( action == "DirtTutorial" )
            {
                Game1.drawObjectDialogue( Mod.instance.Helper.Translation.Get( "message.dirt-tutorial" ) );
            }
            return base.performAction( action, who, tileLocation );
        }

        public override bool answerDialogue( Response answer )
        {
            if ( lastQuestionKey != null && afterQuestion == null )
            {
                string qa = lastQuestionKey.Split( ' ' )[ 0 ] + "_" + answer.responseKey;
                switch ( qa )
                {
                    case "GargoyleEntrance_Yes":
                        performTouchAction( "MagicWarp FarmHouse 5 7"/*"MagicWarp Custom_MP_AsteroidsDungeon0 50 50"*/, Game1.player.getTileLocation() );
                        return true;
                }
            }

            return base.answerDialogue( answer );
        }
    }
}

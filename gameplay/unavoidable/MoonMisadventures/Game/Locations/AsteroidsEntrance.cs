using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;

namespace MoonMisadventures.Game.Locations
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_AsteroidsEntrance" )]
    public class AsteroidsEntrance : LunarLocation
    {
        public AsteroidsEntrance() { }
        public AsteroidsEntrance( IModContentHelper content )
        :   base( content, "MoonAsteroidsEntrance", "MoonAsteroidsEntrance" )
        {
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();

            if ( Game1.player.Tile.Y == 1 )
            {
                Game1.player.Position = new Vector2( 25.5f * Game1.tileSize, 19 * Game1.tileSize );
            }
        }

        public override bool performAction( string action, Farmer who, Location tileLocation )
        {
            if ( action == "GargoyleEntrance" )
            {
                createQuestionDialogue(I18n.Message_Gargoyle(), createYesNoResponses(), "GargoyleEntrance" );
            }
            else if ( action == "DirtTutorial" )
            {
                Game1.drawObjectDialogue(I18n.Message_DirtTutorial());
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
                        performTouchAction( "MagicWarp " + AsteroidsDungeon.BaseLocationName + "1 0 0", Game1.player.Tile );
                        return true;
                }
            }

            return base.answerDialogue( answer );
        }
    }
}

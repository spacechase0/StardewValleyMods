using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using MoonMisadventures.VirtualProperties;
using StardewModdingAPI;
using StardewValley;

namespace MoonMisadventures.Game.Locations
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_MoonLandingArea" )]
    public class MoonLandingArea : LunarLocation
    {
        private readonly string ufoTexPath;

        public MoonLandingArea() { }
        public MoonLandingArea( IModContentHelper content )
        :   base( content, "MoonLandingArea", "MoonLandingArea" )
        {
            ufoTexPath = content.GetInternalAssetName("assets/ufo-big.png").BaseName;
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();

            TemporarySprites.Clear();

            base.TemporarySprites.Add( new TemporaryAnimatedSprite( ufoTexPath, new Rectangle( 0, 48, 96, 48 ), 100, Mod.instance.Config.FlashingUfo ? 4 : 1, 99999, new Vector2( ( int )( 6.5f * Game1.tileSize ), 27 * Game1.tileSize ), false, false )
            {
                scale = 4f,
            } );
            base.TemporarySprites.Add( new TemporaryAnimatedSprite( ufoTexPath, new Rectangle( 96, 0, 96, 48 ), 99999, 1, 99999, new Vector2( ( int ) ( 6.5f * Game1.tileSize ), 27 * Game1.tileSize ), false, false )
            {
                scale = 4f,
            } );

            Game1.background = null;
        }
        public override bool isActionableTile( int xTile, int yTile, Farmer who )
        {
            if ( xTile >= 7 && xTile <= 11 && yTile >= 28 && yTile <= 29 )
                return true;
            return base.isActionableTile( xTile, yTile, who );
        }

        public override bool checkAction( xTile.Dimensions.Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who )
        {
            int xTile = tileLocation.X, yTile = tileLocation.Y;
            if ( xTile >= 7 && xTile <= 11 && yTile >= 28 && yTile <= 29 )
            {
                createQuestionDialogue(I18n.Message_Ufo_Travel(), createYesNoResponses(), "TravelUfo" );
                return true;
            }

            return base.checkAction( tileLocation, viewport, who );
        }

        public override bool performAction( string action, Farmer who, xTile.Dimensions.Location tileLocation )
        {
            if ( action == "LunarTempleDoor" )
            {
                if ( Game1.player.team.get_hasLunarKey() )
                {
                    Game1.warpFarmer("Custom_MM_MoonInfuserRoom", 15, 24, Game1.up);
                }
                else
                {
                    Game1.drawObjectDialogue(I18n.Message_LunarTemple_Locked());
                }
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
                    case "TravelUfo_Yes":
                        Game1.warpFarmer( "Custom_MM_MountainTop", 22, 23, 0 );
                        return true;
                }
            }

            return base.answerDialogue( answer );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace MoonMisadventures.Game.Locations
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_MoonLandingArea" )]
    public class MoonLandingArea : LunarLocation
    {
        private readonly string ufoTexPath;

        public MoonLandingArea() { }
        public MoonLandingArea( IContentHelper content )
        :   base( content, "MoonLandingArea", "MoonLandingArea" )
        {
            ufoTexPath = content.GetActualAssetKey( "assets/ufo-big.png" );
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();

            TemporarySprites.Clear();

            base.TemporarySprites.Add( new TemporaryAnimatedSprite( ufoTexPath, new Rectangle( 0, 48, 96, 48 ), 100, 4, 99999, new Vector2( ( int )( 6.5f * Game1.tileSize ), 27 * Game1.tileSize ), false, false )
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
                createQuestionDialogue( Mod.instance.Helper.Translation.Get( "message.ufo.travel" ), createYesNoResponses(), "TravelUfo" );
                return true;
            }

            return base.checkAction( tileLocation, viewport, who );
        }
        public override bool answerDialogue( Response answer )
        {
            if ( lastQuestionKey != null && afterQuestion == null )
            {
                string qa = lastQuestionKey.Split( ' ' )[ 0 ] + "_" + answer.responseKey;
                switch ( qa )
                {
                    case "TravelUfo_Yes":
                        Game1.warpFarmer( "Custom_MP_MountainTop", 22, 23, 0 );
                        return true;
                }
            }

            return base.answerDialogue( answer );
        }
    }
}

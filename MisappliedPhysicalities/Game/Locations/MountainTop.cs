using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using xTile;

namespace MisappliedPhysicalities.Game.Locations
{
    [XmlType("Mods_spacechase0_MisappliedPhysicalities_MountainTop")]
    public class MountainTop : GameLocation
    {
        private readonly string ufoTexPath;

        public readonly NetBool ufoRepaired = new NetBool( false );

        public MountainTop() { }
        public MountainTop( IContentHelper content )
        : base( content.GetActualAssetKey( "assets/InactiveCaldera.tmx" ), "Custom_MP_MountainTop" )
        {
            ufoTexPath = content.GetActualAssetKey( "assets/ufo-big.png" );
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            base.NetFields.AddFields( ufoRepaired );
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();

            Game1.background = new Background();

            TemporarySprites.Clear();

            base.TemporarySprites.Add( new TemporaryAnimatedSprite( ufoTexPath, new Rectangle( 0, 0, 96, 48 ), 99999, 1, 99999, new Vector2( 20 * Game1.tileSize, 20 * Game1.tileSize ), false, false )
            {
                scale = 4f,
            } );
        }

        public override void cleanupBeforePlayerExit()
        {
            base.cleanupBeforePlayerExit();
            Game1.background = null;
        }

        public override bool isActionableTile( int xTile, int yTile, Farmer who )
        {
            if ( yTile == 22 && xTile >= 20 && xTile <= 25 )
                return true;
            return base.isActionableTile( xTile, yTile, who );
        }

        public override bool checkAction( xTile.Dimensions.Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who )
        {
            int xTile = tileLocation.X, yTile = tileLocation.Y;
            if ( yTile == 22 && xTile >= 20 && xTile <= 25 )
            {
                if ( ufoRepaired.Value )
                {
                    createQuestionDialogue( Mod.instance.Helper.Translation.Get( "message.ufo.travel" ), createYesNoResponses(), "TravelUfo" );
                }
                else
                {
                    createQuestionDialogue( Mod.instance.Helper.Translation.Get( "message.ufo.repair" ), createYesNoResponses(), "RepairUfo" );
                }
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
                    case "RepairUfo_Yes":
                        if ( Game1.player.hasItemInInventory( 910 /* radioactive bar */, 25 ) &&
                             Game1.player.hasItemInInventory( 896 /* galaxy soul */, 1 ) )
                        {
                            var mp = Mod.instance.Helper.Reflection.GetField< Multiplayer >( typeof( Game1 ), "multiplayer" ).GetValue();
                            mp.globalChatInfoMessage( "RepairedUfo", Game1.player.Name );
                            Game1.player.removeItemsFromInventory( 910, 25 );
                            Game1.player.removeItemsFromInventory( 896, 1 );
                            Game1.playSound( "questcomplete" );
                            ufoRepaired.Value = true;
                            TemporarySprites.Clear();
                            base.TemporarySprites.Add( new TemporaryAnimatedSprite( ufoTexPath, new Rectangle( 0, 48, 96, 48 ), 100, 4, 99999, new Vector2( 20 * Game1.tileSize, 20 * Game1.tileSize ), false, false )
                            {
                                scale = 4f,
                            } );
                            base.TemporarySprites.Add( new TemporaryAnimatedSprite( ufoTexPath, new Rectangle( 96, 0, 96, 48 ), 99999, 1, 99999, new Vector2( 20 * Game1.tileSize, 20 * Game1.tileSize ), false, false )
                            {
                                scale = 4f,
                            } );
                            // TODO add some sparkles
                        }
                        else
                        {
                            Game1.drawObjectDialogue( Mod.instance.Helper.Translation.Get( "message.ufo.repair.lacking" ) );
                        }
                        return true;
                    case "TravelUfo_Yes":
                        // TODO: Show the dwarf and you getting on the UFO, like the vanilla boat
                        if ( !Game1.player.hasOrWillReceiveMail( "firstUfoTravel" ) )
                        {
                            Game1.addMailForTomorrow( "firstUfoTravel" );
                            Game1.currentMinigame = new LaunchJourney();
                        }
                        else
                        {
                            Game1.warpFarmer( "Custom_MP_MoonLandingArea", 9, 31, 0 );
                        }
                        return true;
                }
            }

            return base.answerDialogue( answer );
        }
    }
}

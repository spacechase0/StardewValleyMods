using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace PreexistingRelationship
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry( StardewModdingAPI.IModHelper helper )
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Multiplayer.ModMessageReceived += OnMessageReceived;

            helper.ConsoleCommands.Add( "marry", "...", OnCommand );
        }

        private void OnUpdateTicked( object sender, UpdateTickedEventArgs e )
        {
            if ( Context.IsPlayerFree && Game1.activeClickableMenu == null &&
                 !Game1.player.hasOrWillReceiveMail( $"{ModManifest.UniqueID}/FreeMarriage" ) &&
                 Game1.player.getSpouse() == null && Game1.player.isCustomized.Value )
            {
                Game1.activeClickableMenu = new MarryMenu();
                Game1.player.mailReceived.Add( $"{ModManifest.UniqueID}/FreeMarriage" );
            }
        }

        private void OnMessageReceived( object sender, ModMessageReceivedEventArgs e )
        {
            if ( e.Type != nameof( DoMarriageMessage ) || e.FromPlayerID == Game1.player.UniqueMultiplayerID )
                return;

            var msg = e.ReadAs< DoMarriageMessage >();
            var player = Game1.getFarmer( e.FromPlayerID );
            DoMarriage( player, msg.NpcName, false );
        }

        private void OnCommand( string arg1, string[] arg2 )
        {
            if ( !Context.IsPlayerFree )
                return;

            if ( Game1.player.getSpouse() != null )
            {
                Log.error( "You are already married." );
                return;
            }

            Game1.activeClickableMenu = new MarryMenu();
        }

        internal static void DoMarriage( Farmer player, string npcName, bool local )
        {
            Log.debug( player.Name + " selected " + npcName + " (" + local + ")" );
            foreach ( var farmer in Game1.getAllFarmers() )
            {
                if ( farmer.spouse == npcName )
                {
                    return;
                }
            }

            // Prepare house
            if ( local )
            {
                Utility.getHomeOfFarmer( player ).moveObjectsForHouseUpgrade( 1 );
                Utility.getHomeOfFarmer( player ).setMapForUpgradeLevel( 1 );
            }
            player.HouseUpgradeLevel = 1;
            Game1.removeFrontLayerForFarmBuildings();
            Game1.addNewFarmBuildingMaps();
            Utility.getHomeOfFarmer( player ).RefreshFloorObjectNeighbors();

            // Do spouse stuff
            if ( local )
            {
                if ( !player.friendshipData.ContainsKey( npcName ) )
                    player.friendshipData.Add( npcName, new Friendship() );
                player.friendshipData[ npcName ].Points = 2500;
                player.spouse = npcName;
                player.friendshipData[ npcName ].WeddingDate = new WorldDate( Game1.Date );
                player.friendshipData[ npcName ].Status = FriendshipStatus.Married;
            }

            NPC spouse = Game1.getCharacterFromName(npcName);
            spouse.Schedule = null;
            spouse.DefaultMap = player.homeLocation.Value;
            spouse.DefaultPosition = Utility.PointToVector2( ( Game1.getLocationFromName( player.homeLocation.Value ) as FarmHouse ).getSpouseBedSpot( player.spouse ) ) * Game1.tileSize;
            spouse.DefaultFacingDirection = 2;

            spouse.Schedule = null;
            spouse.ignoreScheduleToday = true;
            spouse.shouldPlaySpousePatioAnimation.Value = false;
            spouse.controller = null;
            spouse.temporaryController = null;
            if ( local )
                spouse.Dialogue.Clear();
            spouse.currentMarriageDialogue.Clear();
            Game1.warpCharacter( spouse, "Farm", Utility.getHomeOfFarmer( player ).getPorchStandingSpot() );
            spouse.faceDirection( 2 );
            if ( local )
            {
                if ( Game1.content.LoadStringReturnNullIfNotFound( "Strings\\StringsFromCSFiles:" + spouse.Name + "_AfterWedding" ) != null )
                {
                    spouse.addMarriageDialogue( "Strings\\StringsFromCSFiles", spouse.Name + "_AfterWedding", false );
                }
                else
                {
                    spouse.addMarriageDialogue( "Strings\\StringsFromCSFiles", "Game1.cs.2782", false );
                }

                Game1.addHUDMessage( new HUDMessage( Mod.instance.Helper.Translation.Get( "married" ) ) );
            }
        }
    }
}

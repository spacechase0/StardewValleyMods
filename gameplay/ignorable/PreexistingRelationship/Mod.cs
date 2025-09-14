using PreexistingRelationship.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace PreexistingRelationship
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Multiplayer.ModMessageReceived += this.OnMessageReceived;

            helper.ConsoleCommands.Add("marry", "...", this.OnCommand);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Context.IsPlayerFree && Game1.activeClickableMenu == null &&
                 !Game1.player.hasOrWillReceiveMail($"{this.ModManifest.UniqueID}/FreeMarriage") &&
                 Game1.player.getSpouse() == null && Game1.player.isCustomized.Value)
            {
                Game1.activeClickableMenu = new MarryMenu();
                Game1.player.mailReceived.Add($"{this.ModManifest.UniqueID}/FreeMarriage");
            }
        }

        private void OnMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.Type != nameof(DoMarriageMessage) || e.FromPlayerID == Game1.player.UniqueMultiplayerID)
                return;

            var msg = e.ReadAs<DoMarriageMessage>();
            var player = Game1.getFarmer(e.FromPlayerID);
            Mod.DoMarriage(player, msg.NpcName, false);
        }

        private void OnCommand(string arg1, string[] arg2)
        {
            if (!Context.IsPlayerFree)
                return;

            if (Game1.player.getSpouse() != null)
            {
                Log.Error("You are already married.");
                return;
            }

            Game1.activeClickableMenu = new MarryMenu();
        }

        internal static void DoMarriage(Farmer player, string npcName, bool local)
        {
            Log.Debug(player.Name + " selected " + npcName + " (" + local + ")");
            foreach (var farmer in Game1.getAllFarmers())
            {
                if (farmer.spouse == npcName)
                {
                    return;
                }
            }

            // Prepare house
            if (local)
            {
                Utility.getHomeOfFarmer(player).moveObjectsForHouseUpgrade(1);
                Utility.getHomeOfFarmer(player).setMapForUpgradeLevel(1);
            }
            player.HouseUpgradeLevel = 1;
            Game1.removeFrontLayerForFarmBuildings();
            Game1.addNewFarmBuildingMaps();
            Utility.getHomeOfFarmer(player).RefreshFloorObjectNeighbors();

            // Do spouse stuff
            if (local)
            {
                if (!player.friendshipData.TryGetValue(npcName, out Friendship friendship))
                {
                    friendship = new Friendship();
                    player.friendshipData.Add(npcName, friendship);
                }

                friendship.Points = 2500;
                player.spouse = npcName;
                friendship.WeddingDate = new WorldDate(Game1.Date);
                friendship.Status = FriendshipStatus.Married;
            }

            NPC spouse = Game1.getCharacterFromName(npcName);
            spouse.Schedule = null;
            spouse.DefaultMap = player.homeLocation.Value;
            spouse.DefaultPosition = Utility.PointToVector2((Game1.getLocationFromName(player.homeLocation.Value) as FarmHouse).getSpouseBedSpot(player.spouse)) * Game1.tileSize;
            spouse.DefaultFacingDirection = 2;

            spouse.Schedule = null;
            spouse.ignoreScheduleToday = true;
            spouse.shouldPlaySpousePatioAnimation.Value = false;
            spouse.controller = null;
            spouse.temporaryController = null;
            if (local)
                spouse.Dialogue.Clear();
            spouse.currentMarriageDialogue.Clear();
            Game1.warpCharacter(spouse, "Farm", Utility.getHomeOfFarmer(player).getPorchStandingSpot());
            spouse.faceDirection(2);
            if (local)
            {
                if (Game1.content.LoadStringReturnNullIfNotFound("Strings\\StringsFromCSFiles:" + spouse.Name + "_AfterWedding") != null)
                {
                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", spouse.Name + "_AfterWedding");
                }
                else
                {
                    spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "Game1.cs.2782");
                }

                Game1.addHUDMessage(new HUDMessage(I18n.Married()));
            }
        }
    }
}

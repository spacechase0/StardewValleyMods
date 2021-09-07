using System;
using System.IO;
using System.Linq;
using SpaceCore.Events;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI.Events;
using StardewValley;

namespace PicturePortraits
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(StardewModdingAPI.IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            this.Helper.Events.GameLoop.GameLaunched += this.OnDayStarted;
            SpaceEvents.OnEventFinished += this.OnEventFinished;

            this.Helper.ConsoleCommands.Add("camera_add", "Add a camera to your inventory.", (cmd, args) => Game1.player.addItemByMenuIfNecessary(new CameraTool()));
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var spacecore = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            spacecore.RegisterSerializerType(typeof(CameraTool));

            var dga = this.Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
            dga.AddEmbeddedPack(this.ModManifest, Path.Combine(this.Helper.DirectoryPath, "assets", "dga"));
        }

        private void OnDayStarted(object sender, GameLaunchedEventArgs e)
        {
            if (!Game1.IsMasterGame)
                return;

            int foundCamera = 0;
            SpaceUtility.iterateAllItems((item) => { if (item is CameraTool) ++foundCamera; return item; });

            for (int i = foundCamera; i < Game1.getAllFarmers().Count((f) => f.eventsSeen.Contains(14)); ++i)
            {
                Game1.player.team.returnedDonationsMutex.RequestLock(() => Game1.player.team.returnedDonations.Add(new CameraTool()));
            }
        }
        private void OnEventFinished(object sender, EventArgs e)
        {
            if (Game1.CurrentEvent.id == 14)
                Game1.player.addItemByMenuIfNecessary(new CameraTool());
        }
    }
}

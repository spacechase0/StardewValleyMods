using System;
using System.IO;
using SpaceShared;
using SpaceShared.APIs;
using StardewValley;

namespace DeepSeaFishing
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(StardewModdingAPI.IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            Helper.Events.Specialized.LoadStageChanged += this.Specialized_LoadStageChanged;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType(typeof(DeepSeaLocation));

            var ja = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            ja.LoadAssets( Path.Combine( Helper.DirectoryPath, "assets" ), Helper.Translation );
        }
        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            // TrainStation requires this to be in save loaded or later
            var ts = Helper.ModRegistry.GetApi<ITrainStationApi>("Cherry.TrainStation");
            ts.RegisterBoatStation("DeepSeaFishing", "Custom_DeepSeaFishing_DeepSea", new(), 27, 17, 3000, Game1.down, null, I18n.DeepSeaDock());
        }

        private void Specialized_LoadStageChanged(object sender, StardewModdingAPI.Events.LoadStageChangedEventArgs e)
        {
            if (e.NewStage == StardewModdingAPI.Enums.LoadStage.CreatedInitialLocations || e.NewStage == StardewModdingAPI.Enums.LoadStage.SaveAddedLocations)
            {
                Game1.locations.Add(new DeepSeaLocation());
            }
        }
    }
}

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.IO;
using Magic.Other;
using Newtonsoft.Json;

namespace Magic
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static MultiplayerSaveData Data { get; private set; } = new MultiplayerSaveData();
        public static Configuration Config { get; private set; }
        
        internal static JsonAssetsApi ja;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            Config = Helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += onGameLaunched;
            helper.Events.GameLoop.SaveLoaded += onSaveLoaded;
            helper.Events.GameLoop.Saved += onSaved;

            Magic.init(helper.Events, helper.Input, helper.Multiplayer.GetNewID);
        }

        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var api = Helper.ModRegistry.GetApi<JsonAssetsApi>("spacechase0.JsonAssets");
            if (api == null)
            {
                Log.error("No Json Assets API???");
                return;
            }
            ja = api;

            api.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets"));
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                if (!Game1.IsMultiplayer || Game1.IsMasterGame)
                {
                    Log.info($"Loading save data (\"{MultiplayerSaveData.FilePath}\")...");
                    Data = File.Exists(MultiplayerSaveData.FilePath)
                        ? JsonConvert.DeserializeObject<MultiplayerSaveData>(File.ReadAllText(MultiplayerSaveData.FilePath))
                        : new MultiplayerSaveData();
                    
                    if ( !Data.players.ContainsKey( Game1.player.UniqueMultiplayerID ) )
                        Data.players[Game1.player.UniqueMultiplayerID] = new MultiplayerSaveData.PlayerData();
                }
            }
            catch ( Exception ex )
            {
                Log.warn($"Exception loading save data: {ex}");
            }
        }

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaved(object sender, SavedEventArgs e)
        {
            if (!Game1.IsMultiplayer || Game1.IsMasterGame)
            {
                Log.info($"Saving save data (\"{MultiplayerSaveData.FilePath}\")...");
                File.WriteAllText(MultiplayerSaveData.FilePath, JsonConvert.SerializeObject(Data));
            }
        }
    }
}

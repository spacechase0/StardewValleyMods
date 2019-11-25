using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.IO;
using Magic.Other;
using Newtonsoft.Json;
using SpaceShared;
using SpaceShared.APIs;

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
            Log.Monitor = Monitor;

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
            var capi = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(ModManifest, () => Config = new Configuration(), () => Helper.WriteConfig(Config));
                capi.RegisterSimpleOption(ModManifest, "Altar Location", "The (internal) name of the location the magic altar should be placed at.", () => Config.AltarLocation, (string val) => Config.AltarLocation = val);
                capi.RegisterSimpleOption(ModManifest, "Altar X", "The X tile position of where the magic altar should be placed.", () => Config.AltarX, (int val) => Config.AltarX = val);
                capi.RegisterSimpleOption(ModManifest, "Altar Y", "The Y tile position of where the magic altar should be placed.", () => Config.AltarY, (int val) => Config.AltarY = val);
                capi.RegisterSimpleOption(ModManifest, "Key: Cast", "The key to initiate casting a spell.", () => Config.Key_Cast, (SButton val) => Config.Key_Cast = val);
                capi.RegisterSimpleOption(ModManifest, "Key: Swap Spells", "The key to swap spell sets.", () => Config.Key_SwapSpells, (SButton val) => Config.Key_SwapSpells = val);
                capi.RegisterSimpleOption(ModManifest, "Key: Spell 1", "The key for spell 1.", () => Config.Key_Spell1, (SButton val) => Config.Key_Spell1 = val);
                capi.RegisterSimpleOption(ModManifest, "Key: Spell 2", "The key for spell 2.", () => Config.Key_Spell2, (SButton val) => Config.Key_Spell2 = val);
                capi.RegisterSimpleOption(ModManifest, "Key: Spell 3", "The key for spell 3.", () => Config.Key_Spell3, (SButton val) => Config.Key_Spell3 = val);
                capi.RegisterSimpleOption(ModManifest, "Key: Spell 4", "The key for spell 4.", () => Config.Key_Spell4, (SButton val) => Config.Key_Spell4 = val);
                capi.RegisterSimpleOption(ModManifest, "Key: Spell 5", "The key for spell 5.", () => Config.Key_Spell5, (SButton val) => Config.Key_Spell5 = val);
            }

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

                    foreach (var magic in Data.players)
                    {
                        if (magic.Value.spellBook.prepared[0].Length == 4)
                        {
                            var newSpells = new PreparedSpell[5];
                            for (int i = 0; i < 4; ++i)
                                newSpells[i] = magic.Value.spellBook.prepared[0][i];
                            magic.Value.spellBook.prepared[0] = newSpells;
                        }

                        if (magic.Value.spellBook.prepared[1].Length == 4)
                        {
                            var newSpells = new PreparedSpell[5];
                            for (int i = 0; i < 4; ++i)
                                newSpells[i] = magic.Value.spellBook.prepared[1][i];
                            magic.Value.spellBook.prepared[1] = newSpells;
                        }
                    }

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

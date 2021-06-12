using System;
using System.IO;
using Magic.Other;
using Newtonsoft.Json;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Magic
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static MultiplayerSaveData Data { get; private set; } = new();
        public static Configuration Config { get; private set; }

        internal static JsonAssetsApi Ja;
        internal static IManaBarApi Mana;

        internal Api Api;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            Mod.Config = this.Helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saving += this.OnSaving;

            Magic.Init(helper.Events, helper.Input, helper.Multiplayer.GetNewID);
        }

        public override object GetApi()
        {
            if (this.Api == null)
                this.Api = new Api();
            return this.Api;
        }

        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(this.ModManifest, () => Mod.Config = new Configuration(), () => this.Helper.WriteConfig(Mod.Config));
                capi.RegisterSimpleOption(this.ModManifest, "Altar Location", "The (internal) name of the location the magic altar should be placed at.", () => Mod.Config.AltarLocation, (string val) => Mod.Config.AltarLocation = val);
                capi.RegisterSimpleOption(this.ModManifest, "Altar X", "The X tile position of where the magic altar should be placed.", () => Mod.Config.AltarX, (int val) => Mod.Config.AltarX = val);
                capi.RegisterSimpleOption(this.ModManifest, "Altar Y", "The Y tile position of where the magic altar should be placed.", () => Mod.Config.AltarY, (int val) => Mod.Config.AltarY = val);
                capi.RegisterSimpleOption(this.ModManifest, "Key: Cast", "The key to initiate casting a spell.", () => Mod.Config.Key_Cast, (SButton val) => Mod.Config.Key_Cast = val);
                capi.RegisterSimpleOption(this.ModManifest, "Key: Swap Spells", "The key to swap spell sets.", () => Mod.Config.Key_SwapSpells, (SButton val) => Mod.Config.Key_SwapSpells = val);
                capi.RegisterSimpleOption(this.ModManifest, "Key: Spell 1", "The key for spell 1.", () => Mod.Config.Key_Spell1, (SButton val) => Mod.Config.Key_Spell1 = val);
                capi.RegisterSimpleOption(this.ModManifest, "Key: Spell 2", "The key for spell 2.", () => Mod.Config.Key_Spell2, (SButton val) => Mod.Config.Key_Spell2 = val);
                capi.RegisterSimpleOption(this.ModManifest, "Key: Spell 3", "The key for spell 3.", () => Mod.Config.Key_Spell3, (SButton val) => Mod.Config.Key_Spell3 = val);
                capi.RegisterSimpleOption(this.ModManifest, "Key: Spell 4", "The key for spell 4.", () => Mod.Config.Key_Spell4, (SButton val) => Mod.Config.Key_Spell4 = val);
                capi.RegisterSimpleOption(this.ModManifest, "Key: Spell 5", "The key for spell 5.", () => Mod.Config.Key_Spell5, (SButton val) => Mod.Config.Key_Spell5 = val);
            }

            var api2 = this.Helper.ModRegistry.GetApi<IManaBarApi>("spacechase0.ManaBar");
            if (api2 == null)
            {
                Log.Error("No mana bar API???");
                return;
            }
            Mod.Mana = api2;

            var api = this.Helper.ModRegistry.GetApi<JsonAssetsApi>("spacechase0.JsonAssets");
            if (api == null)
            {
                Log.Error("No Json Assets API???");
                return;
            }
            Mod.Ja = api;

            api.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "assets"));
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                if (!Game1.IsMultiplayer || Game1.IsMasterGame)
                {
                    Log.Info($"Loading save data (\"{MultiplayerSaveData.FilePath}\")...");
                    Mod.Data = File.Exists(MultiplayerSaveData.FilePath)
                        ? JsonConvert.DeserializeObject<MultiplayerSaveData>(File.ReadAllText(MultiplayerSaveData.FilePath))
                        : new MultiplayerSaveData();

                    foreach (var magic in Mod.Data.Players)
                    {
                        if (magic.Value.SpellBook.Prepared[0].Length == 4)
                        {
                            var newSpells = new PreparedSpell[5];
                            for (int i = 0; i < 4; ++i)
                                newSpells[i] = magic.Value.SpellBook.Prepared[0][i];
                            magic.Value.SpellBook.Prepared[0] = newSpells;
                        }

                        if (magic.Value.SpellBook.Prepared[1].Length == 4)
                        {
                            var newSpells = new PreparedSpell[5];
                            for (int i = 0; i < 4; ++i)
                                newSpells[i] = magic.Value.SpellBook.Prepared[1][i];
                            magic.Value.SpellBook.Prepared[1] = newSpells;
                        }
                    }

                    if (!Mod.Data.Players.ContainsKey(Game1.player.UniqueMultiplayerID))
                        Mod.Data.Players[Game1.player.UniqueMultiplayerID] = new MultiplayerSaveData.PlayerData();
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"Exception loading save data: {ex}");
            }
        }

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (!Game1.IsMultiplayer || Game1.IsMasterGame)
            {
                Log.Info($"Saving save data (\"{MultiplayerSaveData.FilePath}\")...");
                File.WriteAllText(MultiplayerSaveData.FilePath, JsonConvert.SerializeObject(Mod.Data));
            }
        }
    }
}

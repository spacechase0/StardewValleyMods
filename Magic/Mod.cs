using System;
using System.IO;
using Magic.Framework;
using Magic.Framework.Apis;
using SpaceShared;
using SpaceShared.APIs;
using SpaceShared.ConsoleCommands;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Magic
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static Configuration Config { get; private set; }

        internal static JsonAssetsApi Ja;
        internal static IManaBarApi Mana;

        internal Api Api;

        /// <summary>Handles migrating legacy data for a save file.</summary>
        private LegacyDataMigrator LegacyDataMigrator;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            Mod.Config = this.Helper.ReadConfig<Configuration>();

            this.LegacyDataMigrator = new(this.Monitor);

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.Saving += this.OnSaving;

            Framework.Magic.Init(helper.Events, helper.Input, helper.ModRegistry, helper.Multiplayer.GetNewID);
            ConsoleCommandHelper.RegisterCommandsInAssembly(this);
        }

        public override object GetApi()
        {
            return this.Api ??= new Api();
        }

        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
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

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            try
            {
                this.LegacyDataMigrator.OnSaveLoaded();
            }
            catch (Exception ex)
            {
                Log.Warn($"Exception migrating legacy save data: {ex}");
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.DayStarted"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // fix player's mana pool if needed
            if (Game1.player.eventsSeen.Contains(Framework.Magic.LearnedMagicEventId))
                Framework.Magic.FixManaPoolIfNeeded(Game1.player);
        }

        /// <inheritdoc cref="IGameLoopEvents.Saving"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            this.LegacyDataMigrator.OnSaved();
        }
    }
}

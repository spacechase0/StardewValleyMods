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
    /// <summary>The mod entry class.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>Handles migrating legacy data for a save file.</summary>
        private LegacyDataMigrator LegacyDataMigrator;


        /*********
        ** Accessors
        *********/
        public static Mod Instance;
        public static Configuration Config { get; private set; }

        public static JsonAssetsApi Ja;
        public static IManaBarApi Mana;

        public Api Api;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
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

        /// <summary>Get an API that other mods can access. This is always called after <see cref="M:StardewModdingAPI.Mod.Entry(StardewModdingAPI.IModHelper)" />.</summary>
        public override object GetApi()
        {
            return this.Api ??= new Api();
        }


        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // hook asset editor
            this.Helper.Content.AssetEditors.Add(new AltarMapEditor(Mod.Config, this.Helper.Content));

            // hook Generic Mod Config Menu
            {
                var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
                if (configMenu != null)
                {
                    configMenu.RegisterModConfig(
                        mod: this.ModManifest,
                        revertToDefault: () => Mod.Config = new Configuration(),
                        saveToFile: () => this.Helper.WriteConfig(Mod.Config)
                    );
                    configMenu.RegisterSimpleOption(
                        mod: this.ModManifest,
                        optionName: "Altar Location",
                        optionDesc: "The (internal) name of the location the magic altar should be placed at.",
                        optionGet: () => Mod.Config.AltarLocation,
                        optionSet: value => Mod.Config.AltarLocation = value
                    );
                    configMenu.RegisterSimpleOption(
                        mod: this.ModManifest,
                        optionName: "Altar X",
                        optionDesc: "The X tile position of where the magic altar should be placed.",
                        optionGet: () => Mod.Config.AltarX,
                        optionSet: value => Mod.Config.AltarX = value
                    );
                    configMenu.RegisterSimpleOption(
                        mod: this.ModManifest,
                        optionName: "Altar Y",
                        optionDesc: "The Y tile position of where the magic altar should be placed.",
                        optionGet: () => Mod.Config.AltarY,
                        optionSet: value => Mod.Config.AltarY = value
                    );
                    configMenu.RegisterSimpleOption(
                        mod: this.ModManifest,
                        optionName: "Key: Cast",
                        optionDesc: "The key to initiate casting a spell.",
                        optionGet: () => Mod.Config.Key_Cast,
                        optionSet: value => Mod.Config.Key_Cast = value
                    );
                    configMenu.RegisterSimpleOption(
                        mod: this.ModManifest,
                        optionName: "Key: Swap Spells",
                        optionDesc: "The key to swap spell sets.",
                        optionGet: () => Mod.Config.Key_SwapSpells,
                        optionSet: value => Mod.Config.Key_SwapSpells = value
                    );
                    configMenu.RegisterSimpleOption(
                        mod: this.ModManifest,
                        optionName: "Key: Spell 1",
                        optionDesc: "The key for spell 1.",
                        optionGet: () => Mod.Config.Key_Spell1,
                        optionSet: value => Mod.Config.Key_Spell1 = value
                    );
                    configMenu.RegisterSimpleOption(
                        mod: this.ModManifest,
                        optionName: "Key: Spell 2",
                        optionDesc: "The key for spell 2.",
                        optionGet: () => Mod.Config.Key_Spell2,
                        optionSet: value => Mod.Config.Key_Spell2 = value
                    );
                    configMenu.RegisterSimpleOption(
                        mod: this.ModManifest,
                        optionName: "Key: Spell 3",
                        optionDesc: "The key for spell 3.",
                        optionGet: () => Mod.Config.Key_Spell3,
                        optionSet: value => Mod.Config.Key_Spell3 = value
                    );
                    configMenu.RegisterSimpleOption(
                        mod: this.ModManifest,
                        optionName: "Key: Spell 4",
                        optionDesc: "The key for spell 4.",
                        optionGet: () => Mod.Config.Key_Spell4,
                        optionSet: value => Mod.Config.Key_Spell4 = value
                    );
                    configMenu.RegisterSimpleOption(
                        mod: this.ModManifest,
                        optionName: "Key: Spell 5",
                        optionDesc: "The key for spell 5.",
                        optionGet: () => Mod.Config.Key_Spell5,
                        optionSet: value => Mod.Config.Key_Spell5 = value
                    );
                }
            }

            // hook Mana Bar
            {
                var manaBar = this.Helper.ModRegistry.GetApi<IManaBarApi>("spacechase0.ManaBar");
                if (manaBar == null)
                {
                    Log.Error("No mana bar API???");
                    return;
                }
                Mod.Mana = manaBar;
            }

            // hook Json Assets
            {
                var api = this.Helper.ModRegistry.GetApi<JsonAssetsApi>("spacechase0.JsonAssets");
                if (api == null)
                {
                    Log.Error("No Json Assets API???");
                    return;
                }
                Mod.Ja = api;
                api.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "assets"));
            }
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
            // fix player's magic info if needed
            Framework.Magic.FixMagicIfNeeded(Game1.player);
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

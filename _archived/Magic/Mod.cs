using System;
using System.IO;
using Magic.Framework;
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

        public static IJsonAssetsApi Ja;
        public static IManaBarApi Mana;

        /// <summary>Whether Stardew Valley Expanded is installed.</summary>
        public static bool HasStardewValleyExpanded => Mod.Instance.Helper.ModRegistry.IsLoaded("FlashShifter.SVECode");

        public Api Api;

        private MapEditor editor;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
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
            this.editor = new(Mod.Config, this.Helper.ModContent, HasStardewValleyExpanded);
            this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;

            // hook Generic Mod Config Menu
            {
                var configMenu = this.Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
                if (configMenu != null)
                {
                    configMenu.Register(
                        mod: this.ModManifest,
                        reset: () => Mod.Config = new Configuration(),
                        save: () => this.Helper.WriteConfig(Mod.Config),
                        titleScreenOnly: true
                    );

                    // altar placement
                    configMenu.AddSectionTitle(this.ModManifest, I18n.Config_AltarPlacement);
                    configMenu.AddTextOption(
                        mod: this.ModManifest,
                        name: I18n.Config_Location_Name,
                        tooltip: I18n.Config_Location_Tooltip,
                        getValue: () => Mod.Config.AltarLocation,
                        setValue: value => Mod.Config.AltarLocation = value
                    );
                    configMenu.AddNumberOption(
                        mod: this.ModManifest,
                        name: I18n.Config_X_Name,
                        tooltip: I18n.Config_X_Tooltip,
                        getValue: () => Mod.Config.AltarX,
                        setValue: value => Mod.Config.AltarX = value,
                        min: -1,
                        max: 255,
                        formatValue: value => value < 0
                            ? I18n.Config_XOrY_AutomaticPosition()
                            : value.ToString()
                    );
                    configMenu.AddNumberOption(
                        mod: this.ModManifest,
                        name: I18n.Config_Y_Name,
                        tooltip: I18n.Config_Y_Tooltip,
                        getValue: () => Mod.Config.AltarY,
                        setValue: value => Mod.Config.AltarY = value,
                        min: -1,
                        max: 255,
                        formatValue: value => value < 0
                            ? I18n.Config_XOrY_AutomaticPosition()
                            : value.ToString()
                    );

                    // radio placement
                    configMenu.AddSectionTitle(this.ModManifest, I18n.Config_RadioPlacement);
                    configMenu.AddTextOption(
                        mod: this.ModManifest,
                        name: I18n.Config_Location_Name,
                        tooltip: I18n.Config_Location_Tooltip,
                        getValue: () => Mod.Config.RadioLocation,
                        setValue: value => Mod.Config.RadioLocation = value
                    );
                    configMenu.AddNumberOption(
                        mod: this.ModManifest,
                        name: I18n.Config_X_Name,
                        tooltip: I18n.Config_X_Tooltip,
                        getValue: () => Mod.Config.RadioX,
                        setValue: value => Mod.Config.RadioX = value,
                        min: -1,
                        max: 255,
                        formatValue: value => value < 0
                            ? I18n.Config_XOrY_AutomaticPosition()
                            : value.ToString()
                    );
                    configMenu.AddNumberOption(
                        mod: this.ModManifest,
                        name: I18n.Config_Y_Name,
                        tooltip: I18n.Config_Y_Tooltip,
                        getValue: () => Mod.Config.RadioY,
                        setValue: value => Mod.Config.RadioY = value,
                        min: -1,
                        max: 255,
                        formatValue: value => value < 0
                            ? I18n.Config_XOrY_AutomaticPosition()
                            : value.ToString()
                    );

                    // controls
                    configMenu.AddSectionTitle(this.ModManifest, I18n.Config_Controls);
                    configMenu.AddKeybind(
                        mod: this.ModManifest,
                        name: I18n.Config_CastKey_Name,
                        tooltip: I18n.Config_CastKey_Tooltip,
                        getValue: () => Mod.Config.Key_Cast,
                        setValue: value => Mod.Config.Key_Cast = value
                    );
                    configMenu.AddKeybind(
                        mod: this.ModManifest,
                        name: I18n.Config_SwapSpellsKey_Name,
                        tooltip: I18n.Config_SwapSpellsKey_Tooltip,
                        getValue: () => Mod.Config.Key_SwapSpells,
                        setValue: value => Mod.Config.Key_SwapSpells = value
                    );
                    configMenu.AddKeybind(
                        mod: this.ModManifest,
                        name: () => I18n.Config_SelectSpellKey_Name(slotNumber: 1),
                        tooltip: () => I18n.Config_SelectSpellKey_Tooltip(slotNumber: 1),
                        getValue: () => Mod.Config.Key_Spell1,
                        setValue: value => Mod.Config.Key_Spell1 = value
                    );
                    configMenu.AddKeybind(
                        mod: this.ModManifest,
                        name: () => I18n.Config_SelectSpellKey_Name(slotNumber: 2),
                        tooltip: () => I18n.Config_SelectSpellKey_Tooltip(slotNumber: 2),
                        getValue: () => Mod.Config.Key_Spell2,
                        setValue: value => Mod.Config.Key_Spell2 = value
                    );
                    configMenu.AddKeybind(
                        mod: this.ModManifest,
                        name: () => I18n.Config_SelectSpellKey_Name(slotNumber: 3),
                        tooltip: () => I18n.Config_SelectSpellKey_Tooltip(slotNumber: 3),
                        getValue: () => Mod.Config.Key_Spell3,
                        setValue: value => Mod.Config.Key_Spell3 = value
                    );
                    configMenu.AddKeybind(
                        mod: this.ModManifest,
                        name: () => I18n.Config_SelectSpellKey_Name(slotNumber: 4),
                        tooltip: () => I18n.Config_SelectSpellKey_Tooltip(slotNumber: 4),
                        getValue: () => Mod.Config.Key_Spell4,
                        setValue: value => Mod.Config.Key_Spell4 = value
                    );
                    configMenu.AddKeybind(
                        mod: this.ModManifest,
                        name: () => I18n.Config_SelectSpellKey_Name(slotNumber: 5),
                        tooltip: () => I18n.Config_SelectSpellKey_Tooltip(slotNumber: 5),
                        getValue: () => Mod.Config.Key_Spell5,
                        setValue: value => Mod.Config.Key_Spell5 = value
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
                var api = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
                if (api == null)
                {
                    Log.Error("No Json Assets API???");
                    return;
                }
                Mod.Ja = api;
                api.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "assets", "json-assets"), this.Helper.Translation);
            }
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            this.editor.TryEdit(e);
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

            this.Helper.Events.GameLoop.Saving -= this.OnSaving;
        }
    }
}

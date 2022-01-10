using System;
using System.Collections.Generic;
using SleepyEye.Framework;
using SpaceShared;
using SpaceShared.APIs;
using SpaceShared.Migrations;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace SleepyEye
{
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The data key in the save file for the tent location.</summary>
        private readonly string TentLocationKey = "tent-location";


        /*********
        ** Accessors
        *********/
        /// <summary>The static mod instance.</summary>
        public static Mod Instance;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            // load config
            this.ApplyConfig(helper.ReadConfig<ModConfig>());

            // init
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        }

        /// <summary>Remember the location where the player saved, and restore it on the next day.</summary>
        internal void RememberLocation()
        {
            if (Context.IsMainPlayer) // TODO multiplayer support
            {
                this.Helper.Data.WriteSaveData(this.TentLocationKey, new CampData
                {
                    Location = Game1.player.currentLocation?.NameOrUniqueName,
                    Position = Game1.player.Position,
                    MineLevel = (Game1.currentLocation as MineShaft)?.mineLevel,
                    DaysPlayed = Game1.Date.TotalDays
                });
            }
        }


        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var spaceCore = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            spaceCore.RegisterSerializerType(typeof(TentTool));

            var configMenu = this.Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => this.ApplyConfig(new ModConfig()),
                    save: this.SaveConfig
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: I18n.Config_SecondsUntilSave_Name,
                    tooltip: I18n.Config_SecondsUntilSave_Tooltip,
                    getValue: () => (int)TentTool.UseDelay.TotalSeconds,
                    setValue: value => TentTool.UseDelay = TimeSpan.FromSeconds(value)
                );
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Context.IsMainPlayer)
                PyTkMigrator.MigrateItems("SleepyEye.TentTool,  SleepyEye", _ => new TentTool());
        }

        /// <inheritdoc cref="IGameLoopEvents.DayStarted"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // move player back to their tent location
            if (Context.IsMainPlayer)
            {
                CampData camp = this.Helper.Data.ReadSaveData<CampData>(this.TentLocationKey);
                if (camp != null && camp.DaysPlayed == Game1.Date.TotalDays - 1)
                {
                    this.Helper.Data.WriteSaveData<CampData>(this.TentLocationKey, null);
                    this.TryRestoreLocation(camp);
                }
            }
        }

        /// <inheritdoc cref="IDisplayEvents.MenuChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is not ShopMenu menu || menu.portraitPerson?.Name != "Pierre")
                return;

            Log.Debug("Adding tent to shop");

            var forSale = menu.forSale;
            var itemPriceAndStock = menu.itemPriceAndStock;

            var item = new TentTool();
            forSale.Add(item);
            itemPriceAndStock.Add(item, new[] { item.salePrice(), item.Stack });
        }

        /// <summary>Move the player back to where they camped, if possible.</summary>
        /// <param name="camp">The metadata about where and when the player camped.</param>
        private void TryRestoreLocation(CampData camp)
        {
            Log.Debug("Previously slept in a tent, replacing player position.");

            // get location
            GameLocation location = Game1.getLocationFromName(camp.Location);
            if (location == null)
            {
                Game1.addHUDMessage(new HUDMessage(I18n.Messages_SleptAtLostLocation()));
                return;
            }
            if (location.Name == this.GetFestivalLocation())
            {
                Game1.addHUDMessage(new HUDMessage(I18n.Messages_SleptAtFestival()));
                return;
            }

            // restore position
            if (location is MineShaft)
            {
                Log.Trace("Slept in a mine.");
                Game1.enterMine(camp.MineLevel ?? 0);
            }
            else
            {
                Game1.player.currentLocation = Game1.currentLocation = location;
                Game1.player.Position = camp.Position;
            }
        }

        /// <summary>Get the location where a festival will be today, if any.</summary>
        internal string GetFestivalLocation()
        {
            try
            {
                return Game1.temporaryContent.Load<Dictionary<string, string>>($"Data\\Festivals\\{Game1.currentSeason}{Game1.dayOfMonth}")["conditions"].Split('/')[0];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Apply the given mod configuration.</summary>
        /// <param name="config">The configuration model.</param>
        private void ApplyConfig(ModConfig config)
        {
            TentTool.UseDelay = TimeSpan.FromSeconds(config.SecondsUntilSave);
        }

        /// <summary>Save the current mod configuration.</summary>
        private void SaveConfig()
        {
            this.Helper.WriteConfig(new ModConfig
            {
                SecondsUntilSave = (int)TentTool.UseDelay.TotalSeconds
            });
        }
    }
}

using System;
using SleepyEye.Framework;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;

namespace SleepyEye
{
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The static mod instance.</summary>
        public static Mod Instance;

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            // load config
            this.ApplyConfig(helper.ReadConfig<ModConfig>());

            // init
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var gmcm = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm != null)
            {
                gmcm.RegisterModConfig(this.ModManifest, revertToDefault: () => this.ApplyConfig(new ModConfig()), saveToFile: this.SaveConfig);
                gmcm.RegisterSimpleOption(this.ModManifest, "Seconds until save", "The number of seconds until the tent tool should trigger a save.", () => (int)TentTool.UseDelay.TotalSeconds, (int val) => TentTool.UseDelay = TimeSpan.FromSeconds(val));
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is not ShopMenu menu || menu.portraitPerson.Name != "Pierre")
                return;

            Log.Debug("Adding tent to shop");

            var forSale = menu.forSale;
            var itemPriceAndStock = menu.itemPriceAndStock;

            var item = new TentTool();
            forSale.Add(item);
            itemPriceAndStock.Add(item, new[] { item.salePrice(), item.Stack });
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

using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Spacechase.Shared.Harmony;
using SpaceCore.Framework;
using SpaceCore.Patches;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace SpaceCore
{
    public class SpaceCore : Mod
    {
        public Configuration Config { get; set; }
        internal static SpaceCore instance;
        private HarmonyInstance harmony;

        internal static List<Type> modTypes = new List<Type>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            SpaceCore.instance = this;
            Log.Monitor = this.Monitor;
            this.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.onGameLaunched;
            helper.Events.GameLoop.UpdateTicked += this.onUpdate;
            helper.Events.GameLoop.SaveLoaded += this.onSaveLoaded;
            helper.Events.GameLoop.Saving += this.onSaving;
            helper.Events.GameLoop.Saved += this.onSaved;

            Commands.register();
            Skills.init(helper.Events);
            TileSheetExtensions.init();

            var serializerManager = new SerializerManager();

            this.harmony = HarmonyPatcher.Apply(this,
                new EventPatcher(),
                new FarmerPatcher(),
                new Game1Patcher(),
                new GameLocationPatcher(),
                new GameMenuPatcher(),
                new GameServerPatcher(),
                new HoeDirtPatcher(),
                new LoadGameMenuPatcher(serializerManager),
                new MeleeWeaponPatcher(),
                new MultiplayerPatcher(),
                new NpcPatcher(),
                new SaveGamePatcher(serializerManager),
                new SpriteBatchPatcher(),
                new UtilityPatcher()
            );
        }

        public override object GetApi()
        {
            return new Api();
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = this.Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(this.ModManifest, () => this.Config = new Configuration(), () => this.Helper.WriteConfig(this.Config));
                capi.RegisterSimpleOption(this.ModManifest, "Custom Skill Page", "Whether or not to show the custom skill page.\nThis will move the wallet so that there is room for more skills.", () => this.Config.CustomSkillPage, (bool val) => this.Config.CustomSkillPage = val);
            }

            var efapi = this.Helper.ModRegistry.GetApi<EntoaroxFrameworkAPI>("Entoarox.EntoaroxFramework");
            if (efapi != null)
            {
                Log.info("Telling EntoaroxFramework to let us handle the serializer");
                efapi.HoistSerializerOwnership();
            }
        }

        private int tickCount = 0;
        private void onUpdate(object sender, UpdateTickedEventArgs e)
        {
            TileSheetExtensions.UpdateReferences();
            if (this.tickCount++ == 0 && SpaceCore.modTypes.Count == 0)
            {
                Log.info("Disabling serializer patches (no mods using serializer API)");
                foreach (var meth in SaveGamePatcher.GetSaveEnumeratorMethods())
                    this.harmony.Unpatch(meth, PatchHelper.RequireMethod<SaveGamePatcher>(nameof(SaveGamePatcher.Transpile_GetSaveEnumerator)));
                foreach (var meth in SaveGamePatcher.GetLoadEnumeratorMethods())
                    this.harmony.Unpatch(meth, PatchHelper.RequireMethod<SaveGamePatcher>(nameof(SaveGamePatcher.Transpile_GetLoadEnumerator)));
            }
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // todo - MP support
            if (!Context.IsMainPlayer)
                return;

            // Sleep position stuff
            var data = this.Helper.Data.ReadSaveData<Sleep.Data>("sleepy-eye");
            if (data == null)
            {
                var legacyDataPath = Path.Combine(Constants.CurrentSavePath, "sleepy-eye.json");
                data = File.Exists(legacyDataPath)
                    ? JsonConvert.DeserializeObject<Sleep.Data>(File.ReadAllText(legacyDataPath))
                    : null;
            }
            if (data == null || data.Year != Game1.year || data.Season != Game1.currentSeason || data.Day != Game1.dayOfMonth)
                return;

            Log.debug("Previously slept in a tent, replacing player position.");

            var loc = Game1.getLocationFromName(data.Location);
            if (loc == null || loc.Name == this.festivalLocation())
            {
                Game1.addHUDMessage(new HUDMessage("You camped out where the festival was, so you have returned home."));
                return;
            }

            if (loc is MineShaft)
            {
                Log.trace("Slept in a mine.");
                Game1.enterMine(data.MineLevel);
                data.X = -1;
                data.Y = -1;
            }
            else
            {
                Game1.player.currentLocation = Game1.currentLocation = loc;
                Game1.player.Position = new Vector2(data.X, data.Y);
            }
        }

        /// <summary>Raised before the game begins writes data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaving(object sender, SavingEventArgs e)
        {
            if (!Sleep.SaveLocation)
                return;

            Log.debug("Saving tent sleep data");

            if (Game1.player.currentLocation.Name == this.festivalLocation())
            {
                Log.trace("There'll be a festival here tomorrow, canceling");
                Game1.addHUDMessage(new HUDMessage("You camped out where the festival was, so you have returned home."));

                var house = Game1.getLocationFromName("FarmHouse") as FarmHouse;
                Game1.player.currentLocation = Game1.currentLocation = house;
                Game1.player.Position = new Vector2(house.getBedSpot().X * Game1.tileSize, house.getBedSpot().Y * Game1.tileSize);
                Sleep.SaveLocation = false;
                return;
            }

            var data = new Sleep.Data();
            data.Location = Game1.currentLocation.Name;
            if (data.X != -1 && data.Y != -1)
            {
                data.X = Game1.player.position.X;
                data.Y = Game1.player.position.Y;
            }

            data.Year = Game1.year;
            data.Season = Game1.currentSeason;
            data.Day = Game1.dayOfMonth;

            if (Game1.currentLocation is MineShaft)
            {
                data.MineLevel = (Game1.currentLocation as MineShaft).mineLevel;
            }

            this.Helper.Data.WriteSaveData("sleepy-eye", data);
            Sleep.SaveLocation = false;
        }

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaved(object sender, SavedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            var legacyDataPath = Path.Combine(Constants.CurrentSavePath, "sleepy-eye.json");
            if (File.Exists(legacyDataPath))
            {
                Log.trace($"Deleting legacy tent sleep data file: {legacyDataPath}");
                File.Delete(legacyDataPath);
            }
        }

        // TODO: Move somewhere more sensible (and make public)?
        internal string festivalLocation()
        {
            try
            {
                return Game1.temporaryContent.Load<Dictionary<string, string>>($"Data\\Festivals\\{Game1.currentSeason}{Game1.dayOfMonth}")["conditions"].Split('/')[0];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

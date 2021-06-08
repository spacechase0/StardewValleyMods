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
            instance = this;
            Log.Monitor = Monitor;
            Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += onGameLaunched;
            helper.Events.GameLoop.UpdateTicked += onUpdate;
            helper.Events.GameLoop.SaveLoaded += onSaveLoaded;
            helper.Events.GameLoop.Saving += onSaving;
            helper.Events.GameLoop.Saved += onSaved;

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
            var capi = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(ModManifest, () => Config = new Configuration(), () => Helper.WriteConfig(Config));
                capi.RegisterSimpleOption(ModManifest, "Custom Skill Page", "Whether or not to show the custom skill page.\nThis will move the wallet so that there is room for more skills.", () => Config.CustomSkillPage, (bool val) => Config.CustomSkillPage = val);
            }

            var efapi = Helper.ModRegistry.GetApi<EntoaroxFrameworkAPI>("Entoarox.EntoaroxFramework");
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
            if (tickCount++ == 0 && modTypes.Count == 0)
            {
                Log.info("Disabling serializer patches (no mods using serializer API)");
                foreach (var meth in SaveGamePatcher.GetSaveEnumeratorMethods())
                    harmony.Unpatch(meth, PatchHelper.RequireMethod<SaveGamePatcher>(nameof(SaveGamePatcher.Transpile_GetSaveEnumerator)));
                foreach (var meth in SaveGamePatcher.GetLoadEnumeratorMethods())
                    harmony.Unpatch(meth, PatchHelper.RequireMethod<SaveGamePatcher>(nameof(SaveGamePatcher.Transpile_GetLoadEnumerator)));
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
            var data = Helper.Data.ReadSaveData<Sleep.Data>("sleepy-eye");
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
            if (loc == null || loc.Name == festivalLocation())
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

            if (Game1.player.currentLocation.Name == festivalLocation())
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

            Helper.Data.WriteSaveData("sleepy-eye", data);
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

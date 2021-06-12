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
    internal class SpaceCore : Mod
    {
        public Configuration Config { get; set; }
        internal static SpaceCore Instance;
        internal static IReflectionHelper Reflection;
        private HarmonyInstance Harmony;

        internal static List<Type> ModTypes = new();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            SpaceCore.Instance = this;
            SpaceCore.Reflection = helper.Reflection;
            Log.Monitor = this.Monitor;
            this.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdate;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.GameLoop.Saved += this.OnSaved;

            Commands.Register();
            Skills.Init(helper.Events);
            TileSheetExtensions.Init();

            var serializerManager = new SerializerManager();

            this.Harmony = HarmonyPatcher.Apply(this,
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

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(this.ModManifest, () => this.Config = new Configuration(), () => this.Helper.WriteConfig(this.Config));
                capi.RegisterSimpleOption(this.ModManifest, "Custom Skill Page", "Whether or not to show the custom skill page.\nThis will move the wallet so that there is room for more skills.", () => this.Config.CustomSkillPage, (bool val) => this.Config.CustomSkillPage = val);
            }

            var efapi = this.Helper.ModRegistry.GetApi<IEntoaroxFrameworkApi>("Entoarox.EntoaroxFramework");
            if (efapi != null)
            {
                Log.Info("Telling EntoaroxFramework to let us handle the serializer");
                efapi.HoistSerializerOwnership();
            }
        }

        private int TickCount;
        private void OnUpdate(object sender, UpdateTickedEventArgs e)
        {
            TileSheetExtensions.UpdateReferences();
            if (this.TickCount++ == 0 && SpaceCore.ModTypes.Count == 0)
            {
                Log.Info("Disabling serializer patches (no mods using serializer API)");
                foreach (var meth in SaveGamePatcher.GetSaveEnumeratorMethods())
                    this.Harmony.Unpatch(meth, PatchHelper.RequireMethod<SaveGamePatcher>(nameof(SaveGamePatcher.Transpile_GetSaveEnumerator)));
                foreach (var meth in SaveGamePatcher.GetLoadEnumeratorMethods())
                    this.Harmony.Unpatch(meth, PatchHelper.RequireMethod<SaveGamePatcher>(nameof(SaveGamePatcher.Transpile_GetLoadEnumerator)));
            }
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // todo - MP support
            if (!Context.IsMainPlayer)
                return;

            // Sleep position stuff
            var data = this.Helper.Data.ReadSaveData<Sleep.Data>("sleepy-eye");
            if (data == null)
            {
                string legacyDataPath = Path.Combine(Constants.CurrentSavePath, "sleepy-eye.json");
                data = File.Exists(legacyDataPath)
                    ? JsonConvert.DeserializeObject<Sleep.Data>(File.ReadAllText(legacyDataPath))
                    : null;
            }
            if (data == null || data.Year != Game1.year || data.Season != Game1.currentSeason || data.Day != Game1.dayOfMonth)
                return;

            Log.Debug("Previously slept in a tent, replacing player position.");

            var loc = Game1.getLocationFromName(data.Location);
            if (loc == null || loc.Name == this.FestivalLocation())
            {
                Game1.addHUDMessage(new HUDMessage("You camped out where the festival was, so you have returned home."));
                return;
            }

            if (loc is MineShaft)
            {
                Log.Trace("Slept in a mine.");
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
        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (!Sleep.SaveLocation)
                return;

            Log.Debug("Saving tent sleep data");

            if (Game1.player.currentLocation.Name == this.FestivalLocation())
            {
                Log.Trace("There'll be a festival here tomorrow, canceling");
                Game1.addHUDMessage(new HUDMessage("You camped out where the festival was, so you have returned home."));

                var house = Game1.getLocationFromName("FarmHouse") as FarmHouse;
                Game1.player.currentLocation = Game1.currentLocation = house;
                Game1.player.Position = new Vector2(house.getBedSpot().X * Game1.tileSize, house.getBedSpot().Y * Game1.tileSize);
                Sleep.SaveLocation = false;
                return;
            }

            var data = new Sleep.Data
            {
                Location = Game1.currentLocation.Name
            };
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
        private void OnSaved(object sender, SavedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            string legacyDataPath = Path.Combine(Constants.CurrentSavePath, "sleepy-eye.json");
            if (File.Exists(legacyDataPath))
            {
                Log.Trace($"Deleting legacy tent sleep data file: {legacyDataPath}");
                File.Delete(legacyDataPath);
            }
        }

        // TODO: Move somewhere more sensible (and make public)?
        internal string FestivalLocation()
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

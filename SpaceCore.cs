using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Overrides;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using StardewValley.Menus;
using System.Linq;
using SpaceShared;
using SpaceShared.APIs;

namespace SpaceCore
{
    public class SpaceCore : Mod
    {
        public Configuration Config { get; set; }
        internal static SpaceCore instance;
        private HarmonyInstance harmony;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.UpdateTicked += onUpdate;
            helper.Events.GameLoop.SaveLoaded += onSaveLoaded;
            helper.Events.GameLoop.Saving += onSaving;
            helper.Events.GameLoop.Saved += onSaved;

            Commands.register();
            Skills.init(helper.Events);
            TileSheetExtensions.init();

            harmony = HarmonyInstance.Create("spacechase0.SpaceCore");

            MethodInfo showNightEndMethod = null;
            try
            {
                Type game1CompilerType = null;
                foreach (var t in typeof(Game1).Assembly.GetTypes())
                    if (t.FullName == "StardewValley.Game1+<>c")
                        game1CompilerType = t;
                foreach (var m in game1CompilerType.GetRuntimeMethods())
                    if (m.FullDescription().Contains("showEndOfNightStuff"))
                        showNightEndMethod = m;
            }
            catch (ReflectionTypeLoadException e)
            {
                Log.error($"Weird exception doing finding Windows showEndOfNightStuff: {e}");
                foreach (var le in e.LoaderExceptions)
                {
                    Log.error("LE: " + le);
                }
            }
            catch ( Exception e1 )
            {
                Log.trace("Failed to find Windows showEndOfNightStuff lambda: " + e1);
                try
                {
                    Type game1CompilerType = typeof(Game1);
                    foreach (var m in game1CompilerType.GetRuntimeMethods())
                        if (m.FullDescription().Contains("<showEndOfNightStuff>m__"))
                            showNightEndMethod = m;
                }
                catch ( Exception e2 )
                {
                    Log.error("Failed to find Mac/Linux showEndOfNightStuff lambda: " + e2);
                }
            }
            Log.trace("showEndOfNightStuff: " + showNightEndMethod);

            doPrefix(typeof(HoeDirt), nameof(HoeDirt.dayUpdate), typeof(HoeDirtWinterFix));
            doPostfix(typeof(Utility), nameof(Utility.pickFarmEvent), typeof(NightlyFarmEventHook));
            doTranspiler(showNightEndMethod, typeof(ShowEndOfNightStuffHook).GetMethod(nameof(ShowEndOfNightStuffHook.Transpiler)));
            doPostfix(typeof(Farmer), nameof(Farmer.doneEating), typeof(DoneEatingHook));
            doPrefix(typeof(MeleeWeapon).GetMethod(nameof(MeleeWeapon.drawDuringUse), new[] { typeof(int), typeof(int), typeof(SpriteBatch), typeof(Vector2), typeof(Farmer), typeof(Rectangle), typeof(int), typeof(bool) }), typeof(CustomWeaponDrawPatch).GetMethod(nameof(CustomWeaponDrawPatch.Prefix)));
            doPrefix(typeof(Multiplayer), nameof(Multiplayer.processIncomingMessage), typeof(MultiplayerPackets));
            doPrefix(typeof(GameLocation), nameof(GameLocation.performAction), typeof(ActionHook));
            doPrefix(typeof(GameLocation), nameof(GameLocation.performTouchAction), typeof(TouchActionHook));
            doPostfix(typeof(GameLocation), nameof(GameLocation.explode), typeof(ExplodeHook));
            doPostfix(typeof(GameServer), nameof(GameServer.sendServerIntroduction), typeof(ServerGotClickHook));
            doPostfix(typeof(NPC), nameof(NPC.receiveGift), typeof(AfterGiftGivenHook));
            doPostfix(typeof(Game1), nameof(Game1.loadForNewGame), typeof(BlankSaveHook));
            if(Constants.TargetPlatform != GamePlatform.Android)
            {
                doPrefix(typeof(Game1).GetMethod(nameof(Game1.warpFarmer), new[] { typeof(LocationRequest), typeof(int), typeof(int), typeof(int) }), typeof(WarpFarmerHook).GetMethod(nameof(WarpFarmerHook.Prefix)));
            }
            else
            {
                doPrefix(typeof(Game1).GetMethod(nameof(Game1.warpFarmer), new[] { typeof(LocationRequest), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool) }), typeof(WarpFarmerHook).GetMethod(nameof(WarpFarmerHook.Prefix)));
            }
            doPostfix(typeof(GameMenu), nameof(GameMenu.getTabNumberFromName), typeof(GameMenuTabNameHook));
            doPrefix(typeof(SpriteBatch).GetMethod("Draw", new[] { typeof( Texture2D ), typeof( Rectangle ), typeof( Rectangle? ), typeof( Color ), typeof( float ), typeof( Vector2 ),                    typeof( SpriteEffects ), typeof( float ) }), typeof(SpriteBatchTileSheetAdjustments).GetMethod(nameof(SpriteBatchTileSheetAdjustments.Prefix1)));
            doPrefix(typeof(SpriteBatch).GetMethod("Draw", new[] { typeof( Texture2D ), typeof( Rectangle ), typeof( Rectangle? ), typeof( Color ),                                                                                                 }), typeof(SpriteBatchTileSheetAdjustments).GetMethod(nameof(SpriteBatchTileSheetAdjustments.Prefix2)));
            doPrefix(typeof(SpriteBatch).GetMethod("Draw", new[] { typeof( Texture2D ), typeof( Vector2   ), typeof( Rectangle? ), typeof( Color ), typeof( float ), typeof( Vector2 ), typeof( Vector2 ), typeof( SpriteEffects ), typeof( float ) }), typeof(SpriteBatchTileSheetAdjustments).GetMethod(nameof(SpriteBatchTileSheetAdjustments.Prefix3)));
            doPrefix(typeof(SpriteBatch).GetMethod("Draw", new[] { typeof( Texture2D ), typeof( Vector2   ), typeof( Rectangle? ), typeof( Color ), typeof( float ), typeof( Vector2 ), typeof( float   ), typeof( SpriteEffects ), typeof( float ) }), typeof(SpriteBatchTileSheetAdjustments).GetMethod(nameof(SpriteBatchTileSheetAdjustments.Prefix4)));
            doPrefix(typeof(SpriteBatch).GetMethod("Draw", new[] { typeof( Texture2D ), typeof( Vector2   ), typeof( Rectangle? ), typeof( Color )                                                                                                  }), typeof(SpriteBatchTileSheetAdjustments).GetMethod(nameof(SpriteBatchTileSheetAdjustments.Prefix5)));
        }

        public override object GetApi()
        {
            return new Api();
        }

        private void doPrefix(Type origType, string origMethod, Type newType)
        {
            doPrefix(origType.GetMethod(origMethod, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static), newType.GetMethod("Prefix"));
        }
        private void doPrefix(MethodInfo orig, MethodInfo prefix)
        {
            try
            {
                Log.trace($"Doing prefix patch {orig}:{prefix}...");
                harmony.Patch(orig, new HarmonyMethod(prefix), null);
            }
            catch (Exception e)
            {
                Log.error($"Exception doing prefix patch {orig}:{prefix}: {e}");
            }
        }
        private void doPostfix(Type origType, string origMethod, Type newType)
        {
            doPostfix(origType.GetMethod(origMethod, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static), newType.GetMethod("Postfix"));
        }
        private void doPostfix(MethodInfo orig, MethodInfo postfix)
        {
            try
            {
                Log.trace($"Doing postfix patch {orig}:{postfix}...");
                harmony.Patch(orig, null, new HarmonyMethod(postfix));
            }
            catch (Exception e)
            {
                Log.error($"Exception doing postfix patch {orig}:{postfix}: {e}");
            }
        }
        private void doTranspiler(Type origType, string origMethod, Type newType)
        {
            doTranspiler(origType.GetMethod(origMethod, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static), newType.GetMethod("Transpiler"));
        }
        private void doTranspiler(MethodInfo orig, MethodInfo transpiler)
        {
            try
            {
                Log.trace($"Doing transpiler patch {orig}:{transpiler}...");
                harmony.Patch(orig, null, null, new HarmonyMethod(transpiler));
            }
            catch (Exception e)
            {
                Log.error($"Exception doing transpiler patch {orig}:{transpiler}: {e}");
            }
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(ModManifest, () => Config = new Configuration(), () => Helper.WriteConfig(Config));
                capi.RegisterSimpleOption(ModManifest, "Custom Skill Page", "Whether or not to show the custom skill page.\nThis will move the wallet so that there is room for more skills.", () => Config.CustomSkillPage, (bool val) => Config.CustomSkillPage = val);
            }
        }

        private void onUpdate(object sender, UpdateTickedEventArgs e)
        {
            TileSheetExtensions.UpdateReferences();
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
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

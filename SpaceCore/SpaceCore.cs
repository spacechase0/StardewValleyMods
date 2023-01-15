using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Spacechase.Shared.Patching;
using SpaceCore.Events;
using SpaceCore.Framework;
using SpaceCore.Framework.ExtEngine;
using SpaceCore.Interface;
using SpaceCore.Patches;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Network;

namespace SpaceCore
{
    /*
    public static class Fix1_5NetCodeBugPatch
    {
        public static void Prefix(
            NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>> __instance,
            string key,
            ref object __state
        )
        {
            __state = __instance is ModDataDictionary && __instance.ContainsKey(key);
        }
        public static void Postfix(
            NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>> __instance,
            string key,
            string value,
            object __state,
            System.Collections.IList ___outgoingChanges,
            Dictionary<string, NetVersion> ___dictReassigns
        )
        {
            if(__instance is ModDataDictionary)
            if (__state as bool? == true)
            {
                var field = __instance.FieldDict[key];
                var ogts = __instance.GetType().BaseType.BaseType.BaseType.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                var ogt = ogts.First(t => t.Name.StartsWith("OutgoingChange"));
                ogt = ogt.MakeGenericType(new Type[] { typeof( string ), typeof( string ), typeof( NetString ), typeof( SerializableDictionary<string, string> ), typeof( NetStringDictionary<string,NetString> ) });
                var ogc = ogt.GetConstructors(BindingFlags.Public | BindingFlags.Instance)[0];
                object og = ogc.Invoke(new object[] { false, key, field, ___dictReassigns[ key ] });
                ___outgoingChanges.Add(og);
                if (key.Contains("spacechase0"))
                    Log.Debug("oc:" + ___outgoingChanges.Count);
            }
        }
    }
    */

    /// <summary>The mod entry class.</summary>
    internal class SpaceCore : Mod
    {
        /*********
        ** Fields
        *********/
        internal Harmony Harmony;

        /// <summary>Handles migrating legacy data for a save file.</summary>
        private LegacyDataMigrator LegacyDataMigrator;

        /// <summary>Whether the current update tick is the first one raised by SMAPI.</summary>
        private bool IsFirstTick;


        /*********
        ** Accessors
        *********/
        public Configuration Config { get; set; }
        internal static SpaceCore Instance;
        internal static IReflectionHelper Reflection;
        internal static List<Type> ModTypes = new();
        internal static Dictionary<Type, Dictionary<string, CustomPropertyInfo>> CustomProperties = new();


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.LegacyDataMigrator = new LegacyDataMigrator(helper.Data, this.Monitor);

            I18n.Init(helper.Translation);
            SpaceCore.Instance = this;
            SpaceCore.Reflection = helper.Reflection;
            Log.Monitor = this.Monitor;
            this.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;

            EventPatcher.CustomCommands.Add("damageFarmer", AccessTools.Method(this.GetType(), "DamageFarmerEventCommand"));

            Commands.Register();
            ExtensionEngine.Init();

            var serializerManager = new SerializerManager(helper.ModRegistry);

            this.Harmony = HarmonyPatcher.Apply(this,
                new CraftingRecipePatcher(),
                new FarmerPatcher(),
                new ForgeMenuPatcher(),
                new Game1Patcher(),
                new GameLocationPatcher(),
                new GameServerPatcher(),
                new LoadGameMenuPatcher(serializerManager),
                new MultiplayerPatcher(),
                new NpcPatcher(),
                new SaveGamePatcher(serializerManager),
                new SerializationPatcher(),
                new UtilityPatcher(),
                new HoeDirtPatcher()
            );
            /*
            var ps = typeof(NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>).GetProperties();
            MethodBase m = null;
            foreach (var p in ps)
            {
                if (p.GetIndexParameters() == null || p.GetIndexParameters().Length == 0)
                    continue;
                if (p.GetSetMethod() == null)
                    continue;
                m = p.GetSetMethod();
                break;
            }
            Harmony.Patch(m,
                prefix: new HarmonyMethod(typeof(Fix1_5NetCodeBugPatch).GetMethod("Prefix", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)),
                postfix: new HarmonyMethod(typeof(Fix1_5NetCodeBugPatch).GetMethod("Postfix", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)));
            */
            Harmony.PatchAll();
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return new Api();
        }

        private static void DamageFarmerEventCommand(Event evt, GameLocation loc, GameTime time, string[] args)
        {
            Game1.eventUp = false;
            try
            {
                evt.farmer.takeDamage(int.Parse(args[1]), false, null);
            }
            finally
            {
                Game1.eventUp = true;
                evt.CurrentCommand++;
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
            // Set up skills in GameLaunched to allow ModRegistry to be used here.
            Skills.Init(this.Helper.Events);

            var configMenu = this.Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => this.Config = new Configuration(),
                    save: () => this.Helper.WriteConfig(this.Config)
                );
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: I18n.Config_CustomSkillPage_Name,
                    tooltip: I18n.Config_CustomSkillPage_Tooltip,
                    getValue: () => this.Config.CustomSkillPage,
                    setValue: value => this.Config.CustomSkillPage = value
                );
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: I18n.Config_SupportAllProfessionsMod_Name,
                    tooltip: I18n.Config_SupportAllProfessionsMod_Tooltip,
                    getValue: () => this.Config.SupportAllProfessionsMod,
                    setValue: value => this.Config.SupportAllProfessionsMod = value
                );
            }

            var entoaroxFramework = this.Helper.ModRegistry.GetApi<IEntoaroxFrameworkApi>("Entoarox.EntoaroxFramework");
            if (entoaroxFramework != null)
            {
                Log.Info("Telling EntoaroxFramework to let us handle the serializer");
                entoaroxFramework.HoistSerializerOwnership();
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.UpdateTicked"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // disable serializer if not used
            if (this.IsFirstTick && SpaceCore.ModTypes.Count == 0)
            {
                this.IsFirstTick = false;

                Log.Info("Disabling serializer patches (no mods using serializer API)");
                foreach (var method in SaveGamePatcher.GetSaveEnumeratorMethods())
                    this.Harmony.Unpatch(method, PatchHelper.RequireMethod<SaveGamePatcher>(nameof(SaveGamePatcher.Transpile_GetSaveEnumerator)));
                foreach (var method in SaveGamePatcher.GetLoadEnumeratorMethods())
                    this.Harmony.Unpatch(method, PatchHelper.RequireMethod<SaveGamePatcher>(nameof(SaveGamePatcher.Transpile_GetLoadEnumerator)));
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                this.LegacyDataMigrator.OnSaveLoaded();
            }
            catch (Exception ex)
            {
                Log.Warn($"Exception migrating legacy save data: {ex}");
            }
        }

        private void OnSaving( object sender, SavingEventArgs e )
        {
            // This had to be moved to a harmony patch to fix an issue from saving in a custom location context location
            /*
            if ( Game1.IsMasterGame )
            {
                var lws = SaveGame.GetSerializer( typeof( LocationWeather ) );
                Dictionary<int, string> customLocWeathers = new();
                foreach ( int context in Game1.netWorldState.Value.LocationWeather.Keys )
                {
                    if ( !Enum.IsDefined( ( GameLocation.LocationContext ) context ) )
                    {
                        SpaceShared.Log.Debug( "doing ctx " + context );
                        using MemoryStream ms = new();
                        lws.Serialize( ms, Game1.netWorldState.Value.LocationWeather[ context ] );
                        customLocWeathers.Add( context, Encoding.ASCII.GetString( ms.ToArray() ) );
                    }
                }
                foreach ( int key in customLocWeathers.Keys )
                    Game1.netWorldState.Value.LocationWeather.Remove( key );
                Helper.Data.WriteSaveData( "CustomLocationWeathers", customLocWeathers );
            }
            */
        }

        /// <inheritdoc cref="IDisplayEvents.MenuChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is StardewValley.Menus.ForgeMenu)
                Game1.activeClickableMenu = new NewForgeMenu();
        }
    }
}

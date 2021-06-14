using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using Spacechase.Shared.Harmony;
using SpaceCore.Framework;
using SpaceCore.Patches;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace SpaceCore
{
    internal class SpaceCore : Mod
    {
        public Configuration Config { get; set; }
        internal static SpaceCore Instance;
        internal static IReflectionHelper Reflection;
        private HarmonyInstance Harmony;

        internal static List<Type> ModTypes = new();
        internal static Queue<IDisposable> TextureDisposalQueue = new();
        private IDisposable NextToDispose = null;

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
            helper.Events.GameLoop.OneSecondUpdateTicked += this.OnOneSecondUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

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
            TileSheetExtensions.UpdateReferences(this.Config.DisposeOldTextures);
            if (this.TickCount++ == 0 && SpaceCore.ModTypes.Count == 0)
            {
                Log.Info("Disabling serializer patches (no mods using serializer API)");
                foreach (var meth in SaveGamePatcher.GetSaveEnumeratorMethods())
                    this.Harmony.Unpatch(meth, PatchHelper.RequireMethod<SaveGamePatcher>(nameof(SaveGamePatcher.Transpile_GetSaveEnumerator)));
                foreach (var meth in SaveGamePatcher.GetLoadEnumeratorMethods())
                    this.Harmony.Unpatch(meth, PatchHelper.RequireMethod<SaveGamePatcher>(nameof(SaveGamePatcher.Transpile_GetLoadEnumerator)));
            }
        }

        private void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e) {
            if (this.NextToDispose != null)
                {
                this.NextToDispose.Dispose();
                this.NextToDispose = null;
                }
            if (this.Config.DisposeOldTextures)
                {
                // TryDequeue not available on net452
                if (TextureDisposalQueue.Count != 0)
                    // Dispose exactly one item. We don't want too-early disposal.
                    this.NextToDispose = TextureDisposalQueue.Dequeue();
                }
            }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // delete legacy data
            if (Context.IsMainPlayer)
            {
                this.Helper.Data.WriteSaveData("sleepy-eye", null as object);

                FileInfo legacyFile = new FileInfo(Path.Combine(Constants.CurrentSavePath, "sleepy-eye.json"));
                if (legacyFile.Exists)
                    legacyFile.Delete();
            }
        }
    }
}

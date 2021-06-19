using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Harmony;
using SpaceCore.Framework;
using SpaceCore.Patches;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace SpaceCore
{
    internal class SpaceCore : Mod
    {
        public Configuration Config { get; set; }
        internal static SpaceCore Instance;
        internal static IReflectionHelper Reflection;
        private HarmonyInstance Harmony;

        /// <summary>Whether the current update tick is the first one raised by SMAPI.</summary>
        private bool IsFirstTick;

        internal static List<Type> ModTypes = new();

        /// <summary>A queue of textures to dispose, with the <see cref="Game1.ticks"/> value when they were queued.</summary>
        private readonly Queue<KeyValuePair<Texture2D, int>> TextureDisposalQueue = new();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            SpaceCore.Instance = this;
            SpaceCore.Reflection = helper.Reflection;
            Log.Monitor = this.Monitor;
            this.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
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

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // update tilesheet references
            foreach (Texture2D oldTexture in TileSheetExtensions.UpdateReferences())
            {
                if (this.Config.DisposeOldTextures)
                    this.TextureDisposalQueue.Enqueue(new(oldTexture, Game1.ticks));
            }

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

            // dispose old textures
            if (e.IsOneSecond)
            {
                while (this.TextureDisposalQueue.Count != 0)
                {
                    const int delayTicks = 60; // sixty ticks per second

                    var next = this.TextureDisposalQueue.Peek();
                    Texture2D asset = next.Key;
                    int queuedTicks = next.Value;

                    if (Game1.ticks - queuedTicks <= delayTicks)
                        break;

                    this.TextureDisposalQueue.Dequeue();
                    if (!asset.IsDisposed)
                        asset.Dispose();
                }
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

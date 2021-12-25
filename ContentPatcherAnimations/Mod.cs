using System;
using System.Collections;
using System.Reflection;
using ContentPatcherAnimations.Framework;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ContentPatcherAnimations
{
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The Content Patcher mod instance.</summary>
        private StardewModdingAPI.Mod ContentPatcher;

        /// <summary>The per-screen mod states.</summary>
        private readonly PerScreen<ScreenState> ScreenStateImpl = new(createNewState: () => new ScreenState());

        /// <summary>Simplifies access to private code.</summary>
        private IReflectionHelper Reflection => this.Helper.Reflection;


        /*********
        ** Accessors
        *********/
        /// <summary>The current mod state.</summary>
        internal ScreenState ScreenState => this.ScreenStateImpl.Value;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;

            helper.Content.AssetEditors.Add(new WatchForUpdatesAssetEditor(() => this.ScreenState.AnimatedPatches));

            helper.ConsoleCommands.Add("cpa", "...", this.OnCommand);
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>Handle a command received through the SMAPI console.</summary>
        /// <param name="name">The root command name.</param>
        /// <param name="args">The command arguments.</param>
        private void OnCommand(string name, string[] args)
        {
            if (args[0] == "reload")
                this.CollectPatches();
        }

        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            IModInfo modData = this.Helper.ModRegistry.Get("Pathoschild.ContentPatcher");
            this.ContentPatcher = this.GetPropertyValueManually<StardewModdingAPI.Mod>(modData, "Mod");
        }

        /// <inheritdoc cref="IGameLoopEvents.UpdateTicked"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            this.InitializeIfNeeded();
            this.UpdateAnimations();
        }

        /****
        ** Implementation
        ****/
        /// <summary>Get the underlying patches from Content Patcher if this is the first update tick for a screen.</summary>
        private void InitializeIfNeeded()
        {
            var state = this.ScreenState;

            if (state.RawPatches == null)
            {
                object screenManagerPerScreen = this.Reflection.GetField<object>(this.ContentPatcher, "ScreenManager").GetValue();
                object screenManager = this.GetPropertyValueManually<object>(screenManagerPerScreen, "Value");
                object patchManager = this.Reflection.GetProperty<object>(screenManager, "PatchManager").GetValue();
                state.RawPatches = this.Reflection.GetField<IEnumerable>(patchManager, "Patches").GetValue();

                this.CollectPatches();
            }
        }

        /// <summary>Update textures and animations if needed.</summary>
        private void UpdateAnimations()
        {
            var state = this.ScreenState;

            // update animation frames
            ++state.FrameCounter;
            Game1.graphics.GraphicsDevice.Textures[0] = null;
            foreach ((Patch config, PatchData patch) in state.AnimatedPatches)
            {
                patch.RefreshIfNeeded();

                if (!patch.IsActive || patch.Source == null || patch.Target == null)
                    continue;

                if (state.FrameCounter % config.AnimationFrameTime == 0)
                {
                    if (++patch.CurrentFrame >= config.AnimationFrameCount)
                        patch.CurrentFrame = 0;

                    Color[] pixels = patch.GetAnimationFrame(patch.CurrentFrame);
                    patch.Target.SetData(0, patch.ToArea, pixels, 0, pixels.Length);
                }
            }
        }

        /// <summary>Collect all patches from installed Content Patcher packs.</summary>
        private void CollectPatches()
        {
            var state = this.ScreenState;

            state.AnimatedPatches.Clear();
            foreach (var pack in this.ContentPatcher.Helper.ContentPacks.GetOwned())
            {
                var patches = pack.ReadJsonFile<PatchList>("content.json");
                if (patches?.Changes == null)
                    continue;

                foreach (var patch in patches.Changes)
                {
                    if (patch == null)
                        continue;

                    if (patch.AnimationFrameTime > 0 && patch.AnimationFrameCount > 0)
                    {
                        Log.Trace($"Loading animated patch from content pack {pack.Manifest.UniqueID}");
                        if (string.IsNullOrEmpty(patch.LogName))
                        {
                            Log.Error("Animated patches must specify a LogName!");
                            continue;
                        }

                        object targetPatch = null;
                        foreach (object cpPatch in state.RawPatches)
                        {
                            object path = this.Reflection.GetProperty<object>(cpPatch, "Path").GetValue();
                            if (path.ToString() == $"{pack.Manifest.Name} > {patch.LogName}")
                            {
                                targetPatch = cpPatch;
                                break;
                            }
                        }
                        if (targetPatch == null)
                        {
                            Log.Error($"Failed to find patch with name \"{patch.LogName}\"!?!?");
                            continue;
                        }

                        PatchData data = new PatchData(pack, patch.LogName, targetPatch, this.Reflection);

                        state.AnimatedPatches.Add(patch, data);
                    }
                }
            }
        }

        /// <summary>Manually get the value of a property using reflection.</summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="obj">The parent object whose property to get.</param>
        /// <param name="name">The property name.</param>
        /// <remarks>This is only needed when reflecting into SMAPI itself; otherwise we should use the more efficient <see cref="Reflection"/> helper.</remarks>
        private T GetPropertyValueManually<T>(object obj, string name)
        {
            try
            {
                // validate parent
                if (obj is null)
                    throw new InvalidOperationException($"Can't get the '{name}' property on a null object.");

                // get property
                PropertyInfo property = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (property == null)
                    throw new InvalidOperationException($"Can't find the '{name}' property on the {obj.GetType().FullName} type.");

                // get & cast value
                return (T)property.GetValue(obj);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed retrieving the '{name}' property from the {obj?.GetType().FullName} type.", ex);
            }
        }
    }
}

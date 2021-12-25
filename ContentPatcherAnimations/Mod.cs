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
            helper.Events.GameLoop.SaveCreated += this.OnSaveCreated;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;

            helper.Content.AssetEditors.Add(new WatchForUpdatesAssetEditor(
                getAnimatedPatches: () => this.ScreenState.AnimatedPatches,
                getFindTargetsQueue: () => this.ScreenState.FindTargetsQueue,
                reflection: helper.Reflection
            ));

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

        /// <inheritdoc cref="IGameLoopEvents.SaveCreated"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveCreated(object sender, SaveCreatedEventArgs e)
        {
            this.QueueUpdateTargets();
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.QueueUpdateTargets();
        }

        /// <inheritdoc cref="IGameLoopEvents.DayStarted"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            this.QueueUpdateTargets();
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
        /// <summary>Notify all screens to update the texture target values on their next update tick.</summary>
        void QueueUpdateTargets()
        {
            foreach (var screen in this.ScreenStateImpl.GetActiveValues())
                screen.Value.FindTargetsCounter = 1;
        }

        /// <summary>Get the underlying patches from Content Patcher if this is the first update tick for a screen.</summary>
        private void InitializeIfNeeded()
        {
            var state = this.ScreenState;

            if (state.CpPatches == null)
            {
                object screenManagerPerScreen = this.Reflection.GetField<object>(this.ContentPatcher, "ScreenManager").GetValue();
                object screenManager = this.GetPropertyValueManually<object>(screenManagerPerScreen, "Value");
                object patchManager = this.Reflection.GetProperty<object>(screenManager, "PatchManager").GetValue();
                state.CpPatches = this.Reflection.GetField<IEnumerable>(patchManager, "Patches").GetValue();

                this.CollectPatches();
            }
        }

        /// <summary>Update textures and animations if needed.</summary>
        private void UpdateAnimations()
        {
            var state = this.ScreenState;

            // update target textures
            if (state.FindTargetsCounter > 0 && --state.FindTargetsCounter == 0)
                this.UpdateTargetTextures();
            while (state.FindTargetsQueue.Count > 0)
            {
                var patch = state.FindTargetsQueue.Dequeue();
                this.UpdateTargetTextures(patch);
            }

            // update animation frames
            ++state.FrameCounter;
            Game1.graphics.GraphicsDevice.Textures[0] = null;
            foreach (var patch in state.AnimatedPatches)
            {
                if (!patch.Value.IsActive.Invoke() || patch.Value.Source == null || patch.Value.Target == null)
                    continue;

                try
                {
                    if (state.FrameCounter % patch.Key.AnimationFrameTime == 0)
                    {
                        if (++patch.Value.CurrentFrame >= patch.Key.AnimationFrameCount)
                            patch.Value.CurrentFrame = 0;

                        var sourceRect = patch.Value.FromAreaFunc.Invoke();
                        sourceRect.X += patch.Value.CurrentFrame * sourceRect.Width;
                        var targetRect = patch.Value.ToAreaFunc.Invoke();
                        if (targetRect == Rectangle.Empty)
                            targetRect = new Rectangle(0, 0, sourceRect.Width, sourceRect.Height);
                        var cols = new Color[sourceRect.Width * sourceRect.Height];
                        patch.Value.Source.GetData(0, sourceRect, cols, 0, cols.Length);
                        patch.Value.Target.SetData(0, targetRect, cols, 0, cols.Length);
                    }
                }
                catch
                {
                    // No idea why this happens, hack fix
                    patch.Value.ClearTarget();
                    state.FindTargetsQueue.Enqueue(patch.Key);
                }
            }
        }

        /// <summary>Reload all target textures.</summary>
        private void UpdateTargetTextures()
        {
            var state = this.ScreenState;

            foreach (var patch in state.AnimatedPatches)
            {
                try
                {
                    if (!patch.Value.IsActive.Invoke())
                        continue;

                    patch.Value.Reload();
                }
                catch (Exception e)
                {
                    Log.Trace($"Exception loading {patch.Key.LogName} textures, delaying to try again next frame: {e}");
                    state.FindTargetsQueue.Enqueue(patch.Key);
                }
            }
        }

        /// <summary>Reload all target textures for a given patch.</summary>
        private void UpdateTargetTextures(Patch key)
        {
            var state = this.ScreenState;

            try
            {
                var patch = state.AnimatedPatches[key];
                if (!patch.IsActive())
                    return;

                patch.Reload();
            }
            catch (Exception e)
            {
                Log.Error($"Exception loading {key.LogName} textures: {e}");
            }
        }

        /// <summary>Collect all patches from installed Content Patcher packs.</summary>
        private void CollectPatches()
        {
            var state = this.ScreenState;

            state.AnimatedPatches.Clear();
            state.FindTargetsQueue.Clear();
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
                        foreach (object cpPatch in state.CpPatches)
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

                        PatchData data = PatchData.ReadPatchData(pack, targetPatch, this.Reflection);

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

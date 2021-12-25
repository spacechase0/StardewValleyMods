using System;
using System.Collections;
using System.Reflection;
using ContentPatcherAnimations.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

            this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            this.Helper.Events.GameLoop.SaveCreated += this.OnSaveCreated;
            this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;

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

        /// <summary>Initialize the underlying state and update animations if needed.</summary>
        private void UpdateAnimations()
        {
            // initialize if needed
            if (this.ContentPatcher == null)
            {
                var modData = this.Helper.ModRegistry.Get("Pathoschild.ContentPatcher");
                this.ContentPatcher = this.GetPropertyValueManually<StardewModdingAPI.Mod>(modData, "Mod");
            }

            if (this.ScreenState.CpPatches == null)
            {
                object screenManagerPerScreen = this.Reflection.GetField<object>(this.ContentPatcher, "ScreenManager").GetValue();
                object screenManager = this.GetPropertyValueManually<object>(screenManagerPerScreen, "Value");
                object patchManager = this.Reflection.GetProperty<object>(screenManager, "PatchManager").GetValue();
                this.ScreenStateImpl.Value.CpPatches = this.Reflection.GetField<IEnumerable>(patchManager, "Patches").GetValue();

                this.CollectPatches();
            }

            // update target textures
            if (this.ScreenState.FindTargetsCounter > 0 && --this.ScreenState.FindTargetsCounter == 0)
                this.UpdateTargetTextures();
            while (this.ScreenState.FindTargetsQueue.Count > 0)
            {
                var patch = this.ScreenState.FindTargetsQueue.Dequeue();
                this.UpdateTargetTextures(patch);
            }

            // update animation frames
            ++this.ScreenState.FrameCounter;
            Game1.graphics.GraphicsDevice.Textures[0] = null;
            foreach (var patch in this.ScreenState.AnimatedPatches)
            {
                if (!patch.Value.IsActive.Invoke() || patch.Value.Source == null || patch.Value.Target == null)
                    continue;

                try
                {
                    if (this.ScreenState.FrameCounter % patch.Key.AnimationFrameTime == 0)
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
                    patch.Value.Target = null;
                    this.ScreenState.FindTargetsQueue.Enqueue(patch.Key);
                }
            }
        }

        /// <summary>Reload all target textures.</summary>
        private void UpdateTargetTextures()
        {
            foreach (var patch in this.ScreenState.AnimatedPatches)
            {
                try
                {
                    if (!patch.Value.IsActive.Invoke())
                        continue;

                    patch.Value.Source = patch.Value.SourceFunc();
                    patch.Value.Target = patch.Value.TargetFunc();
                }
                catch (Exception e)
                {
                    Log.Trace($"Exception loading {patch.Key.LogName} textures, delaying to try again next frame: {e}");
                    this.ScreenState.FindTargetsQueue.Enqueue(patch.Key);
                }
            }
        }

        /// <summary>Reload all target textures for a given patch.</summary>
        private void UpdateTargetTextures(Patch key)
        {
            try
            {
                var patch = this.ScreenState.AnimatedPatches[key];
                if (!patch.IsActive())
                    return;

                patch.Source = patch.SourceFunc();
                patch.Target = patch.TargetFunc();
            }
            catch (Exception e)
            {
                Log.Error($"Exception loading {key.LogName} textures: {e}");
            }
        }

        /// <summary>Collect all patches from installed Content Patcher packs.</summary>
        private void CollectPatches()
        {
            this.ScreenState.AnimatedPatches.Clear();
            this.ScreenState.FindTargetsQueue.Clear();
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

                        PatchData data = new PatchData();

                        object targetPatch = null;
                        foreach (object cpPatch in this.ScreenState.CpPatches)
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
                        var appliedProp = this.Reflection.GetProperty<bool>(targetPatch, "IsApplied");
                        var sourceProp = this.Reflection.GetProperty<string>(targetPatch, "FromAsset");
                        var targetProp = this.Reflection.GetProperty<string>(targetPatch, "TargetAsset");

                        data.PatchObj = targetPatch;
                        data.IsActive = () => appliedProp.GetValue();
                        data.SourceFunc = () => pack.LoadAsset<Texture2D>(sourceProp.GetValue());
                        data.TargetFunc = () => this.FindTargetTexture(targetProp.GetValue());
                        data.FromAreaFunc = () => this.GetRectangleFromPatch(targetPatch, "FromArea");
                        data.ToAreaFunc = () =>
                        {
                            var fromArea = data.FromAreaFunc();
                            return this.GetRectangleFromPatch(targetPatch, "ToArea", new Rectangle(0, 0, fromArea.Width, fromArea.Height));
                        };

                        this.ScreenState.AnimatedPatches.Add(patch, data);
                    }
                }
            }
        }

        /// <summary>Get the texture for a given asset name.</summary>
        /// <param name="target">The asset name to match.</param>
        private Texture2D FindTargetTexture(string target)
        {
            if (this.Helper.Content.NormalizeAssetName(target) == this.Helper.Content.NormalizeAssetName("TileSheets\\tools"))
                return this.Reflection.GetField<Texture2D>(typeof(Game1), "_toolSpriteSheet").GetValue();

            var tex = Game1.content.Load<Texture2D>(target);
            if (tex.GetType().Name == "ScaledTexture2D")
            {
                Log.Trace($"Found ScaledTexture2D from PyTK: {target}");
                tex = this.Reflection.GetProperty<Texture2D>(tex, "STexture").GetValue();
            }
            return tex;
        }

        /// <summary>Get the source rectangle for a Content Patcher patch.</summary>
        /// <param name="targetPatch">The Content Patcher patch.</param>
        /// <param name="rectName">The rectangle field name.</param>
        /// <param name="defaultTo">The default rectangle value if the field isn't defined.</param>
        private Rectangle GetRectangleFromPatch(object targetPatch, string rectName, Rectangle defaultTo = default)
        {
            object tokenRect = this.Reflection.GetField<object>(targetPatch, rectName).GetValue();
            if (tokenRect == null)
                return defaultTo;

            object[] args = { null, null }; // out Rectangle rectangle, out string error
            return this.Reflection.GetMethod(tokenRect, "TryGetRectangle").Invoke<bool>(args)
                ? (Rectangle)args[0]
                : Rectangle.Empty;
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

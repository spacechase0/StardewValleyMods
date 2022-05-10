using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using ContentPatcherAnimations.Framework;
using ContentPatcherAnimations.Patches;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
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
        /// <summary>
        /// Mod instance for 
        /// </summary>
        internal static Mod instance;

        /// <summary>The Content Patcher mod instance.</summary>
        private StardewModdingAPI.Mod ContentPatcher;

        /// <summary>The per-screen mod states.</summary>
        private readonly PerScreen<ScreenState> ScreenStateImpl = new(createNewState: () => new ScreenState());

        /// <summary>Simplifies access to private code.</summary>
        private IReflectionHelper Reflection => this.Helper.Reflection;

        /// <summary>The maximum number of ticks animations should continue running after the texture was last drawn.</summary>
        private const int MaxTicksSinceDrawn = 5 * 60; // 5 seconds

        /// <summary>The number of ticks between each cleanup of expired tracked asset names.</summary>
        private const int ExpiryTicks = 5 * 60 * 60; // 5 minutes

        /// <summary>The current mod state.</summary>
        private ScreenState ScreenState => this.ScreenStateImpl.Value;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            LocalizedContentManager.OnLanguageChange += this.OnLocaleChanged;

            helper.Content.AssetEditors.Add(new WatchForUpdatesAssetEditor(() => this.ScreenState.AnimatedPatches));

            helper.ConsoleCommands.Add("cpa", "...", this.OnCommand);

            HarmonyPatcher.Apply(this,
                new SpriteBatchPatcher(() => this.ScreenState.AssetDrawTracker)
            );
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
            switch (args.FirstOrDefault()?.ToLower())
            {
                case "reload":
                    this.CollectPatches();
                    Log.Info("Reloaded all patches.");
                    break;

                case "summary":
                    this.PrintSummary();
                    break;

                default:
                    Log.Info("Usage:\n\ncpa reload\nReloads all patches.\n\ncpa summary\nPrints a summary of the registered animations.");
                    break;
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            IModInfo modData = this.Helper.ModRegistry.Get("Pathoschild.ContentPatcher");
            this.ContentPatcher = this.GetPropertyValueManually<StardewModdingAPI.Mod>(modData, "Mod");
        }

        /// <summary>Raised after the game's selected language changes.</summary>
        /// <param name="code">The new language code.</param>
        private void OnLocaleChanged(LocalizedContentManager.LanguageCode code)
        {
            this.ScreenState.OnLocaleChanged(code);
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

            // clear expired draw tracking
            if (state.FrameCounter % Mod.ExpiryTicks == 0)
                state.AssetDrawTracker.ForgetExpired(Mod.MaxTicksSinceDrawn * 5); // buffer to avoid recreating tracking data unnecessarily

            // update animation frames
            ++state.FrameCounter;
            Game1.graphics.GraphicsDevice.Textures[0] = null;
            foreach ((Patch config, PatchData patch) in state.AnimatedPatches)
            {
                patch.RefreshIfNeeded();

                if (state.FrameCounter % config.AnimationFrameTime == 0 && this.ShouldAnimate(state, patch))
                {
                    if (++patch.CurrentFrame >= config.AnimationFrameCount)
                        patch.CurrentFrame = 0;
                    Color[] pixels = patch.GetAnimationFrame(patch.CurrentFrame);
                    patch.Target.SetData(0, patch.ToArea, pixels, 0, pixels.Length);
                }
            }
        }

        /// <summary>Get whether a patch should be animated.</summary>
        /// <param name="state">The screen state to check.</param>
        /// <param name="patch">The patch to check.</param>
        private bool ShouldAnimate(ScreenState state, PatchData patch)
        {
            return
                patch.IsActive
                && patch.Source != null
                && patch.Target != null
                && state.AssetDrawTracker.WasDrawnWithin(patch.TargetName, patch.ToArea, Mod.MaxTicksSinceDrawn);
        }

        /// <summary>Get a human-readable reason a patch isn't being automated.</summary>
        /// <param name="state">The screen state to check.</param>
        /// <param name="patch">The patch to check.</param>
        private string GetReasonPaused(ScreenState state, PatchData patch)
        {
            if (!patch.IsReady)
                return "Content Patcher patch not ready";

            if (!patch.IsActive)
                return "Content Patcher patch not applied";

            if (patch.Source == null || patch.Target == null)
                return "textures couldn't be loaded";

            if (!state.AssetDrawTracker.WasDrawnWithin(patch.TargetName, patch.ToArea, Mod.MaxTicksSinceDrawn))
                return "patched area isn't visible on screen";

            return "unknown";
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

        /// <summary>Print a summary of the current animations to the console.</summary>
        private void PrintSummary()
        {
            var state = this.ScreenState;

            StringBuilder report = new();

            var patches = state.AnimatedPatches;
            var animating = patches.ToLookup(p => this.ShouldAnimate(state, p.Value));

            // general data
            report.AppendLine();
            report.AppendLine("##############################");
            report.AppendLine($"## General stats{(Context.IsSplitScreen ? $" for screen {Context.ScreenId}" : "")}");
            report.AppendLine("##############################");
            report.AppendLine("   Animated patches:");
            report.AppendLine($"    - {patches.Count} total");
            report.AppendLine($"    - {patches.Values.Count(p => p.IsReady && p.IsActive)} applied");
            report.AppendLine($"    - {animating[true].Count()} being animated");
            report.AppendLine();
            report.AppendLine();

            // active animations
            report.AppendLine("##############################");
            report.AppendLine("## Active animations");
            report.AppendLine("##############################");
            {
                var active = animating[true]
                    .GroupBy(p => p.Value.ContentPack.Manifest.Name)
                    .OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (active.Any())
                {
                    foreach (var patchGroup in active)
                    {
                        report.AppendLine($"{patchGroup.Key}:");

                        int targetWidth = Math.Max(patchGroup.Max(p => p.Value.TargetName.ToString().Length), "target asset".Length);
                        int logNameWidth = Math.Max(patchGroup.Max(p => p.Key.LogName.Length), "patch name".Length);
                        int frameWidth = Math.Max(patchGroup.Max(p => $"{p.Value.CurrentFrame + 1} of {p.Key.AnimationFrameCount}".Length), "cur frame".Length);

                        report.AppendLine($"   {"target asset".PadRight(targetWidth)} | {"patch name".PadRight(logNameWidth)} | {"cur frame".PadRight(frameWidth)}");
                        report.AppendLine($"   {"".PadRight(targetWidth, '-')} | {"".PadRight(logNameWidth, '-')} | {"".PadRight(frameWidth, '-')}");
                        foreach (var patch in patchGroup.OrderBy(p => p.Value.TargetName.ToString(), StringComparer.OrdinalIgnoreCase))
                            report.AppendLine($"   {patch.Value.TargetName.ToString().PadRight(targetWidth)} | {patch.Key.LogName.PadRight(logNameWidth)} | {patch.Value.CurrentFrame + 1} of {patch.Key.AnimationFrameCount}");
                        report.AppendLine();
                    }
                }
                else
                {
                    report.AppendLine("There are no active animations.");
                    report.AppendLine();
                }
            }
            report.AppendLine();

            // paused animations
            report.AppendLine("##############################");
            report.AppendLine("## Paused animations");
            report.AppendLine("##############################");
            {
                var paused = animating[false]
                    .GroupBy(p => p.Value.ContentPack.Manifest.Name)
                    .OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (paused.Any())
                {
                    foreach (var patchGroup in paused)
                    {
                        report.AppendLine($"{patchGroup.Key}:");

                        int logNameWidth = Math.Max(patchGroup.Max(p => p.Key.LogName.Length), "patch name".Length);
                        int reasonWidth = "reason paused".Length;

                        report.AppendLine($"   {"patch name".PadRight(logNameWidth)} | reason paused");
                        report.AppendLine($"   {"".PadRight(logNameWidth, '-')} | {"".PadRight(reasonWidth, '-')}");
                        foreach (var patch in patchGroup.OrderBy(p => p.Value.TargetName.ToString(), StringComparer.OrdinalIgnoreCase))
                            report.AppendLine($"   {patch.Key.LogName.PadRight(logNameWidth)} | {this.GetReasonPaused(state, patch.Value)}");
                        report.AppendLine();
                    }
                }
                else
                {
                    report.AppendLine("There are no paused animations.");
                    report.AppendLine();
                }
            }

            Log.Info(report.ToString());
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

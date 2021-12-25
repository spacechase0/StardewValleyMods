using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace ContentPatcherAnimations.Framework
{
    // TODO: Optimize this
    /// <summary>An asset editor which detects when an animated texture changes.</summary>
    internal class WatchForUpdatesAssetEditor : IAssetEditor
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get the patch and animation data for loaded patches.</summary>
        private readonly Func<IDictionary<Patch, PatchData>> GetAnimatedPatches;

        /// <summary>Get the queue of patches whose target textures to reload.</summary>
        private readonly Func<Queue<Patch>> GetFindTargetsQueue;

        /// <summary>Simplifies access to private code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="getAnimatedPatches">Get the patch and animation data for loaded patches.</param>
        /// <param name="getFindTargetsQueue">Get the queue of patches whose target textures to reload.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public WatchForUpdatesAssetEditor(Func<IDictionary<Patch, PatchData>> getAnimatedPatches, Func<Queue<Patch>> getFindTargetsQueue, IReflectionHelper reflection)
        {
            this.GetAnimatedPatches = getAnimatedPatches;
            this.GetFindTargetsQueue = getFindTargetsQueue;
            this.Reflection = reflection;
        }

        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            var animatedPatches = this.GetAnimatedPatches();

            foreach (PatchData patch in animatedPatches.Values)
            {
                string target = this.Reflection.GetProperty<string>(patch.PatchObj, "TargetAsset").GetValue();
                if (!string.IsNullOrWhiteSpace(target) && asset.AssetNameEquals(target))
                    return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            var animatedPatches = this.GetAnimatedPatches();
            var queue = this.GetFindTargetsQueue();

            foreach ((Patch key, PatchData patch) in animatedPatches)
            {
                string target = this.Reflection.GetProperty<string>(patch.PatchObj, "TargetAsset").GetValue();
                if (!string.IsNullOrWhiteSpace(target) && asset.AssetNameEquals(target))
                    queue.Enqueue(key);
            }
        }
    }
}

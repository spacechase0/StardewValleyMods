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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="getAnimatedPatches">Get the patch and animation data for loaded patches.</param>
        public WatchForUpdatesAssetEditor(Func<IDictionary<Patch, PatchData>> getAnimatedPatches)
        {
            this.GetAnimatedPatches = getAnimatedPatches;
        }

        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            var animatedPatches = this.GetAnimatedPatches();

            foreach (PatchData patch in animatedPatches.Values)
            {
                if (patch.TargetName != null && asset.Name.IsEquivalentTo(patch.TargetName))
                    return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            var animatedPatches = this.GetAnimatedPatches();

            foreach (PatchData patch in animatedPatches.Values)
            {
                if (patch.TargetName != null && asset.Name.IsEquivalentTo(patch.TargetName))
                    patch.ForceNextRefresh = true;
            }
        }
    }
}

using System;
using StardewModdingAPI;

namespace ContentPatcherAnimations.Framework
{
    // TODO: Optimize this
    /// <summary>An asset editor which detects when an animated texture changes.</summary>
    internal class WatchForUpdatesAssetEditor : IAssetEditor
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (Mod.Instance.ScreenState == null)
                return false;

            foreach (var patchEntry in Mod.Instance.ScreenState.AnimatedPatches)
            {
                object patch = patchEntry.Value.PatchObj;
                string target = Mod.Instance.Helper.Reflection.GetProperty<string>(patch, "TargetAsset").GetValue();
                if (!string.IsNullOrWhiteSpace(target) && asset.AssetNameEquals(target))
                    return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            if (Mod.Instance.ScreenState == null)
                return;

            foreach (var patchEntry in Mod.Instance.ScreenState.AnimatedPatches)
            {
                object patch = patchEntry.Value.PatchObj;
                string target = Mod.Instance.Helper.Reflection.GetProperty<string>(patch, "TargetAsset").GetValue();
                if (!string.IsNullOrWhiteSpace(target) && asset.AssetNameEquals(target))
                {
                    Mod.Instance.ScreenState.FindTargetsQueue.Enqueue(patchEntry.Key);
                }
            }
        }
    }
}

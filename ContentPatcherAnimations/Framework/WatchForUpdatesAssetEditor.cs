using StardewModdingAPI;

namespace ContentPatcherAnimations.Framework
{
    // TODO: Optimize this
    internal class WatchForUpdatesAssetEditor : IAssetEditor
    {
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

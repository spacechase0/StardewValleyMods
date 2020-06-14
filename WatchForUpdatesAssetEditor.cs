using StardewModdingAPI;

namespace ContentPatcherAnimations
{
    internal class WatchForUpdatesAssetEditor : IAssetEditor
    {
        private readonly Patch patch;
        private readonly PatchData data;
        private readonly string assetPath;

        public WatchForUpdatesAssetEditor( Patch patch, PatchData data, string asset )
        {
            this.patch = patch;
            this.data = data;
            this.assetPath = asset;
        }

        public bool CanEdit<T>( IAssetInfo asset )
        {
            return asset.AssetNameEquals( assetPath );
        }

        public void Edit<T>( IAssetData asset )
        {
            data.Target = data.TargetFunc();
        }
    }
}
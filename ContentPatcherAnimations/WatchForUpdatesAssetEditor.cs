using StardewModdingAPI;
using System;

namespace ContentPatcherAnimations
{
    // TODO: Optimize this
    internal class WatchForUpdatesAssetEditor : IAssetEditor
    {
        public WatchForUpdatesAssetEditor()
        {
        }

        public bool CanEdit<T>( IAssetInfo asset )
        {
            if ( Mod.instance.ScreenState == null )
                return false;

            foreach ( var patchEntry in Mod.instance.ScreenState.animatedPatches )
            {
                var patch = patchEntry.Value.patchObj;
                var target = Mod.instance.Helper.Reflection.GetProperty<string>( patch, "TargetAsset" ).GetValue();
                if ( !string.IsNullOrWhiteSpace( target ) && asset.AssetNameEquals( target ) )
                    return true;
            }
            return false;
        }

        public void Edit<T>( IAssetData asset )
        {
            if ( Mod.instance.ScreenState == null )
                return;

            foreach ( var patchEntry in Mod.instance.ScreenState.animatedPatches )
            {
                var patch = patchEntry.Value.patchObj;
                var target = Mod.instance.Helper.Reflection.GetProperty<string>( patch, "TargetAsset" ).GetValue();
                if ( !string.IsNullOrWhiteSpace( target ) && asset.AssetNameEquals( target ) )
                {
                    Mod.instance.ScreenState.findTargetsQueue.Enqueue( patchEntry.Key );
                }
            }
        }
    }
}
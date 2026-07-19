using System;
using StardewModdingAPI.Utilities;

#if DEBUG
namespace SpaceShared.ContentAssets;

internal class AssetChangedWatcher
{
    private readonly IDevAssetsSource Mod;
    private readonly BaseAssetInstances Assets;

    private FileSystemWatcher watcher;
    private Dictionary<string, TimeSpan> deferredTriggers = new();

    public AssetChangedWatcher(IDevAssetsSource mod, BaseAssetInstances assets)
    {
        Mod = mod;
        Assets = assets;

        watcher = new(mod.DevAssetsFolder);
        watcher.Filter = string.Empty;
        watcher.IncludeSubdirectories = true;
        watcher.Changed += Watcher_Changed;
        watcher.Renamed += Watcher_Renamed;
        watcher.EnableRaisingEvents = true;
    }

    public void Update(TimeSpan elapsed)
    {
        foreach (var path in deferredTriggers.Keys.ToArray())
        {
            deferredTriggers[path] -= elapsed;
            if (deferredTriggers[path] <= TimeSpan.Zero)
            {
                deferredTriggers.Remove(path);
                Assets.Reload(path);
            }
        }
    }

    private void Watcher_Changed(object sender, FileSystemEventArgs e)
    {
        string path = PathUtilities.NormalizePath(Path.GetRelativePath(watcher.Path, e.FullPath));
        if (!File.Exists(e.FullPath))
            return;

        deferredTriggers[path] = TimeSpan.FromSeconds(0.1f);
    }

    private void Watcher_Renamed(object sender, RenamedEventArgs e)
    {
        string path = PathUtilities.NormalizePath(Path.GetRelativePath(watcher.Path, e.FullPath));
        if (!File.Exists(e.FullPath))
            return;

        deferredTriggers[path] = TimeSpan.FromSeconds(0.1f);
    }
}
#endif

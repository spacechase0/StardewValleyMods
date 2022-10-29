using System;

using HarmonyLib;

using Spacechase.Shared.Patching;

using SpaceShared;

using StardewModdingAPI;

using StardewValley.Objects;

namespace JsonAssets.Patches;
internal class BootPatcher : BasePatcher
{

    public override void Apply(Harmony harmony, IMonitor monitor)
    {
        harmony.Patch(
            original: this.RequireMethod<Boots>("loadDisplayFields"),
            finalizer: this.GetHarmonyMethod(nameof(FinalizeBootDisplayFields))
            );
    }

#nullable enable
    private static Exception? FinalizeBootDisplayFields(Boots __instance, Exception __exception)
    {
        if (__exception is not null)
        {
            Log.Warn($"{__instance.indexInTileSheet} corresponds to a pair of boots not found in Data/Boots!");
        }
        return null;
    }
}

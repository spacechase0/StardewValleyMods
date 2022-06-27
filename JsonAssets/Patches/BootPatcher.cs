using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JsonAssets.Data;
using Netcode;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
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

    private static Exception? FinalizeBootDisplayFields(Boots __instance, Exception __exception)
    {
        if (__exception is not null)
        {
            Log.Warn($"{__instance.indexInTileSheet} corresponds to a pair of boots not found in Data/Boots!");
        }
        return null;
    }
}

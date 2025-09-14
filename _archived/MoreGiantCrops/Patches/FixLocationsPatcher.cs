using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using HarmonyLib;

using Microsoft.Xna.Framework;

using Spacechase.Shared.Patching;

using SpaceShared;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;

namespace MoreGiantCrops.Patches;

#warning - remove in 1.6

/// <summary>Applies Harmony patches to <see cref="GameLocation"/>.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
internal class FixLocationsPatcher : BasePatcher
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Apply(Harmony harmony, IMonitor monitor)
    {
        harmony.Patch(
            original: this.RequireMethod<GameLocation>(nameof(GameLocation.TransferDataFromSavedLocation)),
            postfix: this.GetHarmonyMethod(nameof(Postfix))
        );
    }

    private static void Postfix(GameLocation __instance, GameLocation l)
    {
        // game handles these two.
        if (__instance is IslandWest || __instance.Name.Equals("Farm", StringComparison.OrdinalIgnoreCase)
            || __instance.resourceClumps.Count >= l.resourceClumps.Count)
        {
            return;
        }

        // We need to avoid accidentally adding duplicates.
        // Keep track of occupied tiles here.
        HashSet<Vector2> prev = new(l.resourceClumps.Count);

        foreach (var clump in __instance.resourceClumps)
        {
            prev.Add(clump.tile.Value);
        }

        // restore previous giant crops.
        int count = 0;
        foreach (var clump in l.resourceClumps)
        {
            if (clump is GiantCrop crop && prev.Add(crop.tile.Value))
            {
                count++;
                __instance.resourceClumps.Add(crop);
            }
        }

        Mod.Instance.Monitor.Log($"Restored {count} giant crops at {__instance.NameOrUniqueName}");
    }
}

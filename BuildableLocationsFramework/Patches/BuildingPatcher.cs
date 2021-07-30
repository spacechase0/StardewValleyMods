using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;

namespace BuildableLocationsFramework.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Building"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class BuildingPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Building>(nameof(Building.updateInteriorWarps)),
                postfix: this.GetHarmonyMethod(nameof(After_UpdateInteriorWarps))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Building.updateInteriorWarps"/>.</summary>
        private static void After_UpdateInteriorWarps(Building __instance, GameLocation interior)
        {
            string targetName = Mod.FindOutdoorsOf(__instance)?.Name;
            if (targetName == null)
                return;

            interior ??= __instance.indoors.Value;
            if (interior == null)
                return;
            foreach (Warp warp in interior.warps)
            {
                warp.TargetName = targetName;
            }
        }
    }
}

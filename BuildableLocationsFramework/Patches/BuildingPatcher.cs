using System.Diagnostics.CodeAnalysis;
using Harmony;
using Spacechase.Shared.Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;

namespace BuildableLocationsFramework.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Building"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class BuildingPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
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
            string targetName = Mod.findOutdoorsOf(__instance)?.Name;
            if (targetName == null)
                return;

            if (interior == null)
                interior = __instance.indoors.Value;
            if (interior == null)
                return;
            foreach (Warp warp in interior.warps)
            {
                warp.TargetName = targetName;
            }
        }
    }
}
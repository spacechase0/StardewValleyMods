using System.Diagnostics.CodeAnalysis;
using DynamicGameAssets.Game;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Fence"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class FencePatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Fence>(nameof(Fence.CanRepairWithThisItem)),
                prefix: this.GetHarmonyMethod(nameof(Before_CanRepairWithThisItem))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Fence.CanRepairWithThisItem"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_CanRepairWithThisItem(Fence __instance, Item item, ref bool __result)
        {
            if (__instance is CustomFence fence)
            {
                __result = fence.CanRepairWithThisItem(item);
                return false;
            }

            return true;
        }
    }
}

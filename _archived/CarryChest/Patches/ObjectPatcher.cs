using System.Diagnostics.CodeAnalysis;
using CarryChest.Framework;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace CarryChest.Patches
{
    /// <summary>Applies Harmony patches to <see cref="SObject"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ObjectPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.getDescription)),
                postfix: this.GetHarmonyMethod(nameof(After_GetDescription))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="SObject.getDescription"/>.</summary>
        private static void After_GetDescription(SObject __instance, ref string __result)
        {
            if (ChestHelper.IsSupported(__instance))
            {
                var chest = __instance as Chest;
                __result += "\n" + $"Contains {chest?.items?.Count ?? 0} items.";
            }
        }
    }
}

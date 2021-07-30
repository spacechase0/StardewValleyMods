using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;

namespace CapstoneProfessions.Patches
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
                original: this.RequireMethod<SObject>("getPriceAfterMultipliers"),
                postfix: this.GetHarmonyMethod(nameof(After_GetPriceAfterMultipliers))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after <see cref="SObject.getPriceAfterMultipliers"/>.</summary>
        private static void After_GetPriceAfterMultipliers(ref float __result)
        {
            float mult = 1;
            foreach (var player in Game1.getAllFarmers())
            {
                if (player.professions.Contains(Mod.ProfessionProfit))
                {
                    mult += 0.05f;
                }
            }
            __result *= mult;
        }
    }
}

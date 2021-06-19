using System.Diagnostics.CodeAnalysis;
using Harmony;
using Spacechase.Shared.Harmony;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Farmer"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class FarmerPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Farmer>(nameof(Farmer.doneEating)),
                postfix: this.GetHarmonyMethod(nameof(After_DoneEating))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after <see cref="Farmer.doneEating"/>.</summary>
        public static void After_DoneEating(Farmer __instance)
        {
            if (__instance.itemToEat == null)
                return;
            SpaceEvents.InvokeOnItemEaten(__instance);
        }
    }
}

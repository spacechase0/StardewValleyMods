using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace MoreBuildings.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Shed"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ShedPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Shed>(nameof(Shed.updateLayout)),
                prefix: this.GetHarmonyMethod(nameof(Before_UpdateLayout))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Shed.updateLayout"/>.</summary>
        private static bool Before_UpdateLayout(Shed __instance)
        {
            // Why does this happen? Who knows.
            // Not sure why I need this in the first place, but it prevents an error from showing up during save loading
            // If the error shows up, update transition functions won't apply (and probably other things)
            if (__instance.map == null)
                return false;

            return true;
        }
    }
}

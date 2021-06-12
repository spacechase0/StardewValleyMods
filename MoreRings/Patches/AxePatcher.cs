using System.Diagnostics.CodeAnalysis;
using Harmony;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace MoreRings.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Axe"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class AxePatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Axe>(nameof(Axe.DoFunction)),
                prefix: this.GetHarmonyMethod(nameof(Before_DoFunction), priority: Priority.First)
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Axe.DoFunction"/>.</summary>
        private static void Before_DoFunction(Hoe __instance, GameLocation location, ref int x, ref int y, int power, Farmer who)
        {
            if (Mod.Instance.HasRingEquipped(Mod.Instance.RingMageHand) > 0)
            {
                x = (int)who.lastClick.X;
                y = (int)who.lastClick.Y;
            }
        }
    }
}

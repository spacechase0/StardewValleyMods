using System.Diagnostics.CodeAnalysis;
using Harmony;
using Spacechase.Shared.Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace MoreRings.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Hoe"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class HoePatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Hoe>(nameof(Hoe.DoFunction)),
                prefix: this.GetHarmonyMethod(nameof(Before_DoFunction), priority: Priority.First)
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Hoe.DoFunction"/>.</summary>
        private static void Before_DoFunction(Hoe __instance, GameLocation location, ref int x, ref int y, int power, Farmer who)
        {
            if (Mod.instance.hasRingEquipped(Mod.instance.Ring_MageHand) > 0)
            {
                x = (int)who.lastClick.X;
                y = (int)who.lastClick.Y;
            }
        }
    }
}

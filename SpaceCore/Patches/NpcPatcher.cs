using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="NPC"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class NpcPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<NPC>(nameof(NPC.tryToReceiveActiveObject)),
                prefix: this.GetHarmonyMethod(nameof(Before_TryToReceiveActiveObject))
            );

            harmony.Patch(
                original: this.RequireMethod<NPC>(nameof(NPC.receiveGift)),
                postfix: this.GetHarmonyMethod(nameof(After_ReceiveGift))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="NPC.tryToReceiveActiveObject"/>.</summary>
        private static bool Before_TryToReceiveActiveObject(NPC __instance, Farmer who, bool probe)
        {
            return !SpaceEvents.InvokeBeforeReceiveObject(__instance, who.ActiveObject, who, probe);
        }

        /// <summary>The method to call after <see cref="NPC.receiveGift"/>.</summary>
        private static void After_ReceiveGift(NPC __instance, SObject o, Farmer giver, bool updateGiftLimitInfo = true, float friendshipChangeMultiplier = 1f, bool showResponse = true)
        {
            SpaceEvents.InvokeAfterGiftGiven(__instance, o, giver);
        }
    }
}

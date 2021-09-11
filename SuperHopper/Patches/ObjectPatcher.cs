using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace SuperHopper.Patches
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
                original: this.RequireMethod<SObject>(nameof(SObject.minutesElapsed)),
                prefix: this.GetHarmonyMethod(nameof(Before_MinutesElapsed))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="SObject.minutesElapsed"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_MinutesElapsed(SObject __instance, int minutes, GameLocation environment)
        {
            if (__instance is Chest { SpecialChestType: Chest.SpecialChestTypes.AutoLoader } chest && chest.heldObject.Value != null && Utility.IsNormalObjectAtParentSheetIndex(chest.heldObject.Value, SObject.iridiumBar))
            {
                environment.objects.TryGetValue(chest.TileLocation - new Vector2(0, 1), out SObject aboveObj);
                if (aboveObj is Chest aboveChest && chest.items.Count < chest.GetActualCapacity() && aboveChest.items.Count > 0)
                {
                    chest.items.Add(aboveChest.items[0]);
                    aboveChest.items.RemoveAt(0);
                }
                // Not doing for now because I'd need to handle every machine's special rules, like changing ParentSheetIndex
                /*
                else if ( aboveObj != null && aboveObj?.GetType() == typeof( SObject ) && aboveObj.bigCraftable.Value && aboveObj.MinutesUntilReady == 0 && chest.items.Count < chest.GetActualCapacity() )
                {
                    chest.addItem( aboveObj.heldObject.Value );
                    aboveObj.heldObject.Value = null;
                }
                */

                environment.objects.TryGetValue(chest.TileLocation + new Vector2(0, 1), out SObject belowObj);
                if (belowObj is Chest belowChest && chest.items.Count > 0 && belowChest.items.Count < belowChest.GetActualCapacity())
                {
                    belowChest.items.Add(chest.items[0]);
                    chest.items.RemoveAt(0);
                }
                return false;
            }

            return true;
        }
    }
}

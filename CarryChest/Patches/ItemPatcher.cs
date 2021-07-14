using System.Diagnostics.CodeAnalysis;
using Harmony;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace CarryChest.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Item"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ItemPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Item>(nameof(Item.canStackWith)),
                prefix: this.GetHarmonyMethod(nameof(Before_CanStackWith))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Item.canStackWith"/>.</summary>
        public static bool Before_CanStackWith(Item __instance, ISalable other, ref bool __result)
        {
            // We're checking the `.ParentSheetIndex` instead of `is Chest` because when you break a chest 
            // and pick it up it isn't a chest instance, it's just an object with the chest index.
            if (__instance.ParentSheetIndex == 130 && other is SObject { ParentSheetIndex: 130 })
            {
                if (__instance is Chest left && left.items.Count != 0 || other is Chest right && right.items.Count != 0)
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }
}

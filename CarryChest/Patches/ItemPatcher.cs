using System.Diagnostics.CodeAnalysis;
using Harmony;
using Spacechase.Shared.Harmony;
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
            if (__instance.ParentSheetIndex == 130 && (other is SObject obj && obj.ParentSheetIndex == 130))
            {
                Chest c1 = __instance as Chest;
                Chest c2 = other as Chest;
                if (c1 != null && c1.items.Count != 0 || c2 != null && c2.items.Count != 0)
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }
}

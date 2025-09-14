using System.Diagnostics.CodeAnalysis;
using CarryChest.Framework;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

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
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Item>(nameof(Item.canStackWith)),
                postfix: this.GetHarmonyMethod(nameof(After_CanStackWith))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after <see cref="Item.canStackWith"/>.</summary>
        private static void After_CanStackWith(Item __instance, ISalable other, ref bool __result)
        {
            // prevent stacking chests that contain items
            if (__result)
                __result = !ItemPatcher.ShouldPreventStacking(__instance) && !ItemPatcher.ShouldPreventStacking(other);
        }

        /// <summary>Get whether to prevent stacking the given item.</summary>
        /// <param name="item">The item to check.</param>
        private static bool ShouldPreventStacking(ISalable item)
        {
            return
                ChestHelper.IsSupported(item)
                && item is Chest chest
                && chest.items.Count != 0;
        }
    }
}

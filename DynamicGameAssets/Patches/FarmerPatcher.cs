using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using DynamicGameAssets.Framework;
using DynamicGameAssets.Game;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Farmer"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class FarmerPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Farmer>(nameof(Farmer.getItemCountInList)),
                prefix: this.GetHarmonyMethod(nameof(Before_GetItemCountInList))
            );
            harmony.Patch(
                original: this.RequireMethod<Farmer>(nameof(Farmer.removeItemsFromInventory)),
                prefix: this.GetHarmonyMethod(nameof(Before_RemoveItemsFromInventory))
            );
            harmony.Patch(
                original: this.RequireMethod<Farmer>(nameof(Farmer.eatObject)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_EatObject))
            );
            harmony.Patch(
                original: this.RequireMethod<Farmer>(nameof(Farmer.doneEating)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_DoneEating))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Farmer.getItemCountInList"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_GetItemCountInList(Farmer __instance, IList<Item> list, int item_index,
            int min_price, ref int __result)
        {
            if (Mod.itemLookup.ContainsKey(item_index))
            {
                __result = 0;
                for (int i = 0; i < list.Count; ++i)
                {
                    var item = list[i];
                    if (item is CustomObject obj && obj.FullId.GetDeterministicHashCode() == item_index)
                        __result += obj.Stack;
                }

                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="Farmer.removeItemsFromInventory"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_RemoveItemsFromInventory(Farmer __instance, int index, int stack, ref bool __result)
        {
            if (Mod.itemLookup.ContainsKey(index))
            {
                if (__instance.hasItemInInventory(index, stack))
                {
                    for (int i = 0; i < __instance.items.Count; ++i)
                    {
                        var item = __instance.items[i];
                        if (item is not CustomObject obj || obj.FullId.GetDeterministicHashCode() != index)
                            continue;

                        if (item.Stack > stack)
                        {
                            item.Stack -= stack;
                            break;
                        }
                        else
                        {
                            stack -= item.Stack;
                            __instance.items[i] = null;

                            if (stack == 0)
                                break;
                        }
                    }

                    __result = true;
                }
                else
                {
                    __result = false;
                }

                return false;
            }

            return true;
        }

        /// <summary>The method which transpiles <see cref="Farmer.eatObject"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_EatObject(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            return PatchCommon.RedirectForFakeObjectInformationTranspiler2(gen, original, instructions);
        }

        /// <summary>The method which transpiles <see cref="Farmer.doneEating"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_DoneEating(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            return PatchCommon.RedirectForFakeObjectInformationTranspiler2(gen, original, instructions);
        }
    }
}

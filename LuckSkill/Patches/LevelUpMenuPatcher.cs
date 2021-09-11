using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace LuckSkill.Patches
{
    /// <summary>Applies Harmony patches to <see cref="LevelUpMenu"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class LevelUpMenuPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireConstructor<LevelUpMenu>(typeof(int), typeof(int)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_Constructor))
            );

            harmony.Patch(
                original: this.RequireMethod<LevelUpMenu>("getProfessionName"),
                postfix: this.GetHarmonyMethod(nameof(After_GetProfessionName))
            );

            harmony.Patch(
                original: this.RequireMethod<LevelUpMenu>(nameof(LevelUpMenu.AddMissedProfessionChoices)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_AddMissedProfessionChoices_And_AddMissedLevelRecipes))
            );

            harmony.Patch(
                original: this.RequireMethod<LevelUpMenu>(nameof(LevelUpMenu.AddMissedLevelRecipes)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_AddMissedProfessionChoices_And_AddMissedLevelRecipes))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles the <see cref="LevelUpMenu"/> constructor.</summary>
        private static IEnumerable<CodeInstruction> Transpile_Constructor(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            bool foundCurrentSkill = false;
            int skip = 0;
            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (skip > 0)
                {
                    skip--;
                    insn.opcode = OpCodes.Nop;
                    newInsns.Add(insn);
                    continue;
                }

                if (insn.opcode == OpCodes.Ldfld && ((FieldInfo)insn.operand).Name.Contains("currentSkill"))
                {
                    foundCurrentSkill = true;
                }
                else if (foundCurrentSkill)
                {
                    foundCurrentSkill = false;
                    if (insn.opcode == OpCodes.Ldc_I4_5)
                    {
                        skip = 1;
                        newInsns[newInsns.Count - 2].opcode = OpCodes.Nop;
                        newInsns[newInsns.Count - 1].opcode = OpCodes.Nop;
                        insn.opcode = OpCodes.Nop;
                        continue;
                    }
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }

        /// <summary>The method to call after <see cref="LevelUpMenu.getProfessionName"/>.</summary>
        private static void After_GetProfessionName(int whichProfession, ref string __result)
        {
            __result = whichProfession switch
            {
                Mod.FortunateProfessionId => "LUCK_A",
                Mod.PopularHelperProfessionId => "LUCK_B",
                Mod.LuckyProfessionId => "LUCK_A1",
                Mod.UnUnluckyProfessionId => "LUCK_A2",
                Mod.ShootingStarProfessionId => "LUCK_B1",
                Mod.SpiritChildProfessionId => "LUCK_B2",
                _ => __result
            };
        }

        /// <summary>The method which transpiles <see cref="LevelUpMenu.AddMissedProfessionChoices"/> and <see cref="LevelUpMenu.AddMissedLevelRecipes"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_AddMissedProfessionChoices_And_AddMissedLevelRecipes(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                newInsns.Add(insn);
                if (insn.opcode == OpCodes.Call && ((MethodInfo)insn.operand).Name.Contains("InitializeArray"))
                {
                    newInsns.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<LevelUpMenuPatcher>(nameof(GetFixedArray))));
                }
            }

            return newInsns;
        }

        private static int[] GetFixedArray(int[] oldArray)
        {
            return oldArray
                .Concat(new[] { Farmer.luckSkill })
                .ToArray();
        }
    }
}

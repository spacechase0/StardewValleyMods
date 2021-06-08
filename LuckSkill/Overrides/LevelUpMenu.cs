using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Spacechase.Shared.Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace LuckSkill.Overrides
{
    /// <summary>Applies Harmony patches to <see cref="LevelUpMenu"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class LevelUpMenuPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
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
            switch (whichProfession)
            {
                case Mod.PROFESSION_DAILY_LUCK:
                    __result = "LUCK_A";
                    break;
                case Mod.PROFESSION_MORE_QUESTS:
                    __result = "LUCK_B";
                    break;
                case Mod.PROFESSION_CHANCE_MAX_LUCK:
                    __result = "LUCK_A1";
                    break;
                case Mod.PROFESSION_NO_BAD_LUCK:
                    __result = "LUCK_A2";
                    break;
                case Mod.PROFESSION_NIGHTLY_EVENTS:
                    __result = "LUCK_B1";
                    break;
                case Mod.PROFESSION_JUNIMO_HELP:
                    __result = "LUCK_B2";
                    break;
            }
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
                    newInsns.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<LevelUpMenuPatcher>(nameof(LevelUpMenuPatcher.GetFixedArray))));
                }
            }

            return newInsns;
        }

        private static int[] GetFixedArray(int[] oldArray)
        {
            var list = new List<int>(oldArray);
            list.Add(Farmer.luckSkill);
            return list.ToArray();
        }
    }
}

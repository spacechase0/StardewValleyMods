using Harmony;
using SpaceShared;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace LuckSkill.Overrides
{
    public static class LevelUpMenuLuckProfessionConstructorFix
    {
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            bool foundCurrentSkill = false;
            int skip = 0;
            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if ( skip > 0 )
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
                else if ( foundCurrentSkill )
                {
                    foundCurrentSkill = false;
                    if ( insn.opcode == OpCodes.Ldc_I4_5 )
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
    }

    public static class LevelUpMenuProfessionNameHook
    {
        public static void Postfix(int whichProfession, ref string __result)
        {
            switch ( whichProfession )
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
    }

    // For AddMissedProfessionChoices and AddMissedLevelRecipes
    public static class LevelUpMenuMissedStuffPatch
    {
        public static int[] GetFixedArray(int[] oldArray)
        {
            var list = new List<int>(oldArray);
            list.Add(Farmer.luckSkill);
            return list.ToArray();
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                newInsns.Add(insn);
                if (insn.opcode == OpCodes.Call && ((MethodInfo)insn.operand).Name.Contains("InitializeArray"))
                {
                    newInsns.Add(new CodeInstruction(OpCodes.Call, typeof(LevelUpMenuMissedStuffPatch).GetMethod(nameof(GetFixedArray))));
                }
            }

            return newInsns;
        }
    }
}

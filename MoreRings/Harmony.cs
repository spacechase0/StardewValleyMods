using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using SpaceShared;
using StardewValley;
using StardewValley.Tools;

namespace MoreRings
{

    public static class AxeRemoteUseHook
    {
        public static void Prefix(Hoe __instance, GameLocation location, ref int x, ref int y, int power, Farmer who)
        {
            if (Mod.instance.hasRingEquipped(Mod.instance.Ring_MageHand) > 0)
            {
                x = (int)who.lastClick.X;
                y = (int)who.lastClick.Y;
            }
        }
    }
    public static class Game1ToolRangeHook
    {
        public static int toolRangeHook()
        {
            var tool = Game1.player.CurrentTool;
            if (tool == null)
                return 1;
            else if (tool is Hoe || tool is Pickaxe || tool is WateringCan || tool is Axe)
            {
                if (Mod.instance.hasRingEquipped(Mod.instance.Ring_MageHand) > 0)
                    return 100;
                else
                    return 1;
            }
            else
                return 1;
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            int utilWithinRadiusCount = 0;

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Call && insn.operand is MethodInfo meth)
                {
                    if (meth.Name == "withinRadiusOfPlayer")
                    {
                        if (utilWithinRadiusCount++ == 1)
                        {
                            Log.trace("Found second Utility.withinRadiusOfPlayer call, replacing i-2 with our hook function");
                            newInsns[newInsns.Count - 2] = new CodeInstruction(OpCodes.Call, typeof(Game1ToolRangeHook).GetMethod("toolRangeHook"));
                        }
                    }
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }
    }

    public static class HoeRemoteUseHook
    {
        public static void Prefix(Hoe __instance, GameLocation location, ref int x, ref int y, int power, Farmer who)
        {
            if (Mod.instance.hasRingEquipped(Mod.instance.Ring_MageHand) > 0)
            {
                x = (int)who.lastClick.X;
                y = (int)who.lastClick.Y;
            }
        }
    }
    public static class PickaxeRemoteUseHook
    {
        public static void Prefix(Hoe __instance, GameLocation location, ref int x, ref int y, int power, Farmer who)
        {
            if (Mod.instance.hasRingEquipped(Mod.instance.Ring_MageHand) > 0)
            {
                x = (int)who.lastClick.X;
                y = (int)who.lastClick.Y;
            }
        }
    }

    public static class WateringCanRemoteUseHook
    {
        public static void Prefix(Hoe __instance, GameLocation location, ref int x, ref int y, int power, Farmer who)
        {
            if (Mod.instance.hasRingEquipped(Mod.instance.Ring_MageHand) > 0)
            {
                x = (int)who.lastClick.X;
                y = (int)who.lastClick.Y;
            }
        }
    }
}

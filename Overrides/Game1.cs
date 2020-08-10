using Harmony;
using SpaceCore.Events;
using StardewValley;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SpaceCore.Overrides
{
    public class BlankSaveHook
    {
        public static void Postfix(bool loadedGame)
        {
            SpaceEvents.InvokeOnBlankSave();
        }
    }

    public class ShowEndOfNightStuffHook
    {
        public static void showEndOfNightStuff_mid()
        {
            var ev = new EventArgsShowNightEndMenus();
            SpaceEvents.InvokeShowNightEndMenus(ev);
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Ldstr && (string)insn.operand == "newRecord")
                {
                    newInsns.Insert(newInsns.Count - 2, new CodeInstruction(OpCodes.Call, typeof(ShowEndOfNightStuffHook).GetMethod("showEndOfNightStuff_mid")));
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }
    }

    public static class DoneEatingHook
    {
        public static void Postfix(Farmer __instance)
        {
            if ( __instance.itemToEat == null )
                return;
            SpaceEvents.InvokeOnItemEaten( __instance);
        }
    }

    public class WarpFarmerHook
    {
        public static bool Prefix(ref LocationRequest locationRequest, int tileX, int tileY, int facingDirectionAfterWarp)
        {
            return !SpaceEvents.InvokeBeforeWarp(ref locationRequest, tileX, tileY, facingDirectionAfterWarp);
        }
}
}

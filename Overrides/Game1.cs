using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpaceCore.Events;
using SpaceCore.Locations;
using SpaceCore.Utilities;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

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
            SpaceEvents.InvokeOnItemEaten( __instance);
        }
    }
}

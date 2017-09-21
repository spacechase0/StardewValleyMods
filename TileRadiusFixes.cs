using Harmony;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedReach
{
    internal class TileRadiusFix
    {
        internal static IEnumerable<CodeInstruction> IncreaseRadiusChecks(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Call && insn.operand is MethodInfo meth)
                {
                    if (meth.Name == "withinRadiusOfPlayer" || meth.Name == "tileWithinRadiusOfPlayer")
                    {
                        var newInsn = new CodeInstruction(OpCodes.Ldc_I4, 100);
                        Log.trace($"Found {meth.Name}, replacing {newInsns[newInsns.Count - 2]} with {newInsn}");
                        newInsns[newInsns.Count - 2] = newInsn;
                    }
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }
    }

    [HarmonyPatch(typeof(Utility), "canGrabSomethingFromHere")]
    public class Utility_canGrabSomethingFromHere
    {
        internal static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            Log.trace("Patching Utility.canGrabSomethingFromHere");
            return TileRadiusFix.IncreaseRadiusChecks(gen, original, insns);
        }
    }
    [HarmonyPatch(typeof(Utility), "checkForCharacterInteractionAtTile")]
    public class Utility_checkForCharacterInteractionAtTile
    {
        internal static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            Log.trace("Patching Utility.checkForCharacterInteractionAtTile");
            return TileRadiusFix.IncreaseRadiusChecks(gen, original, insns);
        }
    }
    [HarmonyPatch(typeof(Game1), "pressActionButton")]
    public class Game1_pressActionButton
    {
        internal static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            Log.trace("Patching Game1.pressActionButton");
            return TileRadiusFix.IncreaseRadiusChecks(gen, original, insns);
        }
    }
    [HarmonyPatch(typeof(Game1), "pressUseToolButton")]
    public class Game1_pressUseToolButton
    {
        internal static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            Log.trace("Patching Game1.pressUseToolButton");
            return TileRadiusFix.IncreaseRadiusChecks(gen, original, insns);
        }
    }
    [HarmonyPatch(typeof(Game1), "tryToCheckAt")]
    public class Game1_tryToCheckAt
    {
        internal static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            Log.trace("Patching Game1.tryToCheckAt");
            return TileRadiusFix.IncreaseRadiusChecks(gen, original, insns);
        }
    }
    [HarmonyPatch(typeof(GameLocation), "isActionableTile")]
    public class GameLocation_isActionableTile
    {
        internal static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            Log.trace("Patching GameLocation.isActionableTile");
            return TileRadiusFix.IncreaseRadiusChecks(gen, original, insns);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Netcode;
using SpaceShared;
using StardewValley;

namespace MoreGiantCrops
{
    // Copied/modified from JA
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is set by Harmony.")]
    public static class CropPatches
    {
        public static IEnumerable<CodeInstruction> NewDay_Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            instructions = instructions.ToArray();

            // TODO: Learn how to use ILGenerator
            try
            {
                var newInstructions = new List<CodeInstruction>();
                int hookCountdown = 0;
                bool justHooked = false;
                object label = null;
                foreach ( var instr in instructions )
                {
                    // If this is the spot for our hook, inject it
                    if (hookCountdown > 0 && --hookCountdown == 0)
                    {
                        newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                        newInstructions.Add(new CodeInstruction(OpCodes.Ldfld, typeof(Crop).GetField(nameof(Crop.indexOfHarvest))));
                        newInstructions.Add(new CodeInstruction(OpCodes.Call, typeof(CropPatches).GetMethod(nameof(CheckCanBeGiant))));
                        newInstructions.Add(new CodeInstruction(OpCodes.Brtrue_S, label));
                    }

                    // The only reference to 276 is the index of the pumpkin for checking for giant crops growing
                    if (instr.opcode == OpCodes.Ldc_I4 && (int)instr.operand == 276)
                    {
                        // In two instructions (after this and the next), we want our check
                        hookCountdown = 2;
                        justHooked = true;
                    }
                    // If this is the instruction after the previous check, we want to borrow the label for our own use
                    else if ( justHooked )
                    {
                        label = instr.operand;
                        justHooked = false;
                    }
                    newInstructions.Add(instr);
                }

                return newInstructions;
            }
            catch (Exception ex)
            {
                Log.error($"Failed in {nameof(NewDay_Transpiler)}:\n{ex}");
                return instructions;
            }
        }

        public static bool CheckCanBeGiant(NetInt indexOfHarvest_)
        {
            return Mod.sprites.ContainsKey(indexOfHarvest_.Value);
        }
    }
}

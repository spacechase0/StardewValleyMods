using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Netcode;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace MoreGiantCrops.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Crop"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class CropPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Crop>(nameof(Crop.newDay)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_NewDay))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles <see cref="Crop.newDay"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_NewDay(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            instructions = instructions.ToArray();

            // Copied/modified from Json Assets
            // TODO: Learn how to use ILGenerator
            try
            {
                var newInstructions = new List<CodeInstruction>();
                int hookCountdown = 0;
                bool justHooked = false;
                object label = null;
                foreach (var instr in instructions)
                {
                    // To allow on Ginger island and elsewhere where crops can grow, we need to take out this check
                    if ( instr.opcode == OpCodes.Isinst && ( Type ) instr.operand == typeof( Farm ) )
                    {
                        // First check is in an if, taking out the cast makes it always true (it's load var -> isinst -> brfalse)
                        // Second check can be removed also - it just casts and then loads a field on the uncasted one
                        // (This is because in older versions of the game, it was on Farm, but now is on GameLocation)
                        continue;
                    }

                    // If this is the spot for our hook, inject it
                    if (hookCountdown > 0 && --hookCountdown == 0)
                    {
                        newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                        newInstructions.Add(new CodeInstruction(OpCodes.Ldfld, typeof(Crop).GetField(nameof(Crop.indexOfHarvest))));
                        newInstructions.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<CropPatcher>(nameof(CheckCanBeGiant))));
                        newInstructions.Add(new CodeInstruction(OpCodes.Brtrue_S, label));
                    }

                    // The only reference to 276 is the index of the pumpkin for checking for giant crops growing
                    if (instr.opcode == OpCodes.Ldstr && (string)instr.operand == "276")
                    {
                        // In two instructions (after this and the next), we want our check
                        hookCountdown = 3;
                        justHooked = true;
                    }
                    // If this is the instruction after the previous check, we want to borrow the label for our own use
                    else if (justHooked)
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
                Log.Error($"Failed in {nameof(Transpile_NewDay)}:\n{ex}");
                return instructions;
            }
        }

        public static bool CheckCanBeGiant(NetInt indexOfHarvest)
        {
            return Mod.Sprites.ContainsKey(indexOfHarvest.Value);
        }
    }
}

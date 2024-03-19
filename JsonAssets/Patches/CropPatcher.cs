using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JsonAssets.Data;
using Netcode;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace JsonAssets.Patches
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

        /// <summary>The method which transpiles <see cref="Crop.newDay"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_NewDay(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            instructions = instructions.ToArray();

            // TODO: Learn how to use ILGenerator
            try
            {
                // Pass 1 - indoor crop check
                var newInstructions = new List<CodeInstruction>();
                bool justHooked = false;
                foreach (var instr in instructions)
                {
                    // The only reference to 90 is the index of the cactus fruit for checking if it is an indoor only crop
                    if (instr.opcode == OpCodes.Ldstr && (string)instr.operand == "90")
                    {
                        // By default the check is for the crop's product index.
                        // We want the crop itself instead since theoretically two crops could have the same product, but be different types.
                        newInstructions[newInstructions.Count - 3].labels.AddRange(newInstructions[newInstructions.Count - 2].labels);
                        newInstructions.RemoveAt(newInstructions.Count - 2);
                        newInstructions.RemoveAt(newInstructions.Count - 1);

                        // Call our method
                        newInstructions.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<CropPatcher>(nameof(IsIndoorOnlyCrop))));
                        justHooked = true; // We need to change the next insn, whihc is a bna.un.s, to a different type of branch.
                    }
                    else if (justHooked)
                    {
                        instr.opcode = OpCodes.Brfalse_S;
                        //newInstructions.Add(instr);
                        justHooked = false;
                    }
                    else
                    {
                        newInstructions.Add(instr);
                    }
                }

                // Pass 2 - giant crop check
                instructions = newInstructions;
                newInstructions = new List<CodeInstruction>();
                int hookCountdown = 0;
                justHooked = false;
                object label = null;
                foreach (var instr in instructions)
                {
                    // If this is the spot for our hook, inject it
                    if (hookCountdown > 0 && --hookCountdown == 0)
                    {
                        newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                        newInstructions.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<CropPatcher>(nameof(CheckCanBeGiant))));
                        newInstructions.Add(new CodeInstruction(OpCodes.Brtrue_S, label));
                    }
                    else if ( hookCountdown == 1 ) // We want a copy of the label here
                    {
                        label = instr.operand;
                    }

                    // The only reference to 276 is the index of the pumpkin for checking for giant crops growing
                    if (instr.opcode == OpCodes.Ldstr && (string)instr.operand == "276")
                    {
                        // In three instructions (after this, the next, and the next), we want our check
                        hookCountdown = 3;
                        justHooked = true;
                    }
                    else if (justHooked)
                    {
                        justHooked = false;
                    }
                    else
                    {
                        newInstructions.Add(instr);
                    }
                }

                return newInstructions;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(Transpile_NewDay)}:\n{ex}");
                return instructions;
            }
        }

        private static bool IsIndoorOnlyCrop(Crop crop)
        {
            if ( crop.indexOfHarvest == "90" ) // Vanilla cactus fruit
                return true;

            var cropData = Mod.instance.Crops.FirstOrDefault(c => crop.overrideTexturePath.Value == "JA\\Crop\\" + c.Name);
            if (cropData == null)
                return false;
            return cropData.CropType == CropType.IndoorsOnly;
        }

        private static bool CheckCanBeGiant(Crop crop)
        {
            var cropData = Mod.instance.Crops.FirstOrDefault(c => crop.overrideTexturePath.Value == "JA\\Crop\\" + c.Name);
            return cropData?.GiantTexture != null;
        }
    }
}

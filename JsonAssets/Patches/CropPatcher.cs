using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using JsonAssets.Data;
using Netcode;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace JsonAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Crop"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class CropPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Crop>(nameof(Crop.isPaddyCrop)),
                prefix: this.GetHarmonyMethod(nameof(Before_IsPaddyCrop))
            );

            harmony.Patch(
                original: this.RequireMethod<Crop>(nameof(Crop.newDay)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_NewDay))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Crop.isPaddyCrop"/>.</summary>
        private static bool Before_IsPaddyCrop(Crop __instance, ref bool __result)
        {
            try
            {
                var cropData = Mod.instance.crops.FirstOrDefault(c => c.GetCropSpriteIndex() == __instance.rowInSpriteSheet.Value);
                if (cropData == null)
                    return true;

                if (cropData.CropType == CropData.CropType_.Paddy)
                {
                    __result = true;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(Before_IsPaddyCrop)}:\n{ex}");
                return true;
            }
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
                    if (instr.opcode == OpCodes.Ldc_I4_S && (sbyte)instr.operand == 90)
                    {
                        // By default the check is for the crop's product index.
                        // We want the crop index itself instead since theoretically two crops could have the same product, but be different types.
                        newInstructions[newInstructions.Count - 2].operand = typeof(Crop).GetField(nameof(Crop.rowInSpriteSheet));

                        // Call our method
                        newInstructions.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<CropPatcher>(nameof(IsIndoorOnlyCrop))));
                        justHooked = true; // We need to change the next insn, whihc is a bna.un.s, to a different type of branch.
                    }
                    else if (justHooked)
                    {
                        instr.opcode = OpCodes.Brfalse_S;
                        newInstructions.Add(instr);
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
                        newInstructions.Add(new CodeInstruction(OpCodes.Ldfld, typeof(Crop).GetField(nameof(Crop.rowInSpriteSheet)))); // We use the rowInSpriteSheet. See comments on previous pass
                        newInstructions.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<CropPatcher>(nameof(CheckCanBeGiant))));
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

        private static bool IsIndoorOnlyCrop(int cropRow)
        {
            if (cropRow == 41) // Vanilla cactus fruit
                return true;

            var cropData = Mod.instance.crops.FirstOrDefault(c => c.GetCropSpriteIndex() == cropRow);
            if (cropData == null)
                return false;
            return cropData.CropType == CropData.CropType_.IndoorsOnly;
        }

        private static bool CheckCanBeGiant(NetInt cropRow_)
        {
            int cropRow = cropRow_.Value;
            var cropData = Mod.instance.crops.FirstOrDefault(c => c.GetCropSpriteIndex() == cropRow);
            if (cropData == null)
                return false;
            return cropData.giantTex != null;
        }
    }
}

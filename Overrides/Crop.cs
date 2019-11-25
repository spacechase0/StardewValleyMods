using Harmony;
using JsonAssets.Data;
using SpaceShared;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Overrides
{
    public static class PaddyCropHook
    {
        public static bool Prefix(Crop __instance, ref bool __result)
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
    }

    public static class IndoorOnlyCropHook
    {
        public static bool IsIndoorOnlyCrop(int cropRow)
        {
            if (cropRow == 41) // Vanilla cactus fruit
                return true;

            var cropData = Mod.instance.crops.FirstOrDefault(c => c.GetCropSpriteIndex() == cropRow);
            if (cropData == null)
                return false;
            return cropData.CropType == CropData.CropType_.IndoorsOnly;
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            var newInsns = new List<CodeInstruction>();
            bool justHooked = false;
            foreach (var insn in insns)
            {
                // The only reference to 90 is the index of the cactus fruit for checking if it is an indoor only crop
                if (insn.opcode == OpCodes.Ldc_I4_S && (sbyte) insn.operand == 90)
                {
                    // By default the check is for the crop's product index.
                    // We want the crop index itself instead since theoretically two crops could have the same product, but be different types.
                    newInsns[newInsns.Count - 2].operand = typeof(Crop).GetField(nameof(Crop.rowInSpriteSheet)); 

                    // Call our method
                    newInsns.Add(new CodeInstruction(OpCodes.Call, typeof(IndoorOnlyCropHook).GetMethod(nameof(IsIndoorOnlyCrop))));
                    justHooked = true; // We need to change the next insn, whihc is a bna.un.s, to a different type of branch.
                }
                else if (justHooked)
                {
                    insn.opcode = OpCodes.Brfalse_S;
                    newInsns.Add(insn);
                    justHooked = false;
                }
                else
                {
                    newInsns.Add(insn);
                }
            }

            return newInsns;
        }
    }
}

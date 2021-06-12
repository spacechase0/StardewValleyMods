using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace MoreRings.Patches
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
                original: this.RequireMethod<Crop>(nameof(Crop.harvest)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_Harvest))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles <see cref="Crop.harvest"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_Harvest(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            var newInsns = new List<CodeInstruction>();
            LocalBuilder randVar = null;
            Label pendingLabel = default(Label);
            foreach (var insn in insns)
            {
                if (insn.operand is LocalBuilder lb && lb.LocalIndex == 9)
                {
                    randVar = lb;
                }
                if (insn.opcode == OpCodes.Stloc_S && ((LocalBuilder)insn.operand).LocalIndex == 7 /* cropQuality, TODO: Check somehow */ )
                {
                    var prevInsn = newInsns[newInsns.Count - 1];
                    var prev2Insn = newInsns[newInsns.Count - 2];
                    if (prevInsn.opcode == OpCodes.Ldc_I4_1 && prev2Insn.opcode == OpCodes.Bge_Un)
                    {
                        pendingLabel = (Label)prev2Insn.operand;
                        newInsns.Add(insn);

                        newInsns.Add(new CodeInstruction(OpCodes.Ldloc_S, randVar)
                        { labels = new List<Label>(new[] { pendingLabel }) });
                        newInsns.Add(new CodeInstruction(OpCodes.Ldloca_S, insn.operand));
                        newInsns.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<CropPatcher>(nameof(ModifyCropQuality))));
                        continue;
                    }
                }
                if (insn.labels.Contains(pendingLabel))
                {
                    Log.Trace("taking label");
                    insn.labels.Remove(pendingLabel);
                    pendingLabel = default(Label);
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }

        private static void ModifyCropQuality(Random rand, ref int quality)
        {
            if (rand.NextDouble() < Mod.instance.hasRingEquipped(Mod.instance.Ring_Quality) * 0.125)
            {
                if (++quality == 3)
                    ++quality;
            }
            if (quality > 4)
                quality = 4;
        }
    }
}

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley.Objects;

namespace BiggerJunimoChest.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Chest"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ChestPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Chest>(nameof(Chest.GetActualCapacity)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_GetActualCapacity))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles <see cref="Chest.GetActualCapacity"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_GetActualCapacity(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            int counter = 0;

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Ldc_I4_S && (sbyte)insn.operand == 9)
                {
                    if (++counter == 2)
                        insn.operand = (sbyte)36;
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }
    }
}

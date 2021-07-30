using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace CapstoneProfessions.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Game1"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class Game1Patcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Game1>(nameof(Game1.UpdateGameClock)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_UpdateGameClock))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles <see cref="Game1.UpdateGameClock"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_UpdateGameClock(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            List<CodeInstruction> ret = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Ldc_I4 && (int)insn.operand == 7000)
                {
                    ret.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<Game1Patcher>(nameof(GetTimeInterval))));
                    continue;
                }
                ret.Add(insn);
            }
            return ret;
        }

        private static int GetTimeInterval()
        {
            float mult = 1;
            foreach (var player in Game1.getAllFarmers())
            {
                if (player.professions.Contains(Mod.ProfessionTime))
                {
                    mult += 0.2f;
                }
            }

            return (int)(7000 * mult);
        }
    }
}

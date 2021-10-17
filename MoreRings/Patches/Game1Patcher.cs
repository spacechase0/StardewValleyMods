using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace MoreRings.Patches
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
                original: this.RequireMethod<Game1>(nameof(Game1.pressUseToolButton)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_PressUseToolButton))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles <see cref="Game1.pressUseToolButton"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_PressUseToolButton(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            int utilWithinRadiusCount = 0;

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Call && (insn.operand as MethodInfo)?.Name == "withinRadiusOfPlayer")
                {
                    if (utilWithinRadiusCount++ == 1)
                    {
                        Log.Trace("Found second Utility.withinRadiusOfPlayer call, replacing i-2 with our hook function");
                        newInsns[newInsns.Count - 2] = new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<Game1Patcher>(nameof(toolRangeHook)));
                    }
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }

        private static int toolRangeHook()
        {
            var tool = Game1.player.CurrentTool;

            if (tool is Hoe or Pickaxe or WateringCan or Axe)
            {
                return Mod.Instance.HasRingEquipped(Mod.Instance.RingMageHand)
                    ? Mod.Instance.Config.RingOfFarReaching_TileDistance
                    : 1;
            }
            else
                return 1;
        }
    }
}

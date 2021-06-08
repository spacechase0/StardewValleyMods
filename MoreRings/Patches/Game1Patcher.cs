using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace MoreRings.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Game1"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class Game1Patcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
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
                if (insn.opcode == OpCodes.Call && insn.operand is MethodInfo meth)
                {
                    if (meth.Name == "withinRadiusOfPlayer")
                    {
                        if (utilWithinRadiusCount++ == 1)
                        {
                            Log.trace("Found second Utility.withinRadiusOfPlayer call, replacing i-2 with our hook function");
                            newInsns[newInsns.Count - 2] = new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<Game1Patcher>(nameof(toolRangeHook)));
                        }
                    }
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }

        private static int toolRangeHook()
        {
            var tool = Game1.player.CurrentTool;
            if (tool == null)
                return 1;
            else if (tool is Hoe || tool is Pickaxe || tool is WateringCan || tool is Axe)
            {
                if (Mod.instance.hasRingEquipped(Mod.instance.Ring_MageHand) > 0)
                    return 100;
                else
                    return 1;
            }
            else
                return 1;
        }
    }
}

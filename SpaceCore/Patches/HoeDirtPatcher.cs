using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="HoeDirt"/>.</summary>
    internal class HoeDirtPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<HoeDirt>(nameof(HoeDirt.dayUpdate)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_DayUpdate))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles <see cref="HoeDirt.dayUpdate"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_DayUpdate(IEnumerable<CodeInstruction> insns)
        {
            Log.trace("Transpiling for hoe dirt winter stuff");
            bool happened = false;
            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (
                    ( insn.opcode == OpCodes.Call || insn.opcode == OpCodes.Callvirt )
                    && (insn.operand as MethodInfo).Name == "destroyCrop"
                    )
                {
                    Log.trace("Removing destroyCrop call");
                    // There are 4 items loaded to the stack at this point
                    var pop = new CodeInstruction(OpCodes.Pop);
                    newInsns.Add(pop.Clone());
                    newInsns.Add(pop.Clone());
                    newInsns.Add(pop.Clone());
                    newInsns.Add(pop.Clone());
                    happened = true;
                    continue;
                }
                newInsns.Add(insn);
            }

            if (!happened) {
                Log.error($"{nameof(Transpile_DayUpdate)} patching failed!");
                }
            return newInsns;
        }
    }
}

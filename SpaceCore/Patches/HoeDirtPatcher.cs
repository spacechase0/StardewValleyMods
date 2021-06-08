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
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
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
        private static IEnumerable<CodeInstruction> Transpile_DayUpdate(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator
            Log.trace("Transpiling for hoe dirt winter stuff");
            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Call && (insn.operand as MethodInfo).Name == "destroyCrop")
                {
                    Log.trace("Replacing destroyCrop with our call");
                    // Replace with our call. We do this instead of nop to clear the stack entries
                    // Because I'm too lazy to figure out the rest properly.
                    insn.operand = PatchHelper.RequireMethod<HoeDirtPatcher>(nameof(DestroyCropReplacement));
                }

                newInsns.Add(insn);
            }

            return newInsns;
        }

        private static void DestroyCropReplacement(HoeDirt hoeDirt, Vector2 tileLocation, bool showAnimation, GameLocation location)
        {
            // We don't want it to ever do anything.
            // Crops wither out of season anyways.
        }
    }
}

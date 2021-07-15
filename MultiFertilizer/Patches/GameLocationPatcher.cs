using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Microsoft.Xna.Framework;
using MultiFertilizer.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace MultiFertilizer.Patches
{
    /// <summary>Applies Harmony patches to <see cref="GameLocation"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class GameLocationPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.isTileOccupiedForPlacement)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_isTileOccupiedForPlacement))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles <see cref="GameLocation.isTileOccupiedForPlacement"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_isTileOccupiedForPlacement(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            bool stopCaring = false;
            bool foundFertCategory = false;

            // When we find SObject.fertilizerCategory, after the next instruction:
            // Place our patched section function call. If it returns true, return from the function true.

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (stopCaring)
                {
                    newInsns.Add(insn);
                    continue;
                }

                if (insn.opcode == OpCodes.Ldc_I4_S && (sbyte)insn.operand == SObject.fertilizerCategory)
                {
                    newInsns.Add(insn);
                    foundFertCategory = true;
                }
                else if (foundFertCategory)
                {
                    newInsns.Add(insn);

                    var branchPastOld = new CodeInstruction(OpCodes.Br, insn.operand);
                    branchPastOld.labels.Add(gen.DefineLabel());

                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_1));
                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_2));
                    newInsns.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<GameLocationPatcher>(nameof(IsTileOccupiedForPlacementLogic))));

                    newInsns.Add(new CodeInstruction(OpCodes.Brfalse, branchPastOld.labels[0]));

                    newInsns.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
                    newInsns.Add(new CodeInstruction(OpCodes.Ret));

                    newInsns.Add(branchPastOld);

                    foundFertCategory = false;
                    stopCaring = true;
                }
                else
                    newInsns.Add(insn);
            }

            return newInsns;
        }

        private static bool IsTileOccupiedForPlacementLogic(GameLocation __instance, Vector2 tileLocation, SObject toPlace)
        {
            return
                toPlace.Category == SObject.fertilizerCategory
                && __instance.TryGetDirt(tileLocation, out HoeDirt dirt, includePots: false)
                && DirtHelper.TryGetFertilizer(toPlace.ParentSheetIndex, out FertilizerData fertilizer)
                && dirt.HasFertilizer(fertilizer);
        }
    }
}

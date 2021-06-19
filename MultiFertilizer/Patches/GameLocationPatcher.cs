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

            // When we find -19, after the next instruction:
            // Place our patched section function call. If it returns true, return from the function true.

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (stopCaring)
                {
                    newInsns.Add(insn);
                    continue;
                }

                if (insn.opcode == OpCodes.Ldc_I4_S && (sbyte)insn.operand == -19)
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
            if (toPlace.Category == -19 && __instance.terrainFeatures.TryGetValue(tileLocation, out TerrainFeature feature) && feature is HoeDirt hoe_dirt)
            {
                string key = toPlace.ParentSheetIndex switch
                {
                    368 => Mod.KeyFert,
                    369 => Mod.KeyFert,
                    919 => Mod.KeyFert,
                    370 => Mod.KeyRetain,
                    371 => Mod.KeyRetain,
                    920 => Mod.KeyRetain,
                    465 => Mod.KeySpeed,
                    466 => Mod.KeySpeed,
                    918 => Mod.KeySpeed,
                    _ => ""
                };

                if (hoe_dirt.modData.ContainsKey(key))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

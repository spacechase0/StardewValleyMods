using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace MultiFertilizer.Patches
{
    /// <summary>Applies Harmony patches to <see cref="SObject"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class ObjectPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.canBePlacedHere)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_CanBePlacedHere))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="SObject.canBePlacedHere"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_CanBePlacedHere(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            bool stopCaring = false;
            int fertCategoryCounter = 0;

            // When we find the second -19, after the next instruction:
            // Place our patched section function call. If it returns true, return from the function false.

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
                    fertCategoryCounter++;
                }
                else if (fertCategoryCounter == 2)
                {
                    newInsns.Add(insn);

                    var branchPastOld = new CodeInstruction(OpCodes.Br, insn.operand);
                    branchPastOld.labels.Add(gen.DefineLabel());

                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_1));
                    newInsns.Add(new CodeInstruction(OpCodes.Ldarg_2));
                    newInsns.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<ObjectPatcher>(nameof(CanBePlacedHereLogic))));

                    newInsns.Add(new CodeInstruction(OpCodes.Brfalse, branchPastOld.labels[0]));

                    newInsns.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
                    newInsns.Add(new CodeInstruction(OpCodes.Ret));

                    newInsns.Add(branchPastOld);

                    ++fertCategoryCounter;
                    stopCaring = true;
                }
                else
                    newInsns.Add(insn);
            }

            return newInsns;
        }

        private static bool CanBePlacedHereLogic(SObject __instance, GameLocation l, Vector2 tile)
        {
            if (l.isTileHoeDirt(tile))
            {
                int level = 0;
                string key = "";
                switch (__instance.ParentSheetIndex)
                {
                    case 368: level = 1; key = Mod.KEY_FERT; break;
                    case 369: level = 2; key = Mod.KEY_FERT; break;
                    case 919: level = 3; key = Mod.KEY_FERT; break;
                    case 370: level = 1; key = Mod.KEY_RETAIN; break;
                    case 371: level = 2; key = Mod.KEY_RETAIN; break;
                    case 920: level = 3; key = Mod.KEY_RETAIN; break;
                    case 465: level = 1; key = Mod.KEY_SPEED; break;
                    case 466: level = 2; key = Mod.KEY_SPEED; break;
                    case 918: level = 3; key = Mod.KEY_SPEED; break;
                }

                if (__instance.ParentSheetIndex == 805)
                {
                    return true;
                }
                if (l.terrainFeatures.ContainsKey(tile) && l.terrainFeatures[tile] is HoeDirt && (l.terrainFeatures[tile] as HoeDirt).modData.ContainsKey(key))
                {
                    return true;
                }
                if (l.objects.ContainsKey(tile) && l.objects[tile] is IndoorPot && (l.objects[tile] as IndoorPot).hoeDirt.Value.modData.ContainsKey(key))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

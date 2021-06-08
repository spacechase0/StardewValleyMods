using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace BuildableLocationsFramework.Patches
{
    /// <summary>Applies Harmony patches to <see cref="GameLocation"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class GameLocationPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.carpenters)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_Carpenters))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="GameLocation.carpenters"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_Carpenters(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            Log.info("Transpiling " + original);
            List<CodeInstruction> ret = new List<CodeInstruction>();

            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Callvirt && insn.operand is MethodInfo info)
                {
                    if (info.DeclaringType == typeof(BuildableGameLocation) && info.Name == "isThereABuildingUnderConstruction")
                    {
                        Log.debug("Found isThereABuildingUnderConstruction, replacing...");
                        var newInsn = new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<GameLocationPatcher>(nameof(IsAnyBuildingUnderConstruction)));
                        ret.Add(newInsn);
                        continue;
                    }
                }
                ret.Add(insn);
            }

            return ret;
        }

        private static bool IsAnyBuildingUnderConstruction(Farm originalParam)
        {
            foreach (var loc in Mod.GetAllLocations())
            {
                if (loc is BuildableGameLocation bgl)
                {
                    if (bgl.isThereABuildingUnderConstruction())
                        return true;
                }
            }

            return false;
        }
    }
}

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace BuildableLocationsFramework.Patches
{
    /// <summary>Applies Harmony patches to <see cref="GameLocation"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class GameLocationPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
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
        private static IEnumerable<CodeInstruction> Transpile_Carpenters(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.MethodReplacer(
                from: PatchHelper.RequireMethod<BuildableGameLocation>(nameof(BuildableGameLocation.isThereABuildingUnderConstruction)),
                to: PatchHelper.RequireMethod<GameLocationPatcher>(nameof(IsAnyBuildingUnderConstruction))
            );
        }

        private static bool IsAnyBuildingUnderConstruction()
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

using System.Diagnostics.CodeAnalysis;
using Harmony;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;

namespace MultiFertilizer.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Crop"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class CropPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Crop>(nameof(Crop.harvest)),
                prefix: this.GetHarmonyMethod(nameof(Before_Harvest)),
                postfix: this.GetHarmonyMethod(nameof(After_Harvest))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Crop.harvest"/>.</summary>
        private static void Before_Harvest(Crop __instance, int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester)
        {
            if (!soil.modData.TryGetValue(Mod.KeyFert, out string fertilizerData))
                return;

            int index = int.Parse(fertilizerData) switch
            {
                1 => 368,
                2 => 369,
                3 => 919,
                _ => 0
            };

            soil.fertilizer.Value = index;
        }

        /// <summary>The method to call after <see cref="Crop.harvest"/>.</summary>
        private static void After_Harvest(Crop __instance, int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester)
        {
            soil.fertilizer.Value = 0;
        }
    }
}

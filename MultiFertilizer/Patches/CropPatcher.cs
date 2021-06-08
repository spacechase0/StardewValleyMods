using System.Diagnostics.CodeAnalysis;
using Harmony;
using Spacechase.Shared.Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;

namespace MultiFertilizer.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Crop"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
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
            if (!soil.modData.ContainsKey(Mod.KEY_FERT))
                return;

            int index = 0;
            switch (int.Parse(soil.modData[Mod.KEY_FERT]))
            {
                case 1: index = 368; break;
                case 2: index = 369; break;
                case 3: index = 919; break;
            }

            soil.fertilizer.Value = index;
        }

        /// <summary>The method to call after <see cref="Crop.harvest"/>.</summary>
        private static void After_Harvest(Crop __instance, int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester)
        {
            soil.fertilizer.Value = 0;
        }
    }
}

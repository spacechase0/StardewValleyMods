using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Utility"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class UtilityPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.isViableSeedSpot)),
                prefix: this.GetHarmonyMethod(nameof(Before_IsViableSeedSpot))
            );

            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.getAllFurnituresForFree)),
                postfix: this.GetHarmonyMethod(nameof(After_GetAllFurnituresForFree))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Utility.isViableSeedSpot"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_IsViableSeedSpot(GameLocation location, Vector2 tileLocation, Item item, ref bool __result)
        {
            if (item is CustomObject obj && !string.IsNullOrEmpty(obj.Data.Plants))
            {
                __result = UtilityPatcher.Impl(location, tileLocation, obj);
                return false;
            }
            return true;
        }

        private static bool Impl(GameLocation location, Vector2 tileLocation, CustomObject item)
        {
            if ((!location.terrainFeatures.ContainsKey(tileLocation) || location.terrainFeatures[tileLocation] is not HoeDirt || !item.CanPlantThisSeedHere(((HoeDirt)location.terrainFeatures[tileLocation]), (int)tileLocation.X, (int)tileLocation.Y)) && (!location.objects.ContainsKey(tileLocation) || location.objects[tileLocation] is not IndoorPot || !item.CanPlantThisSeedHere((location.objects[tileLocation] as IndoorPot).hoeDirt.Value, (int)tileLocation.X, (int)tileLocation.Y) || (item as StardewValley.Object).ParentSheetIndex == 499))
            {
                if (location.isTileHoeDirt(tileLocation) || !location.terrainFeatures.ContainsKey(tileLocation))
                {
                    return false; // StardewValley.Object.isWildTreeSeed( item.parentSheetIndex );
                }
                return false;
            }
            return true;
        }

        /// <summary>The method to call after <see cref="Utility.getAllFurnituresForFree"/>.</summary>
        private static void After_GetAllFurnituresForFree(Dictionary<ISalable, int[]> __result)
        {
            foreach (var pack in Mod.contentPacks)
            {
                foreach (var data in pack.Value.items.Values)
                {
                    if (!data.Enabled)
                        continue;

                    var item = data.ToItem();
                    if (item != null && item is Furniture && data is FurniturePackData furnData && furnData.ShowInCatalogue)
                    {
                        __result.Add(item, new int[2] { 0, 2147483647 });
                    } 
                }
            }
        }
    }
}

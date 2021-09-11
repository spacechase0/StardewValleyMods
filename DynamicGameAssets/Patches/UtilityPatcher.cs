using DynamicGameAssets.Game;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace DynamicGameAssets.Patches
{
    [HarmonyPatch(typeof(Utility), nameof(Utility.isViableSeedSpot))]
    public static class UtilityIsViableSeedSpotPatch
    {
        public static bool Prefix(GameLocation location, Vector2 tileLocation, Item item, ref bool __result)
        {
            if (item is CustomObject cobj && !string.IsNullOrEmpty(cobj.Data.Plants))
            {
                __result = UtilityIsViableSeedSpotPatch.Impl(location, tileLocation, cobj);
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
    }
}

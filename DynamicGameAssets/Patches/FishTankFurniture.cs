using DynamicGameAssets.Game;
using HarmonyLib;
using StardewValley.Objects;
using static StardewValley.Objects.FishTankFurniture;

namespace DynamicGameAssets.Patches
{
    [HarmonyPatch(typeof(FishTankFurniture), nameof(FishTankFurniture.GetCapacityForCategory))]
    public static class FishTankCapacityPatch
    {
        public static bool Prefix(FishTankFurniture __instance, FishTankCategories category, ref int __result)
        {
            if (__instance is CustomFishTankFurniture cftf)
            {
                __result = cftf.GetCapacityForCategory(category);
                return false;
            }

            return true;
        }
    }
}

using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;

namespace JsonAssets.Overrides
{
    public class ObjectCanPlantHereOverride
    {
        public static bool Prefix(StardewValley.Object __instance, GameLocation l, Vector2 tile, ref bool __result)
        {
            if (!__instance.bigCraftable.Value && Mod.instance.objectIds.Values.Contains(__instance.ParentSheetIndex))
            {
                if (__instance.Category == StardewValley.Object.SeedsCategory)
                {
                    bool isTree = false;
                    foreach (var tree in Mod.instance.fruitTrees)
                    {
                        if (tree.sapling.id == __instance.ParentSheetIndex)
                        {
                            isTree = true;
                            break;
                        }
                    }

                    var lobj = l.objects.ContainsKey(tile) ? l.objects[tile] : null;
                    if (isTree)
                    {
                        __result = lobj == null && !l.isTileOccupiedForPlacement(tile, __instance);
                        return false;
                    }
                    else
                    {
                        if (l.isTileHoeDirt(tile) || (lobj is IndoorPot))
                            __result = l.isTileOccupiedForPlacement(tile);
                        else
                            __result = false;
                        return false;
                    }
                }
                return true;
            }
            else
                return true;
        }
    }
}

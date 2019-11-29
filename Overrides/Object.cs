using JsonAssets.Data;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

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
                            __result = !l.isTileOccupiedForPlacement(tile, __instance);
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

    public static class ObjectNoActionHook
    {
        public static bool Prefix(StardewValley.Object __instance)
        {
            if (__instance.bigCraftable.Value && Mod.instance.bigCraftableIds.Values.Contains(__instance.ParentSheetIndex))
                return false;
            return true;
        }
    }

    public static class ObjectCollectionShippingHook
    {
        public static void Postfix(int index, ref bool __result)
        {
            foreach (var ring in Mod.instance.myRings)
            {
                if (ring.GetObjectId() == index)
                {
                    __result = false;
                    break;
                }
            }
        }
    }

    public static class ObjectDisplayNameHook
    {
        public static bool Prefix(StardewValley.Object __instance, ref string __result)
        {
            if (!__instance.Name?.Contains("Honey") == true)
                return true;

            if ( !__instance.bigCraftable.Value && Mod.instance.objectIds.Values.Contains(__instance.ParentSheetIndex) )
            {
                string str;
                Game1.objectInformation.TryGetValue(__instance.ParentSheetIndex, out str);
                if (!string.IsNullOrEmpty(str))
                    __result = str.Split('/')[4];
                return false;
            }
            else if (__instance.bigCraftable.Value && Mod.instance.bigCraftableIds.Values.Contains(__instance.ParentSheetIndex) )
            {
                string str;
                Game1.bigCraftablesInformation.TryGetValue(__instance.ParentSheetIndex, out str);
                if (!string.IsNullOrEmpty(str))
                {
                    string[] strArray = str.Split('/');
                    __result = strArray[strArray.Length - 1];
                }
                return false;
            }

            return true;
        }
    }

    public static class ObjectCategoryTextOverride
    {
        public static bool Prefix(StardewValley.Object __instance, ref string __result )
        {
            ObjectData objData = null;
            foreach ( var obj in Mod.instance.objects )
            {
                if ( obj.GetObjectId() == __instance.ParentSheetIndex )
                {
                    objData = obj;
                    break;
                }
            }

            if ( objData != null && objData.CategoryTextOverride != null)
            {
                __result = objData.CategoryTextOverride;
                return false;
            }

            return true;
        }
    }
    public static class ObjectCategoryColorOverride
    {
        public static bool Prefix(StardewValley.Object __instance, ref Color __result)
        {
            ObjectData objData = null;
            foreach (var obj in Mod.instance.objects)
            {
                if (obj.GetObjectId() == __instance.ParentSheetIndex)
                {
                    objData = obj;
                    break;
                }
            }

            if (objData != null && objData.CategoryColorOverride.A != 0)
            {
                __result = objData.CategoryColorOverride;
                return false;
            }

            return true;
        }
    }
}

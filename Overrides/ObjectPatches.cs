using System;
using System.Diagnostics.CodeAnalysis;
using JsonAssets.Data;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace JsonAssets.Overrides
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is set by Harmony.")]
    public class ObjectPatches
    {
        public static bool CanBePlacedHere_Prefix(SObject __instance, GameLocation l, Vector2 tile, ref bool __result)
        {
            try
            {
                if (!__instance.bigCraftable.Value && Mod.instance.objectIds.Values.Contains(__instance.ParentSheetIndex))
                {
                    if (__instance.Category == SObject.SeedsCategory)
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

                        var tileObj = l.objects.ContainsKey(tile) ? l.objects[tile] : null;
                        if (isTree)
                        {
                            __result = tileObj == null && !l.isTileOccupiedForPlacement(tile, __instance);
                            return false;
                        }
                        else
                        {
                            if (l.isTileHoeDirt(tile) || tileObj is IndoorPot)
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
            catch (Exception ex)
            {
                Log.error($"Failed in {nameof(CanBePlacedHere_Prefix)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
                return true;
            }
        }

        public static bool CheckForAction_Prefix(SObject __instance)
        {
            try
            {
                if (__instance.bigCraftable.Value && Mod.instance.bigCraftableIds.Values.Contains(__instance.ParentSheetIndex) && __instance.name.Contains("Chair"))
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.error($"Failed in {nameof(CheckForAction_Prefix)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
                return true;
            }
        }

        public static bool LoadDisplayName_Prefix(SObject __instance, ref string __result)
        {
            try
            {
                if (!__instance.Name?.Contains("Honey") == true)
                    return true;

                if (!__instance.bigCraftable.Value && Mod.instance.objectIds.Values.Contains(__instance.ParentSheetIndex))
                {
                    Game1.objectInformation.TryGetValue(__instance.ParentSheetIndex, out string str);
                    if (!string.IsNullOrEmpty(str))
                        __result = str.Split('/')[4];
                    return false;
                }
                else if (__instance.bigCraftable.Value && Mod.instance.bigCraftableIds.Values.Contains(__instance.ParentSheetIndex))
                {
                    Game1.bigCraftablesInformation.TryGetValue(__instance.ParentSheetIndex, out string str);
                    if (!string.IsNullOrEmpty(str))
                    {
                        string[] strArray = str.Split('/');
                        __result = strArray[strArray.Length - 1];
                    }
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.error($"Failed in {nameof(LoadDisplayName_Prefix)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
                return true;
            }
        }

        public static bool GetCategoryName_Prefix(SObject __instance, ref string __result)
        {
            try
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

                if (objData?.CategoryTextOverride != null)
                {
                    __result = objData.CategoryTextOverride;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.error($"Failed in {nameof(GetCategoryName_Prefix)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
                return true;
            }
        }

        public static void IsIndexOkForBasicShippedCategory_Postfix(int index, ref bool __result)
        {
            try
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
            catch (Exception ex)
            {
                Log.error($"Failed in {nameof(IsIndexOkForBasicShippedCategory_Postfix)} for #{index}:\n{ex}");
            }
        }

        public static bool GetCategoryColor_Prefix(SObject __instance, ref Color __result)
        {
            try
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
            catch (Exception ex)
            {
                Log.error($"Failed in {nameof(GetCategoryColor_Prefix)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
                return true;
            }
        }
    }
}

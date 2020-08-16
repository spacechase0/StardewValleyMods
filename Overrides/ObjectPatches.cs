using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Harmony;
using JsonAssets.Data;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewValley;
using StardewValley.Network;
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
                if ( Mod.instance.objectIds.Values.Contains(index) )
                {
                    var obj = new List<ObjectData>(Mod.instance.objects).Find(od => od.GetObjectId() == index);
                    if ( obj != null && !obj.CanSell )
                        __result = false;
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
        
        public static void CanBeGivenAsGift_Postfix(StardewValley.Object __instance, ref bool __result)
        {
            try
            {
                if (!__instance.bigCraftable.Value && Mod.instance.objectIds.Values.Contains(__instance.ParentSheetIndex))
                {
                    var obj = new List<ObjectData>(Mod.instance.objects).Find(od => od.GetObjectId() == __instance.ParentSheetIndex);
                    if (obj != null && !obj.CanBeGifted)
                        __result = false;
                }
            }
            catch (Exception ex)
            {
                Log.error($"Failed in {nameof(CanBeGivenAsGift_Postfix)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.isPlaceable))]
    public static class ObjectIsPlaceablePatch
    {
        public static bool Prefix( StardewValley.Object __instance, ref bool __result )
        {
            if ( __instance.bigCraftable.Value )
                return true;

            if ( __instance.Category == StardewValley.Object.CraftingCategory && Mod.instance.objectIds.Values.Contains( __instance.ParentSheetIndex ) )
            {
                if ( !Mod.instance.fences.Any( f => f.correspondingObject.id == __instance.ParentSheetIndex ) )
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction))]
    public static class ObjectPlacementActionPatch
    {
        public static bool Prefix(StardewValley.Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result )
        {
            Vector2 pos = new Vector2( x / 64, y / 64 );
            if ( !__instance.bigCraftable.Value && !(__instance is Furniture) )
            {
                foreach ( var fence in Mod.instance.fences )
                {
                    if ( __instance.ParentSheetIndex == fence.correspondingObject.GetObjectId() )
                    {
                        if ( location.objects.ContainsKey( pos ) )
                        {
                            __result = false;
                            return false;
                        }
                        location.objects.Add( pos, new Fence( pos, fence.correspondingObject.GetObjectId(), false ) );
                        location.playSound( fence.PlacementSound, NetAudio.SoundContext.Default );
                        __result = true;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

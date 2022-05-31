using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using JsonAssets.Data;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace JsonAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="SObject"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ObjectPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.canBePlacedHere)),
                prefix: this.GetHarmonyMethod(nameof(Before_CanBePlacedHere))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.checkForAction)),
                prefix: this.GetHarmonyMethod(nameof(Before_CheckForAction))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>("loadDisplayName"),
                prefix: this.GetHarmonyMethod(nameof(Before_LoadDisplayName))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.getCategoryName)),
                prefix: this.GetHarmonyMethod(nameof(Before_GetCategoryName))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.isIndexOkForBasicShippedCategory)),
                postfix: this.GetHarmonyMethod(nameof(After_IsIndexOkForBasicShippedCategory))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.getCategoryColor)),
                prefix: this.GetHarmonyMethod(nameof(Before_GetCategoryColor))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.canBeGivenAsGift)),
                postfix: this.GetHarmonyMethod(nameof(After_CanBeGivenAsGift))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.isPlaceable)),
                prefix: this.GetHarmonyMethod(nameof(Before_IsPlaceable))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.placementAction)),
                prefix: this.GetHarmonyMethod(nameof(Before_PlacementAction))
            );

        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="SObject.canBePlacedHere"/>.</summary>
        public static bool Before_CanBePlacedHere(SObject __instance, GameLocation l, Vector2 tile, ref bool __result)
        {
            try
            {
                if (!__instance.bigCraftable.Value && Mod.instance.ObjectIds.Values.Contains(__instance.ParentSheetIndex))
                {
                    if (__instance.Category == SObject.SeedsCategory)
                    {
                        bool isTree = false;
                        foreach (var tree in Mod.instance.FruitTrees)
                        {
                            if (tree.Sapling.Id == __instance.ParentSheetIndex)
                            {
                                isTree = true;
                                break;
                            }
                        }

                        if (!l.objects.TryGetValue(tile, out SObject tileObj))
                            tileObj = null;

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
                Log.Error($"Failed in {nameof(Before_CanBePlacedHere)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
                return true;
            }
        }

        /// <summary>The method to call before <see cref="SObject.checkForAction"/>.</summary>
        public static bool Before_CheckForAction(SObject __instance)
        {
            try
            {
                if (__instance.bigCraftable.Value && Mod.instance.BigCraftableIds.Values.Contains(__instance.ParentSheetIndex) && __instance.name.Contains("Chair"))
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(Before_CheckForAction)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
                return true;
            }
        }

        /// <summary>The method to call before <see cref="SObject.loadDisplayName"/>.</summary>
        public static bool Before_LoadDisplayName(SObject __instance, ref string __result)
        {
            try
            {
                if (!__instance.Name?.Contains("Honey") == true)
                    return true;

                if (Mod.instance.ObjectIds == null)
                    return true;

                if (!__instance.bigCraftable.Value && Mod.instance.ObjectIds.Values.Contains(__instance.ParentSheetIndex))
                {
                    Game1.objectInformation.TryGetValue(__instance.ParentSheetIndex, out string str);
                    if (!string.IsNullOrEmpty(str))
                        __result = str.Split('/')[4];
                    return false;
                }
                else if (__instance.bigCraftable.Value && Mod.instance.BigCraftableIds.Values.Contains(__instance.ParentSheetIndex))
                {
                    Game1.bigCraftablesInformation.TryGetValue(__instance.ParentSheetIndex, out string str);
                    if (!string.IsNullOrEmpty(str))
                    {
                        int index = str.LastIndexOf('/');
                        if (index >= 0)
                            __result = str[(index + 1)..];
                    }
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(Before_LoadDisplayName)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
                return true;
            }
        }

        /// <summary>The method to call before <see cref="SObject.getCategoryName"/>.</summary>
        public static bool Before_GetCategoryName(SObject __instance, ref string __result)
        {
            try
            {
                ObjectData objData = null;
                foreach (var obj in Mod.instance.Objects)
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
                Log.Error($"Failed in {nameof(Before_GetCategoryName)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
                return true;
            }
        }

        /// <summary>The method to call after <see cref="SObject.isIndexOkForBasicShippedCategory"/>.</summary>
        public static void After_IsIndexOkForBasicShippedCategory(int index, ref bool __result)
        {
            try
            {
                foreach (var ring in Mod.instance.MyRings)
                {
                    if (ring.GetObjectId() == index)
                    {
                        __result = false;
                        break;
                    }
                }
                if (Mod.instance.ObjectIds.Values.Contains(index))
                {
                    var obj = new List<ObjectData>(Mod.instance.Objects).Find(od => od.GetObjectId() == index);
                    if (obj != null && (!obj.CanSell || obj.HideFromShippingCollection))
                        __result = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(After_IsIndexOkForBasicShippedCategory)} for #{index}:\n{ex}");
            }
        }

        /// <summary>The method to call before <see cref="SObject.getCategoryColor"/>.</summary>
        public static bool Before_GetCategoryColor(SObject __instance, ref Color __result)
        {
            try
            {
                ObjectData objData = null;
                foreach (var obj in Mod.instance.Objects)
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
                Log.Error($"Failed in {nameof(Before_GetCategoryColor)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
                return true;
            }
        }

        /// <summary>The method to call after <see cref="SObject.canBeGivenAsGift"/>.</summary>
        public static void After_CanBeGivenAsGift(SObject __instance, ref bool __result)
        {
            try
            {
                if (!__instance.bigCraftable.Value && Mod.instance.ObjectIds.Values.Contains(__instance.ParentSheetIndex))
                {
                    var obj = new List<ObjectData>(Mod.instance.Objects).Find(od => od.GetObjectId() == __instance.ParentSheetIndex);
                    if (obj?.CanBeGifted == false)
                        __result = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(After_CanBeGivenAsGift)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
            }
        }

        /// <summary>The method to call before <see cref="SObject.isPlaceable"/>.</summary>
        public static bool Before_IsPlaceable(SObject __instance, ref bool __result)
        {
            if (__instance.bigCraftable.Value)
                return true;

            if (__instance.Category == SObject.CraftingCategory && Mod.instance.ObjectIds.Values.Contains(__instance.ParentSheetIndex))
            {
                if (Mod.instance.Fences.All(f => f.CorrespondingObject.Id != __instance.ParentSheetIndex))
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }

        /// <summary>The method to call before <see cref="SObject.placementAction"/>.</summary>
        public static bool Before_PlacementAction(SObject __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            Vector2 pos = new Vector2(x / 64, y / 64);
            if (!__instance.bigCraftable.Value && __instance is not Furniture)
            {
                foreach (var fence in Mod.instance.Fences)
                {
                    if (__instance.ParentSheetIndex == fence.CorrespondingObject.GetObjectId())
                    {
                        if (location.objects.ContainsKey(pos))
                        {
                            __result = false;
                            return false;
                        }
                        location.objects.Add(pos, new Fence(pos, fence.CorrespondingObject.GetObjectId(), false));
                        location.playSound(fence.PlacementSound);
                        __result = true;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using HarmonyLib;

using JsonAssets.Data;
using JsonAssets.Framework;

using Microsoft.Xna.Framework;

using Spacechase.Shared.Patching;

using SpaceCore.Framework.Extensions;

using SpaceShared;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

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
                original: this.RequireMethod<SObject>("loadDisplayName"),
                prefix: this.GetHarmonyMethod(nameof(Before_LoadDisplayName))
                ); ;

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.canBePlacedHere)),
                postfix: this.GetHarmonyMethod(nameof(After_CanBePlacedHere))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.isSapling)),
                postfix: this.GetHarmonyMethod(nameof(After_IsSapling))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.checkForAction)),
                prefix: this.GetHarmonyMethod(nameof(Before_CheckForAction))
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
                postfix: this.GetHarmonyMethod(nameof(After_IsPlaceable))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.placementAction)),
                prefix: this.GetHarmonyMethod(nameof(Before_PlacementAction))
            );

        }

        /*********
        ** Private methods
        *********/

        private static bool Before_LoadDisplayName(SObject __instance, ref string __result)
        {
            if (__instance.bigCraftable.Value)
            {
                if (BigCraftableData.HasHoneyInName.Contains(__instance.ParentSheetIndex)
                    && Game1.bigCraftablesInformation.TryGetValue(__instance.ParentSheetIndex, out string data))
                {
                    int index = data.LastIndexOf('/');
                    if (index > 0)
                    {
                        __result = data[(index + 1)..]; // big craftables keep their display name as the last field.
                        return false;
                    }
                }
            }
            else
            {
                if (ObjectData.HasHoneyInName.Contains(__instance.ParentSheetIndex)
                    && Game1.objectInformation.TryGetValue(__instance.ParentSheetIndex, out string data))
                {
                    string name = data.GetNthChunk('/', SObject.objectInfoDisplayNameIndex).ToString();
                    if (name.Length != 0)
                    {
                        __result = name;
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>The method to call before <see cref="SObject.canBePlacedHere(GameLocation, Vector2)"/>.</summary>
        /// <remarks>This method doesn't check IsSapling.</remarks>
        public static void After_CanBePlacedHere(SObject __instance, GameLocation l, Vector2 tile, ref bool __result)
        {
            if (__result)
                return;

            if (FruitTreeData.SaplingIds.Contains(__instance.ParentSheetIndex))
            {
                if (!l.isTileOccupiedForPlacement(tile, __instance))
                {
                    if (l.CanPlantTreesHere(__instance.ParentSheetIndex, (int)tile.X, (int)tile.Y) || l.IsOutdoors)
                        __result = !FruitTree.IsGrowthBlocked(tile, l); // this sucks. This is a very expensive call.
                }
            }
        }


        /// <summary>The method to call before <see cref="SObject.isSapling"/>.</summary>
        /// <remarks>Ensure that JA saplings are considered saplings.</remarks>
        public static void After_IsSapling(SObject __instance, ref bool __result)
        {
            try
            {
                if (!__result && !__instance.bigCraftable.Value && __instance.GetType() == typeof(SObject) && FruitTreeData.SaplingIds.Contains(__instance.ParentSheetIndex))
                {
                    __result = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(After_IsSapling)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
            }
        }

        /// <summary>The method to call before <see cref="SObject.checkForAction"/>.</summary>
        /// <remarks>Game has special handling for chairs we probably don't want for JA items in general.</remarks>
        public static bool Before_CheckForAction(SObject __instance)
        {
            try
            {
                if (__instance.bigCraftable.Value && __instance.name.Contains("Chair") && Mod.instance.BigCraftableIds.Values.Contains(__instance.ParentSheetIndex))
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(Before_CheckForAction)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
                return true;
            }
        }

        /// <summary>The method to call before <see cref="SObject.getCategoryName"/>.</summary>
        /// <remarks>Used for <see cref="ObjectData.CategoryTextOverride"></remarks>
        public static bool Before_GetCategoryName(SObject __instance, ref string __result)
        {
            try
            {
                foreach (ObjectData obj in Mod.instance.Objects)
                {
                    if (obj.GetObjectId() == __instance.ParentSheetIndex)
                    {
                        if (obj.CategoryTextOverride is not null)
                        {
                            __result = obj.CategoryTextOverride;
                            return false;
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(Before_GetCategoryName)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
            }
            return true;
        }

        /// <summary>The method to call after <see cref="SObject.isIndexOkForBasicShippedCategory"/>.</summary>
        public static void After_IsIndexOkForBasicShippedCategory(int index, ref bool __result)
        {
            try
            {
                if (ObjectData.TrackedRings.Contains(index))
                {
                    __result = false;
                    return;
                }
                var objData = Mod.instance.Objects.FirstOrDefault((obj) => obj.GetObjectId() == index);
                if (objData is not null && (!objData.CanSell || objData.HideFromShippingCollection))
                {
                    __result = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(After_IsIndexOkForBasicShippedCategory)} for #{index}:\n{ex}");
            }
        }

        /// <summary>The method to call before <see cref="SObject.getCategoryColor"/>.</summary>
        /// <remarks>Used for <see cref="ObjectData.CategoryColorOverride"/></remarks>
        public static bool Before_GetCategoryColor(SObject __instance, ref Color __result)
        {
            try
            {
                foreach (var obj in Mod.instance.Objects)
                {
                    if (obj.GetObjectId() == __instance.ParentSheetIndex)
                    {
                        if (obj.CategoryColorOverride.A != 0)
                        {
                            __result = obj.CategoryColorOverride;
                            return false;
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(Before_GetCategoryColor)} for #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
            }
            return true;
        }

        /// <summary>The method to call after <see cref="SObject.canBeGivenAsGift"/>.</summary>
        public static void After_CanBeGivenAsGift(SObject __instance, ref bool __result)
        {
            try
            {
                if (__result && !__instance.bigCraftable.Value)
                {
                    var obj = Mod.instance.Objects.FirstOrDefault(od => od.GetObjectId() == __instance.ParentSheetIndex);
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
        public static void After_IsPlaceable(SObject __instance, ref bool __result)
        {
            if (__instance.bigCraftable.Value)
            {
                __result = true;
                return;
            }

            if (__instance.Category == SObject.CraftingCategory)
            {
                if (ContentInjector1.FenceIndexes.ContainsKey(__instance.ParentSheetIndex))
                {
                    __result = true;
                    return;
                }
                else if (__result && Mod.instance.ObjectIds.Values.Contains(__instance.ParentSheetIndex))
                {
                    __result = false;
                    return;
                }
            }
        }

        /// <summary>The method to call before <see cref="SObject.placementAction"/>.</summary>
        public static bool Before_PlacementAction(SObject __instance, GameLocation location, int x, int y, ref bool __result)
        {
            if (!__instance.bigCraftable.Value && __instance is not Furniture && ContentInjector1.FenceIndexes.ContainsKey(__instance.ParentSheetIndex))
            {
                Vector2 pos = new(x / 64, y / 64);
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

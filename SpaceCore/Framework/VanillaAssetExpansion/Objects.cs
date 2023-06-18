using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;

namespace SpaceCore.Framework.VanillaAssetExpansion
{
    public class ObjectExtensionData
    {
        public string CategoryTextOverride { get; set; } = null;
        public Color? CategoryColorOverride { get; set; } = null;

        public bool HideFromShippingCollection { get; set; } = false;

        public bool CanBeTrashed { get; set; } = true;
        public bool CanBeGifted { get; set; } = true;
        public bool CanBeShipped { get; set; } = true;

        public int? EatenHealthRestoredOverride { get; set; } = null;
        public int? EatenStaminaRestoredOverride { get; set; } = null;

        public int? MaxStackSizeOverride { get; set; } = null;

        // totem warp?

        // might make you able to run arbritrary scripts?
        //public string UsageScriptPath { get; set; } = null;
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.getCategoryName))]
    public static class ObjectCategoryNamePatch
    {
        public static void Postfix(StardewValley.Object __instance, ref string __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId))
            {
                __result = dict[__instance.ItemId].CategoryTextOverride;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.getCategoryColor))]
    public static class ObjectCategoryColorPatch
    {
        public static void Postfix(StardewValley.Object __instance, ref Color __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && dict[__instance.ItemId].CategoryColorOverride.HasValue)
            {
                __result = dict[__instance.ItemId].CategoryColorOverride.Value;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.isIndexOkForBasicShippedCategory))]
    public static class ObjectHiddenInShippingCollectionPatch
    {
        public static void Postfix(string itemId, ref bool __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(itemId) && ( !dict[itemId].CanBeShipped || !dict[itemId].HideFromShippingCollection ) )
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.canBeTrashed))]
    public static class ObjectTrashablePatch
    {
        public static void Postfix(StardewValley.Object __instance, ref bool __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && !dict[__instance.ItemId].CanBeTrashed)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Item), nameof(StardewValley.Item.canBeDropped))]
    public static class ObjectDroppablePatch
    {
        public static void Postfix(StardewValley.Item __instance, ref bool __result)
        {
            if (__instance is StardewValley.Object obj)
            ObjectTrashablePatch.Postfix(obj, ref __result);
        }
    }


    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.canBeGivenAsGift))]
    public static class ObjectGiftablePatch
    {
        public static void Postfix(StardewValley.Object __instance, ref bool __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && !dict[__instance.ItemId].CanBeGifted)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.canBeShipped))]
    public static class ObjectShippablePatch
    {
        public static void Postfix(StardewValley.Object __instance, ref bool __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && !dict[__instance.ItemId].CanBeShipped)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.healthRecoveredOnConsumption))]
    public static class ObjectHealthRecoveredPatch
    {
        public static void Postfix(StardewValley.Object __instance, ref int __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && dict[__instance.ItemId].EatenHealthRestoredOverride.HasValue)
            {
                __result = dict[__instance.ItemId].EatenHealthRestoredOverride.Value;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.staminaRecoveredOnConsumption))]
    public static class ObjectStaminaRecoveredPatch
    {
        public static void Postfix(StardewValley.Object __instance, ref int __result)
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && dict[__instance.ItemId].EatenStaminaRestoredOverride.HasValue)
            {
                __result = dict[__instance.ItemId].EatenStaminaRestoredOverride.Value;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.maximumStackSize))]
    public static class ObjectMaxStackPatch
    {
        public static void Postfix(StardewValley.Object __instance, ref int __result )
        {
            var dict = Game1.content.Load<Dictionary<string, ObjectExtensionData>>("spacechase0.SpaceCore/ObjectExtensionData");
            if (dict.ContainsKey(__instance.ItemId) && dict[__instance.ItemId].MaxStackSizeOverride.HasValue)
            {
                __result = dict[__instance.ItemId].MaxStackSizeOverride.Value;
            }
        }
    }
}

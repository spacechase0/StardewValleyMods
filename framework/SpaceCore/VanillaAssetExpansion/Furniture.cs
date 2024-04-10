using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace SpaceCore.VanillaAssetExpansion
{
    public class FurnitureExtensionData
    {
        public Dictionary<Vector2, Dictionary<string, Dictionary<string, string>>> TileProperties { get; set; } = new();
        public string DescriptionOverride { get; set; }
        public Dictionary<int, List<Vector2>> SeatLocations { get; set; } = new();
    }

    [HarmonyPatch(typeof(Furniture), nameof(Furniture.DoesTileHaveProperty))]
    public static class FurnitureTilePropertyExtensionPatch
    {
        public static void Postfix(Furniture __instance, int tile_x, int tile_y, string property_name, string layer_name, ref string property_value, ref bool __result)
        {
            tile_x -= (int)__instance.tileLocation.X;
            tile_y -= (int)__instance.tileLocation.Y;

            var dict = Game1.content.Load<Dictionary<string, FurnitureExtensionData>>("spacechase0.SpaceCore/FurnitureExtensionData");
            if ( dict.TryGetValue( __instance.ItemId, out var furnData ) && furnData.TileProperties != null )
            {
                if ( furnData.TileProperties.TryGetValue( new Vector2( tile_x, tile_y ), out var tileProps ) &&
                     tileProps.TryGetValue( layer_name, out var layerProps ) &&
                     layerProps.TryGetValue( property_name, out string propValue ) )
                {
                    property_value = propValue;
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Furniture), nameof(Furniture.getDescription))]
    public static class FurnitureDescriptionExtensionPatch
    {
        public static void Postfix(Furniture __instance, ref string __result)
        {
            var dict = Game1.content.Load<Dictionary<string, FurnitureExtensionData>>("spacechase0.SpaceCore/FurnitureExtensionData");
            if (dict.TryGetValue(__instance.ItemId, out var furnData) && furnData.DescriptionOverride != null)
            {
                __result = Game1.parseText(TokenParser.ParseText(furnData.DescriptionOverride), Game1.smallFont, 320);
            }
        }
    }

    [HarmonyPatch(typeof(Furniture), nameof(Furniture.GetSeatPositions))]
    public static class FurnitureSeatLocationExtensionPatch
    {
        public static void Postfix(Furniture __instance, ref List<Vector2> __result)
        {
            var dict = Game1.content.Load<Dictionary<string, FurnitureExtensionData>>("spacechase0.SpaceCore/FurnitureExtensionData");
            if (dict.TryGetValue(__instance.ItemId, out var furnData) && furnData.SeatLocations != null)
            {
                if (furnData.SeatLocations.ContainsKey(__instance.currentRotation.Value))
                {
                    __result = furnData.SeatLocations[__instance.currentRotation.Value];
                }
            }
        }
    }

    [HarmonyPatch(typeof(Furniture), nameof(Furniture.GetSeatCapacity))]
    public static class FurnitureSeatCapacityExtensionPatch
    {
        public static void Postfix(Furniture __instance, ref int __result)
        {
            var dict = Game1.content.Load<Dictionary<string, FurnitureExtensionData>>("spacechase0.SpaceCore/FurnitureExtensionData");
            if (dict.TryGetValue(__instance.ItemId, out var furnData) && furnData.SeatLocations != null)
            {
                if (furnData.SeatLocations.ContainsKey(__instance.currentRotation.Value))
                {
                    __result = furnData.SeatLocations[__instance.currentRotation.Value].Count;
                }
            }
        }
    }
}

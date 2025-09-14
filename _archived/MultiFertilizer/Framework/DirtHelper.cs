using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace MultiFertilizer.Framework
{
    /// <summary>Provides utility methods for managing dirt and fertilizer.</summary>
    internal static class DirtHelper
    {
        /*********
        ** Fields
        *********/
        /// <summary>The supported fertilizer types.</summary>
        private static readonly Dictionary<string, FertilizerData> FertilizersById;

        /// <summary>The supported fertilizer types indexed by key and level.</summary>
        private static readonly Dictionary<string, FertilizerData> FertilizersByKeyAndLevel;

        /// <summary>The unique fertilizer keys.</summary>
        private static readonly string[] FertilizerKeys;


        /*********
        ** Public methods
        *********/
        /// <summary>Initialize the static data.</summary>
        static DirtHelper()
        {
            var fertilizers = new FertilizerData[]
            {
                new(id: "368", key: Mod.KeyFert, level: 1, spriteIndex: 0),
                new(id: "369", key: Mod.KeyFert, level: 2, spriteIndex: 1),
                new(id: "919", key: Mod.KeyFert, level: 3, spriteIndex: 2),
                new(id: "370", key: Mod.KeyRetain, level: 1, spriteIndex: 3),
                new(id: "371", key: Mod.KeyRetain, level: 2, spriteIndex: 4),
                new(id: "920", key: Mod.KeyRetain, level: 3, spriteIndex: 5),
                new(id: "465", key: Mod.KeySpeed, level: 1, spriteIndex: 6),
                new(id: "466", key: Mod.KeySpeed, level: 2, spriteIndex: 7),
                new(id: "918", key: Mod.KeySpeed, level: 3, spriteIndex: 8)
            };

            DirtHelper.FertilizersById = fertilizers.ToDictionary(p => p.Id);
            DirtHelper.FertilizersByKeyAndLevel = fertilizers.ToDictionary(p => $"{p.Key}:{p.Level}");
            DirtHelper.FertilizerKeys = fertilizers.Select(p => p.Key).Distinct().ToArray();
        }

        /// <summary>Get the unique fertilizer keys.</summary>
        public static string[] GetFertilizerTypes()
        {
            return DirtHelper.FertilizerKeys;
        }

        /// <summary>Get the fertilizer info for an item ID.</summary>
        /// <param name="itemId">The item ID.</param>
        /// <param name="fertilizer">The fertilizer data.</param>
        /// <returns>Returns whether the item is a recognized fertilizer.</returns>
        public static bool TryGetFertilizer(string itemId, out FertilizerData fertilizer)
        {
            return DirtHelper.FertilizersById.TryGetValue(itemId, out fertilizer);
        }

        /// <summary>Get the fertilizer info for a fertilizer type and level.</summary>
        /// <param name="key">The fertilizer type key.</param>
        /// <param name="level">The fertilizer level.</param>
        /// <param name="fertilizer">The fertilizer data.</param>
        /// <returns>Returns whether a valid fertilizer was found.</returns>
        public static bool TryGetFertilizer(string key, int level, out FertilizerData fertilizer)
        {
            return DirtHelper.FertilizersByKeyAndLevel.TryGetValue($"{key}:{level}", out fertilizer);
        }

        /// <summary>Get the fertilizer info for a dirt tile and fertilizer type.</summary>
        /// <param name="dirt">The dirt tile to check.</param>
        /// <param name="type">The fertilizer type to check for.</param>
        /// <param name="fertilizer">The fertilizer data.</param>
        /// <returns>Returns whether a valid fertilizer was found.</returns>
        public static bool TryGetFertilizer(this HoeDirt dirt, string type, out FertilizerData fertilizer)
        {
            fertilizer = null;
            return
                dirt.modData.TryGetValue(type, out string rawLevel)
                && int.TryParse(rawLevel, out int level)
                && DirtHelper.TryGetFertilizer(type, level, out fertilizer);
        }

        /// <summary>Get the tilled dirt on a tile, if any.</summary>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile to check.</param>
        /// <param name="dirt">The dirt found on the tile.</param>
        /// <param name="includePots">Whether to check for dirt in indoor pots.</param>
        /// <returns>Returns whether any dirt was found.</returns>
        public static bool TryGetDirt(this GameLocation location, Vector2 tile, out HoeDirt dirt, bool includePots = true)
        {
            dirt = null;

            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature) && feature is HoeDirt terrainDirt)
                dirt = terrainDirt;
            else if (includePots && location.objects.TryGetValue(tile, out SObject obj) && obj is IndoorPot pot)
                dirt = pot.hoeDirt.Value;

            return dirt != null;
        }

        /// <summary>Get whether the dirt has any fertilizer of the given type.</summary>
        /// <param name="dirt">The dirt to check.</param>
        /// <param name="fertilizer">The fertilizer data.</param>
        public static bool HasFertilizer(this HoeDirt dirt, FertilizerData fertilizer)
        {
            return fertilizer != null && dirt.HasFertilizer(fertilizer.Key);
        }

        /// <summary>Get whether the dirt has any fertilizer of the given type.</summary>
        /// <param name="dirt">The dirt to check.</param>
        /// <param name="key">The fertilizer type key.</param>
        public static bool HasFertilizer(this HoeDirt dirt, string key)
        {
            return dirt.modData.ContainsKey(key);
        }
    }
}

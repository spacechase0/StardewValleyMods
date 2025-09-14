using System;
using System.Runtime.Serialization;

namespace JsonAssets.Data
{
    public class ForgeRecipeData
    {
        /*********
        ** Accessors
        *********/
        public string EnableWithMod { get; set; }
        public string DisableWithMod { get; set; }

        public string BaseItemName { get; set; } // Checks by Item.Name, so supports anything
        public string IngredientContextTag { get; set; }
        public int CinderShardCost { get; set; }

        public string ResultItemName { get; set; } // Uses Utility.fuzzyItemSearch, so go nuts

        public string[] AbleToForgeConditions { get; set; }


        /*********
        ** Private methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.AbleToForgeConditions ??= Array.Empty<string>();
        }
    }
}

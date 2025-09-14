using System.Collections.Generic;
using System.Runtime.Serialization;
using JsonAssets.Framework;
using Microsoft.Xna.Framework;
using StardewValley.GameData.Crafting;

namespace JsonAssets.Data
{
    public class TailoringRecipeData
    {
        /*********
        ** Accessors
        *********/
        public string EnableWithMod { get; set; }
        public string DisableWithMod { get; set; }

        public IList<string> FirstItemTags { get; set; } = new List<string>(new[] { "item_cloth" });
        public IList<string> SecondItemTags { get; set; } = new List<string>();

        public bool ConsumeSecondItem { get; set; } = true;

        public IList<object> CraftedItems { get; set; } = new List<object>();
        //public Color CraftedItemColor { get; set; } = Color.White;


        /*********
        ** Public methods
        *********/
        public TailorItemRecipe ToGameData()
        {
            var recipe = new TailorItemRecipe
            {
                FirstItemTags = new List<string>(this.FirstItemTags),
                SecondItemTags = new List<string>(this.SecondItemTags),
                SpendRightItem = this.ConsumeSecondItem,
                //CraftedItemColor = $"{this.CraftedItemColor.R} {this.CraftedItemColor.G} {this.CraftedItemColor.B}"
            };

            recipe.CraftedItemIds = new List<string>();
            foreach (object entry in this.CraftedItems)
                recipe.CraftedItemIds.Add(GetCraftedItem(entry.ToString())); // TODO: always uses first crafted item?

            return recipe;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.FirstItemTags ??= new List<string>();
            this.SecondItemTags ??= new List<string>();
            this.CraftedItems ??= new List<object>();

            this.FirstItemTags.FilterNulls();
            this.SecondItemTags.FilterNulls();
            this.CraftedItems.FilterNulls();
        }

        private string GetCraftedItem(string itemName)
        {
            if (itemName.FixIdJA("S") != null)
                return itemName.FixIdJA("S");
            else if (itemName.FixIdJA("P") != null)
                return itemName.FixIdJA("P");
            else if (itemName.FixIdJA("B") != null)
                return itemName.FixIdJA("B");
            else if (!StardewValley.ItemRegistry.GetDataOrErrorItem(itemName).IsErrorItem)
                return StardewValley.ItemRegistry.GetDataOrErrorItem(itemName).ItemId;
            else return itemName;
        }
    }
}

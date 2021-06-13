using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.GameData.Crafting;

namespace JsonAssets.Data
{
    public class TailoringRecipeData
    {
        public string EnableWithMod { get; set; }
        public string DisableWithMod { get; set; }

        public IList<string> FirstItemTags { get; set; } = new List<string>(new[] { "item_cloth" });
        public IList<string> SecondItemTags { get; set; }

        public bool ConsumeSecondItem { get; set; } = true;

        public IList<object> CraftedItems { get; set; }
        public Color CraftedItemColor { get; set; } = Color.White;

        public TailorItemRecipe ToGameData()
        {
            var recipe = new TailorItemRecipe
            {
                FirstItemTags = new List<string>(this.FirstItemTags),
                SecondItemTags = new List<string>(this.SecondItemTags),
                SpendRightItem = this.ConsumeSecondItem,
                CraftedItemColor = $"{this.CraftedItemColor.R} {this.CraftedItemColor.G} {this.CraftedItemColor.B}"
            };

            if (this.CraftedItems.Count > 1)
            {
                recipe.CraftedItemIDs = new List<string>();
                foreach (object entry in this.CraftedItems)
                    recipe.CraftedItemIDs.Add(Mod.instance.ResolveClothingId(this.CraftedItems[0]).ToString());
            }
            else
                recipe.CraftedItemID = Mod.instance.ResolveClothingId(this.CraftedItems[0]);

            return recipe;
        }
    }
}

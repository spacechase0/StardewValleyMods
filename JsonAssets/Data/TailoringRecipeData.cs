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
            var tir = new TailorItemRecipe();
            tir.FirstItemTags = new List<string>(this.FirstItemTags);
            tir.SecondItemTags = new List<string>(this.SecondItemTags);

            tir.SpendRightItem = this.ConsumeSecondItem;

            if (this.CraftedItems.Count > 1)
            {
                tir.CraftedItemIDs = new List<string>();
                foreach (var entry in this.CraftedItems)
                    tir.CraftedItemIDs.Add(Mod.instance.ResolveClothingId(this.CraftedItems[0]).ToString());
            }
            else
                tir.CraftedItemID = Mod.instance.ResolveClothingId(this.CraftedItems[0]);
            tir.CraftedItemColor = $"{this.CraftedItemColor.R} {this.CraftedItemColor.G} {this.CraftedItemColor.B}";

            return tir;
        }
    }
}

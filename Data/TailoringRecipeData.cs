using Microsoft.Xna.Framework;
using StardewValley.GameData.Crafting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Data
{
    public class TailoringRecipeData
    {
        public string DisableWithMod { get; set; }

        public IList<string> FirstItemTags { get; set; } = new List<string>(new string[] { "item_cloth" });
        public IList<string> SecondItemTags { get; set; }

        public bool ConsumeSecondItem { get; set; } = true;

        public IList<object> CraftedItems { get; set; }
        public Color CraftedItemColor { get; set; } = Color.White;

        public TailorItemRecipe ToGameData()
        {
            var tir = new TailorItemRecipe();
            tir.FirstItemTags = new List<string>(FirstItemTags);
            tir.SecondItemTags = new List<string>(SecondItemTags);

            tir.SpendRightItem = ConsumeSecondItem;

            if (CraftedItems.Count > 1)
            {
                tir.CraftedItemIDs = new List<string>();
                foreach (var entry in CraftedItems)
                    tir.CraftedItemIDs.Add(Mod.instance.ResolveClothingId(CraftedItems[0]).ToString());
            }
            else
                tir.CraftedItemID = Mod.instance.ResolveClothingId(CraftedItems[0]);
            tir.CraftedItemColor = $"{CraftedItemColor.R} {CraftedItemColor.G} {CraftedItemColor.B}";

            return tir;
        }
    }
}

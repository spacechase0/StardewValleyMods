using System;
using JsonAssets.Framework;
using StardewValley;

namespace JsonAssets
{
    public class ShopDataEntry
    {
        /*********
        ** Accessors
        *********/
        public string PurchaseFrom { get; set; }
        public int Price { get; set; }
        public IParsedConditions PurchaseRequirements { get; set; }
        public Func<ISalable> Object { get; set; }
        public bool ShowWithStocklist { get; set; } = false;
    }
}

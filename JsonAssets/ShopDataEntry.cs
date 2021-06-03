using System;
using StardewValley;

namespace JsonAssets
{
    public class ShopDataEntry
    {
        public string PurchaseFrom;
        public int Price;
        public string[] PurchaseRequirements;
        public Func<ISalable> Object;
        public bool ShowWithStocklist = false;
    }
}

using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets
{
    public class ShopDataEntry
    {
        public string PurchaseFrom;
        public int Price;
        public string PurchaseRequirements;
        public Func<ISalable> Object;
    }
}

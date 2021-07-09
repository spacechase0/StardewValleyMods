using System;
using System.Collections.Generic;
using System.Linq;
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
        public string[] PurchaseRequirements { get; set; }
        public Func<ISalable> Object { get; set; }
        public bool ShowWithStocklist { get; set; } = false;


        /*********
        ** Public methods
        *********/
        /// <summary>Format individual requirements for the <see cref="PurchaseRequirements"/> property.</summary>
        /// <param name="requirementFields">The purchase requirements.</param>
        public static string[] FormatRequirements(IList<string> requirementFields)
        {
            return requirementFields?.Any() == true
                ? new[] { string.Join("/", requirementFields) }
                : new string[0];
        }
    }
}

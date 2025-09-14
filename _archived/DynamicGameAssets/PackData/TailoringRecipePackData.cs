using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace DynamicGameAssets.PackData
{
    public class TailoringRecipePackData : BasePackData
    {
        public List<string> FirstItemTags { get; set; } = new(new[] { "item_cloth" });
        public List<string> SecondItemTags { get; set; }

        [DefaultValue(true)]
        public bool ConsumeSecondItem { get; set; } = true;

        [JsonConverter(typeof(ItemAbstractionWeightedListConverter))]
        public List<Weighted<ItemAbstraction>> CraftedItem { get; set; }
    }
}

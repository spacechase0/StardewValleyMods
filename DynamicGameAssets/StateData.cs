using System.Collections.Generic;

namespace DynamicGameAssets
{
    internal class StateData
    {
        public Dictionary<string, List<ShopEntry>> TodaysShopEntries { get; set; } = new Dictionary<string, List<ShopEntry>>();

        public int AnimationFrames { get; set; } = 0;
    }
}

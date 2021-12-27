using System.Collections.Generic;

namespace DynamicGameAssets.Framework
{
    internal class StateData
    {
        public Dictionary<string, List<ShopEntry>> TodaysShopEntries { get; set; } = new();

        public int AnimationFrames { get; set; } = 0;
    }
}

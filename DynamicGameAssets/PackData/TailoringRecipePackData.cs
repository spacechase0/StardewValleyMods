using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpaceShared;

namespace DynamicGameAssets.PackData
{
    public class TailoringRecipePackData : BasePackData
    {
        public List<string> FirstItemTags { get; set; } = new List<string>( new string[] { "item_cloth" } );
        public List<string> SecondItemTags { get; set; }

        public bool ConsumeSecondItem { get; set; } = true;

        [JsonConverter( typeof( ItemAbstractionWeightedListConverter ) )]
        public List<Weighted<ItemAbstraction>> CraftedItem { get; set; }
    }
}

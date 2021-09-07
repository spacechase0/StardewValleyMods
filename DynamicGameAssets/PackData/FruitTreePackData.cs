using System.Collections.Generic;
using System.ComponentModel;
using DynamicGameAssets.Game;
using Newtonsoft.Json;
using SpaceShared;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    public class FruitTreePackData : CommonPackData
    {
        public string Texture { get; set; }

        [DefaultValue(false)]
        public bool CanGrowNow { get; set; } = false; // must be controlled using dynamic fields

        [JsonConverter(typeof(ItemAbstractionWeightedListConverter))]
        public List<Weighted<ItemAbstraction>> Product { get; set; }

        public override void OnDisabled()
        {
            SpaceUtility.iterateAllTerrainFeatures((tf) =>
           {
               if (tf is CustomFruitTree cftree)
               {
                   if (cftree.SourcePack == this.pack.smapiPack.Manifest.UniqueID && cftree.Id == this.ID)
                       return null;
               }
               return tf;
           });
        }

        public override Item ToItem()
        {
            return null;
        }

        public override TexturedRect GetTexture()
        {
            return this.pack.GetTexture(this.Texture, 432, 80);
        }

        public override object Clone()
        {
            var ret = (FruitTreePackData)base.Clone();
            ret.Product = new List<Weighted<ItemAbstraction>>();
            foreach (var product in this.Product)
                ret.Product.Add((Weighted<ItemAbstraction>)product.Clone());
            return ret;
        }
    }
}

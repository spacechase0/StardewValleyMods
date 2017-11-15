using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace JsonAssets.Data
{
    public class FruitTreeData : DataNeedsId
    {
        [JsonIgnore]
        internal string directory;
        
        public object Product { get; set; }
        public string SaplingName { get; set; }
        public string SaplingDescription { get; set; }

        public string Season { get; set; }

        public IList<string> SaomgPurchaseRequirements { get; set; } = new List<string>();
        public int SaplingPurchasePrice { get; set; }
        public string SsaplingPurchaseFrom { get; set; } = "Pierre";

        internal ObjectData sapling;
        public int GetSaplingId() { return sapling.id; }
        public int GetFruitTreeIndex() { return id; }
        internal string GetFruitTreeInformation()
        {
            return $"{GetFruitTreeIndex()}/{Season}/{Mod.instance.ResolveObjectId(Product)}/what goes here?";
        }
    }
}

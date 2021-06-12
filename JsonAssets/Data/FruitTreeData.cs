using System.Collections.Generic;

namespace JsonAssets.Data
{
    public class FruitTreeData : DataNeedsIdWithTexture
    {
        public object Product { get; set; }
        public string SaplingName { get; set; }
        public string SaplingDescription { get; set; }

        public string Season { get; set; }

        public IList<string> SaplingPurchaseRequirements { get; set; } = new List<string>();
        public int SaplingPurchasePrice { get; set; }
        public string SaplingPurchaseFrom { get; set; } = "Pierre";
        public IList<PurchaseData> SaplingAdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public Dictionary<string, string> SaplingNameLocalization = new();
        public Dictionary<string, string> SaplingDescriptionLocalization = new();

        internal ObjectData Sapling;
        public int GetSaplingId() { return this.Sapling.Id; }
        public int GetFruitTreeIndex() { return this.Id; }
        internal string GetFruitTreeInformation()
        {
            return $"{this.GetFruitTreeIndex()}/{this.Season}/{Mod.instance.ResolveObjectId(this.Product)}/what goes here?";
        }
    }
}

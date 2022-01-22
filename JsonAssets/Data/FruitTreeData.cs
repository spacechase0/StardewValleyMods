using System.Collections.Generic;
using System.Runtime.Serialization;
using JsonAssets.Framework;

namespace JsonAssets.Data
{
    public class FruitTreeData : DataNeedsIdWithTexture
    {
        /*********
        ** Accessors
        *********/
        public object Product { get; set; }
        public string SaplingName { get; set; }
        public string SaplingDescription { get; set; }

        public string Season { get; set; }

        public IList<string> SaplingPurchaseRequirements { get; set; } = new List<string>();
        public int SaplingPurchasePrice { get; set; }
        public string SaplingPurchaseFrom { get; set; } = "Pierre";
        public IList<PurchaseData> SaplingAdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public Dictionary<string, string> SaplingNameLocalization { get; set; } = new();
        public Dictionary<string, string> SaplingDescriptionLocalization { get; set; } = new();
        public string SaplingTranslationKey { get; set; }

        internal ObjectData Sapling { get; set; }


        /*********
        ** Public methods
        *********/
        public string GetSaplingId()
        {
            return this.Sapling.Name;
        }

        internal string GetFruitTreeInformation()
        {
            return $"0/{this.Season}/{this.Product}/what goes here?/0/JA\\FruitTree\\{this.Name}";
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.SaplingPurchaseRequirements ??= new List<string>();
            this.SaplingAdditionalPurchaseData ??= new List<PurchaseData>();
            this.SaplingNameLocalization ??= new();
            this.SaplingDescriptionLocalization ??= new();

            this.SaplingPurchaseRequirements.FilterNulls();
            this.SaplingAdditionalPurchaseData.FilterNulls();
        }
    }
}

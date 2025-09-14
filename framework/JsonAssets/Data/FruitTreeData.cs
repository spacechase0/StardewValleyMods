using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JsonAssets.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace JsonAssets.Data
{
    public class FruitTreeData : DataNeedsIdWithTexture
    {
        /*********
        ** Accessors
        *********/
        public object Product { get; set; }
        public string SaplingName { get; set; }
        public string SaplingDescription
    {
            get => descript;
            set => descript = value ?? " ";
        }
        private string descript = " ";

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

        internal StardewValley.GameData.FruitTrees.FruitTreeData GetFruitTreeInformation()
        {
            StardewValley.GameData.FruitTrees.FruitTreeData ftree = new()
            {
                DisplayName = this.Name,
                Seasons = this.GetSeasons(),
                Fruit = new(new[]
                        {
                            new StardewValley.GameData.FruitTrees.FruitTreeFruitData()
                            {
                                ItemId = "(O)" + this.Product.ToString().FixIdJA("O"),
                            }
                        }),
                Texture = "JA/FruitTree/" + this.Name.FixIdJA("FruitTree"),
                TextureSpriteRow = 0,

            };
            return ftree;
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

        private List<Season> GetSeasons()
        {
            string[] seasonNames = this.Season.Split(",");
            List<Season> seasonList = new();
            foreach (string s in seasonNames)
            {
                seasonList.Add(Enum.Parse<Season>(s.Trim().Substring(0, 1).ToUpper() + s.Trim().Substring(1)));
            }
            return seasonList;
        }
    }
}

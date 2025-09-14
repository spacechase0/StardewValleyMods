using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using JsonAssets.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;
using StardewValley;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public class CropData : DataNeedsIdWithTexture
    {
        /*********
        ** Accessors
        *********/
        [JsonIgnore]
        public Texture2D GiantTexture { get; set; }

        public object Product { get; set; }
        public string SeedName { get; set; }
        public string SeedDescription
        {
            get => descript;
            set => descript = value ?? " ";
        }
        private string descript = " ";

        public CropType CropType { get; set; } = CropType.Normal;

        public IList<string> Seasons { get; set; } = new List<string>();
        public IList<int> Phases { get; set; } = new List<int>();
        public int RegrowthPhase { get; set; } = -1;
        public bool HarvestWithScythe { get; set; } = false;
        public bool TrellisCrop { get; set; } = false;
        public IList<Color> Colors { get; set; } = new List<Color>();
        public CropBonus Bonus { get; set; } = null;

        public IList<string> SeedPurchaseRequirements { get; set; } = new List<string>();
        public int SeedPurchasePrice { get; set; }
        public int SeedSellPrice { get; set; } = -1;
        public string SeedPurchaseFrom { get; set; } = "Pierre";
        public IList<PurchaseData> SeedAdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public Dictionary<string, string> SeedNameLocalization { get; set; } = new();
        public Dictionary<string, string> SeedDescriptionLocalization { get; set; } = new();
        public string SeedTranslationKey { get; set; }

        internal ObjectData Seed { get; set; }


        /*********
        ** Public methods
        *********/
        public string GetSeedId()
        {
            return this.Seed.Name;
        }

        internal StardewValley.GameData.Crops.CropData GetCropInformation()
        {
            List<Season> seasons = new();
            foreach (string season in this.Seasons)
                seasons.Add(Enum.Parse<Season>(season.Substring(0, 1).ToUpper() + season.Substring(1)));

            List<string> colors = new();
            foreach (var c in this.Colors)
            {
                colors.Add("#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2"));
            }

            var cropData = new StardewValley.GameData.Crops.CropData()
            {
                Seasons = seasons,
                DaysInPhase = new(this.Phases),
                RegrowDays = this.RegrowthPhase,
                IsRaised = this.TrellisCrop,
                IsPaddyCrop = this.CropType == CropType.Paddy,
                HarvestItemId = this.getFixedProductName(),
                HarvestMinStack = this.Bonus?.MinimumPerHarvest ?? 1,
                HarvestMaxStack = this.Bonus?.MaximumPerHarvest ?? 1,
                HarvestMaxIncreasePerFarmingLevel = this.Bonus?.MaxIncreasePerFarmLevel ?? 0,
                ExtraHarvestChance = this.Bonus?.ExtraChance ?? 0,
                HarvestMethod = this.HarvestWithScythe ? StardewValley.GameData.Crops.HarvestMethod.Scythe : StardewValley.GameData.Crops.HarvestMethod.Grab,
                TintColors = colors,
                Texture = "JA/Crop/" + this.Name.FixIdJA("Crop"),
            };
            return cropData;
        }

        internal StardewValley.GameData.GiantCrops.GiantCropData GetGiantCropInformation()
        {
            var giantData = new StardewValley.GameData.GiantCrops.GiantCropData()
            {
                FromItemId = this.getFixedProductName(),
                HarvestItems = new List<StardewValley.GameData.GiantCrops.GiantCropHarvestItemData> {
                    new StardewValley.GameData.GiantCrops.GiantCropHarvestItemData () {
                        Chance = 1.0f,
                        ScaledMinStackWhenShaving = 2,
                        ScaledMaxStackWhenShaving = 2,
                        Id = this.getFixedProductName(),
                        ItemId = this.getFixedProductName(),
                        MinStack = 15,
                        MaxStack = 21,
                        QualityModifierMode = StardewValley.GameData.QuantityModifier.QuantityModifierMode.Stack,
                    }
                },
                Texture = "JA/CropGiant/" + this.Name.FixIdJA("Crop")
            };
            return giantData;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.Seasons ??= new List<string>();
            this.Phases ??= new List<int>();
            this.Colors ??= new List<Color>();
            this.SeedPurchaseRequirements ??= new List<string>();
            this.SeedAdditionalPurchaseData ??= new List<PurchaseData>();
            this.SeedNameLocalization ??= new();
            this.SeedDescriptionLocalization ??= new();

            this.Seasons.FilterNulls();
            this.SeedPurchaseRequirements.FilterNulls();
            this.SeedAdditionalPurchaseData.FilterNulls();
        }

        private string getFixedProductName()
        {
            if (this.Product == null)
            {
                Log.Warn($"Null crop product detected");
                return "";
            }
            else if (int.TryParse(this.Product.ToString(), out int productId) && StardewValley.ItemRegistry.Exists("(O)" + productId.ToString()))
            {
                return "(O)" + productId.ToString();
            }
            else if (this.Product.ToString().FixIdJA("O") != null)
            {
                return "(O)" + this.Product.ToString().FixIdJA("O");
            }
            else
            {
                Log.Warn($"Invalid crop product detected: {this.Product.ToString()}");
                return this.Product.ToString();
            }
        }
    }
}

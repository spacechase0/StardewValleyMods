using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public class CropData : DataNeedsIdWithTexture
    {
        [JsonIgnore]
        public Texture2D giantTex;

        public object Product { get; set; }
        public string SeedName { get; set; }
        public string SeedDescription { get; set; }

        public enum CropType_
        {
            Normal,
            IndoorsOnly,
            Paddy,
        }
        public CropType_ CropType { get; set; } = CropType_.Normal;

        public IList<string> Seasons { get; set; } = new List<string>();
        public IList<int> Phases { get; set; } = new List<int>();
        public int RegrowthPhase { get; set; } = -1;
        public bool HarvestWithScythe { get; set; } = false;
        public bool TrellisCrop { get; set; } = false;
        public IList<Color> Colors { get; set; } = new List<Color>();
        public class Bonus_
        {
            public int MinimumPerHarvest { get; set; }
            public int MaximumPerHarvest { get; set; }
            public int MaxIncreasePerFarmLevel { get; set; }
            public double ExtraChance { get; set; }
        }
        public Bonus_ Bonus { get; set; } = null;

        public IList<string> SeedPurchaseRequirements { get; set; } = new List<string>();
        public int SeedPurchasePrice { get; set; }
        public int SeedSellPrice { get; set; } = -1;
        public string SeedPurchaseFrom { get; set; } = "Pierre";
        public IList<PurchaseData> SeedAdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public Dictionary<string, string> SeedNameLocalization = new();
        public Dictionary<string, string> SeedDescriptionLocalization = new();

        internal ObjectData seed;
        public int GetSeedId() { return this.seed.Id; }
        public int GetCropSpriteIndex() { return this.Id; }
        internal string GetCropInformation()
        {
            string str = "";
            //str += GetProductId() + "/";
            foreach (int phase in this.Phases)
            {
                str += phase + " ";
            }
            str = str.Substring(0, str.Length - 1) + "/";
            foreach (string season in this.Seasons)
            {
                str += season + " ";
            }
            str = str.Substring(0, str.Length - 1) + "/";
            str += $"{this.GetCropSpriteIndex()}/{Mod.instance.ResolveObjectId(this.Product)}/{this.RegrowthPhase}/";
            str += (this.HarvestWithScythe ? "1" : "0") + "/";
            if (this.Bonus != null)
                str += $"true {this.Bonus.MinimumPerHarvest} {this.Bonus.MaximumPerHarvest} {this.Bonus.MaxIncreasePerFarmLevel} {this.Bonus.ExtraChance}/";
            else str += "false/";
            str += (this.TrellisCrop ? "true" : "false") + "/";
            if (this.Colors != null && this.Colors.Count > 0)
            {
                str += "true";
                foreach (var color in this.Colors)
                    str += $" {color.R} {color.G} {color.B}";
            }
            else
                str += "false";
            return str;
        }
    }
}

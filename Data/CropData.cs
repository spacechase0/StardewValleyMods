using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace JsonAssets.Data
{
    public class CropData : DataNeedsId
    {
        [JsonIgnore]
        internal Texture2D texture;
        
        public object Product { get; set; }
        public string SeedName { get; set; }
        public string SeedDescription { get; set; }

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
        public string SeedPurchaseFrom { get; set; } = "Pierre";
        
        public Dictionary<string, string> SeedNameLocalization = new Dictionary<string, string>();
        public Dictionary<string, string> SeedDescriptionLocalization = new Dictionary<string, string>();

        internal ObjectData seed;
        public int GetSeedId() { return seed.id; }
        public int GetCropSpriteIndex() { return id; }
        internal string GetCropInformation()
        {
            string str = "";
            //str += GetProductId() + "/";
            foreach ( var phase in Phases )
            {
                str += phase + " ";
            }
            str = str.Substring(0, str.Length - 1) + "/";
            foreach (var season in Seasons)
            {
                str += season + " ";
            }
            str = str.Substring(0, str.Length - 1) + "/";
            str += $"{GetCropSpriteIndex()}/{Mod.instance.ResolveObjectId(Product)}/{RegrowthPhase}/";
            str += (HarvestWithScythe ? "1" : "0") + "/";
            if (Bonus != null)
                str += $"true {Bonus.MinimumPerHarvest} {Bonus.MaximumPerHarvest} {Bonus.MaxIncreasePerFarmLevel} {Bonus.ExtraChance}/";
            else str += "false/";
            str += (TrellisCrop ? "true" : "false") + "/";
            if (Colors != null && Colors.Count > 0)
            {
                str += "true";
                foreach (var color in Colors)
                    str += $" {color.R} {color.G} {color.B}";
            }
            else
                str += "false";
            return str;
        }
    }
}

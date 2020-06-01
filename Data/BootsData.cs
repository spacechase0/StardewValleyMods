using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StardewValley;
using System;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace JsonAssets.Data
{
    public class BootsData : DataSeparateTextureIndex
    {
        [JsonIgnore]
        public Texture2D texture;

        [JsonIgnore]
        public Texture2D textureColor;
        
        public string Description { get; set; }

        public int Price { get; set; }
        
        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Marlon";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();
        public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public Dictionary<string, string> NameLocalization = new Dictionary<string, string>();
        public Dictionary<string, string> DescriptionLocalization = new Dictionary<string, string>();
        
        public int Defense { get; set; }
        public int Immunity { get; set; }

        public string LocalizedName()
        {
            var currLang = LocalizedContentManager.CurrentLanguageCode;
            if (currLang == LocalizedContentManager.LanguageCode.en)
                return Name;
            if (NameLocalization == null || !NameLocalization.ContainsKey(currLang.ToString()))
                return Name;
            return NameLocalization[currLang.ToString()];
        }

        public string LocalizedDescription()
        {
            var currLang = LocalizedContentManager.CurrentLanguageCode;
            if (currLang == LocalizedContentManager.LanguageCode.en)
                return Description;
            if (DescriptionLocalization == null || !DescriptionLocalization.ContainsKey(currLang.ToString()))
                return Description;
            return DescriptionLocalization[currLang.ToString()];
        }

        public int GetObjectId() { return id; }
        public int GetTextureIndex() { return textureIndex; }
        
        internal string GetPurchaseRequirementString()
        {
            if ( PurchaseRequirements == null )
                return "";
            var str = $"1234567890";
            foreach (var cond in PurchaseRequirements)
                str += $"/{cond}";
            return str;
        }

        internal string GetBootsInformation()
        {
            return $"{Name}/{LocalizedDescription()}/{Price}/{Defense}/{Immunity}/{textureIndex}/{LocalizedName()}";
        }
    }
}

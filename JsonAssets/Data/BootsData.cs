using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;

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
            /*if (currLang == LocalizedContentManager.LanguageCode.en)
                return Name;*/
            if (this.NameLocalization == null || !this.NameLocalization.ContainsKey(currLang.ToString()))
                return this.Name;
            return this.NameLocalization[currLang.ToString()];
        }

        public string LocalizedDescription()
        {
            var currLang = LocalizedContentManager.CurrentLanguageCode;
            /*if (currLang == LocalizedContentManager.LanguageCode.en)
                return Description;*/
            if (this.DescriptionLocalization == null || !this.DescriptionLocalization.ContainsKey(currLang.ToString()))
                return this.Description;
            return this.DescriptionLocalization[currLang.ToString()];
        }

        public int GetObjectId() { return this.id; }
        public int GetTextureIndex() { return this.textureIndex; }

        internal string GetBootsInformation()
        {
            return $"{this.Name}/{this.LocalizedDescription()}/{this.Price}/{this.Defense}/{this.Immunity}/{this.textureIndex}/{this.LocalizedName()}";
        }
    }
}

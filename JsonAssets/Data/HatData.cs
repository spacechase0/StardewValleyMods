using System.Collections.Generic;
using StardewValley;

namespace JsonAssets.Data
{
    public class HatData : DataNeedsIdWithTexture
    {
        public string Description { get; set; }
        public int PurchasePrice { get; set; }
        public bool ShowHair { get; set; }
        public bool IgnoreHairstyleOffset { get; set; }

        public bool CanPurchase { get; set; } = true;

        public string Metadata { get; set; } = "";

        public Dictionary<string, string> NameLocalization = new Dictionary<string, string>();
        public Dictionary<string, string> DescriptionLocalization = new Dictionary<string, string>();

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

        public int GetHatId() { return this.id; }

        internal string GetHatInformation()
        {
            return $"{this.Name}/{this.LocalizedDescription()}/" + (this.ShowHair ? "true" : "false") + "/" + (this.IgnoreHairstyleOffset ? "true" : "false") + $"/{this.Metadata}/{this.LocalizedName()}";
        }
    }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;

namespace JsonAssets.Data
{
    public class ClothingData : DataSeparateTextureIndex
    {
        [JsonIgnore]
        public Texture2D textureMale;
        [JsonIgnore]
        public Texture2D textureFemale;

        public string Description { get; set; }
        public bool HasFemaleVariant { get; set; } = false;

        public int Price { get; set; }

        public Color DefaultColor { get; set; } = new(255, 235, 203);
        public bool Dyeable { get; set; } = false;

        public string Metadata { get; set; } = "";

        public Dictionary<string, string> NameLocalization = new();
        public Dictionary<string, string> DescriptionLocalization = new();

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

        public int GetClothingId() { return this.id; }
        public int GetMaleIndex() { return this.textureIndex; }
        public int GetFemaleIndex() { return this.HasFemaleVariant ? (this.textureIndex + 1) : -1; }

        internal string GetClothingInformation()
        {
            return $"{this.Name}/{this.LocalizedName()}/{this.LocalizedDescription()}/{this.GetMaleIndex()}/{this.GetFemaleIndex()}/{this.Price}/{this.DefaultColor.R} {this.DefaultColor.G} {this.DefaultColor.B}/{this.Dyeable}/Shirt/{this.Metadata}";
        }
    }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;

namespace JsonAssets.Data
{
    public class PantsData : DataSeparateTextureIndex
    {
        [JsonIgnore]
        public Texture2D texture;

        public string Description { get; set; }

        public int Price { get; set; }

        public Color DefaultColor { get; set; } = new Color(255, 235, 203);
        public bool Dyeable { get; set; } = false;

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

        public int GetClothingId() { return this.id; }
        public int GetTextureIndex() { return this.textureIndex; }

        internal string GetClothingInformation()
        {
            return $"{this.Name}/{this.LocalizedName()}/{this.LocalizedDescription()}/{this.GetTextureIndex()}/-1/{this.Price}/{this.DefaultColor.R} {this.DefaultColor.G} {this.DefaultColor.B}/{this.Dyeable}/Pants/{this.Metadata}";
        }
    }
}

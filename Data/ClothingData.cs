using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Color DefaultColor { get; set; } = new Color(255, 235, 203);
        public bool Dyeable { get; set; } = false;

        public string Metadata { get; set; } = "";

        public Dictionary<string, string> NameLocalization = new Dictionary<string, string>();
        public Dictionary<string, string> DescriptionLocalization = new Dictionary<string, string>();

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

        public int GetClothingId() { return id; }
        public int GetMaleIndex() { return textureIndex; }
        public int GetFemaleIndex() { return HasFemaleVariant ? (textureIndex + 1): -1; }

        internal string GetClothingInformation()
        {
            return $"{Name}/{LocalizedName()}/{LocalizedDescription()}/{GetMaleIndex()}/{GetFemaleIndex()}/{Price}/{DefaultColor.R} {DefaultColor.G} {DefaultColor.B}/{Dyeable}/Shirt/{Metadata}";
        }
    }
}

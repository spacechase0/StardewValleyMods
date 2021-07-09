using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;
using StardewValley;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public class PantsData : DataSeparateTextureIndex
    {
        /*********
        ** Accessors
        *********/
        [JsonIgnore]
        public Texture2D Texture { get; set; }

        public string Description { get; set; }

        public int Price { get; set; }

        public Color DefaultColor { get; set; } = new(255, 235, 203);
        public bool Dyeable { get; set; } = false;

        public string Metadata { get; set; } = "";

        public Dictionary<string, string> NameLocalization { get; set; } = new();
        public Dictionary<string, string> DescriptionLocalization { get; set; } = new();


        /*********
        ** Public methods
        *********/
        public string LocalizedName()
        {
            var lang = LocalizedContentManager.CurrentLanguageCode;
            return this.NameLocalization != null && this.NameLocalization.TryGetValue(lang.ToString(), out string localization)
                ? localization
                : this.Name;
        }

        public string LocalizedDescription()
        {
            var lang = LocalizedContentManager.CurrentLanguageCode;
            return this.DescriptionLocalization != null && this.DescriptionLocalization.TryGetValue(lang.ToString(), out string localization)
                ? localization
                : this.Description;
        }

        public int GetClothingId()
        {
            return this.Id;
        }

        public int GetTextureIndex()
        {
            return this.TextureIndex;
        }

        internal string GetClothingInformation()
        {
            return $"{this.Name}/{this.LocalizedName()}/{this.LocalizedDescription()}/{this.GetTextureIndex()}/-1/{this.Price}/{this.DefaultColor.R} {this.DefaultColor.G} {this.DefaultColor.B}/{this.Dyeable}/Pants/{this.Metadata}";
        }
    }
}

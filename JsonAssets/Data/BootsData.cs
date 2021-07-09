using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;
using StardewValley;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public class BootsData : DataSeparateTextureIndex
    {
        /*********
        ** Accessors
        *********/
        [JsonIgnore]
        public Texture2D Texture { get; set; }

        [JsonIgnore]
        public Texture2D TextureColor { get; set; }

        public string Description { get; set; }

        public int Price { get; set; }

        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Marlon";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();
        public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public Dictionary<string, string> NameLocalization { get; set; } = new();
        public Dictionary<string, string> DescriptionLocalization { get; set; } = new();

        public int Defense { get; set; }
        public int Immunity { get; set; }


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

        public int GetObjectId()
        {
            return this.Id;
        }

        public int GetTextureIndex()
        {
            return this.TextureIndex;
        }

        internal string GetBootsInformation()
        {
            return $"{this.Name}/{this.LocalizedDescription()}/{this.Price}/{this.Defense}/{this.Immunity}/{this.TextureIndex}/{this.LocalizedName()}";
        }
    }
}

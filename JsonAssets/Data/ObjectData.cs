using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;
using StardewValley;
using SObject = StardewValley.Object;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public class ObjectData : DataNeedsIdWithTexture
    {
        [JsonIgnore]
        public Texture2D textureColor;

        public string Description { get; set; }
        public ObjectCategory Category { get; set; }
        public string CategoryTextOverride { get; set; }
        public Color CategoryColorOverride { get; set; } = new(0, 0, 0, 0);
        public bool IsColored { get; set; } = false;

        public int Price { get; set; }

        public bool CanTrash { get; set; } = true;
        public bool CanSell { get; set; } = true;
        public bool CanBeGifted { get; set; } = true;

        public bool HideFromShippingCollection { get; set; } = false;

        public ObjectRecipe Recipe { get; set; }

        public int Edibility { get; set; } = SObject.inedible;
        public bool EdibleIsDrink { get; set; } = false;
        public ObjectFoodBuffs EdibleBuffs = new();

        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Pierre";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();
        public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public ObjectGiftTastes GiftTastes;

        public Dictionary<string, string> NameLocalization = new();
        public Dictionary<string, string> DescriptionLocalization = new();

        public List<string> ContextTags = new();

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

        public int GetObjectId() { return this.Id; }

        internal string GetObjectInformation()
        {
            if (this.Edibility != SObject.inedible)
            {
                int itype = (int)this.Category;
                string str = $"{this.Name}/{this.Price}/{this.Edibility}/" + (this.Category == ObjectCategory.Artifact ? "Arch" : $"{this.Category} {itype}") + $"/{this.LocalizedName()}/{this.LocalizedDescription()}/";
                str += (this.EdibleIsDrink ? "drink" : "food") + "/";
                this.EdibleBuffs ??= new ObjectFoodBuffs();
                str += $"{this.EdibleBuffs.Farming} {this.EdibleBuffs.Fishing} {this.EdibleBuffs.Mining} 0 {this.EdibleBuffs.Luck} {this.EdibleBuffs.Foraging} 0 {this.EdibleBuffs.MaxStamina} {this.EdibleBuffs.MagnetRadius} {this.EdibleBuffs.Speed} {this.EdibleBuffs.Defense} {this.EdibleBuffs.Attack}/{this.EdibleBuffs.Duration}";
                return str;
            }
            else
            {
                int itype = (int)this.Category;
                return $"{this.Name}/{this.Price}/{this.Edibility}/" + (this.Category == ObjectCategory.Artifact ? "Arch" : $"Basic {itype}") + $"/{this.LocalizedName()}/{this.LocalizedDescription()}";
            }
        }
    }
}

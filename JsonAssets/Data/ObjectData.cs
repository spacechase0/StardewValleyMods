using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using JsonAssets.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;
using SObject = StardewValley.Object;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    [DebuggerDisplay("name = {Name}, id = {Id}")]
    public class ObjectData : DataNeedsIdWithTexture, ITranslatableItem
    {
        /*********
        ** Accessors
        *********/
        [JsonIgnore]
        public Texture2D TextureColor { get; set; }

        /// <inheritdoc />
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
        public ObjectFoodBuffs EdibleBuffs { get; set; } = new();

        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Pierre";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();
        public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public ObjectGiftTastes GiftTastes { get; set; } = new();

        /// <inheritdoc />
        public Dictionary<string, string> NameLocalization { get; set; } = new();

        /// <inheritdoc />
        public Dictionary<string, string> DescriptionLocalization { get; set; } = new();

        /// <inheritdoc />
        public string TranslationKey { get; set; }

        public List<string> ContextTags { get; set; } = new();


        /*********
        ** Public methods
        *********/
        public int GetObjectId()
        {
            return this.Id;
        }

        internal string GetObjectInformation()
        {
            if (this.Edibility != SObject.inedible)
            {
                int itype = (int)this.Category;
                string str = $"{this.Name}/{this.Price}/{this.Edibility}/" + (this.Category == ObjectCategory.Artifact ? "Arch" : $"{this.Category} {itype}") + $"/{this.LocalizedName()}/{this.LocalizedDescription()}/";
                str += (this.EdibleIsDrink ? "drink" : "food") + "/";
                str += $"{this.EdibleBuffs.Farming} {this.EdibleBuffs.Fishing} {this.EdibleBuffs.Mining} 0 {this.EdibleBuffs.Luck} {this.EdibleBuffs.Foraging} 0 {this.EdibleBuffs.MaxStamina} {this.EdibleBuffs.MagnetRadius} {this.EdibleBuffs.Speed} {this.EdibleBuffs.Defense} {this.EdibleBuffs.Attack}/{this.EdibleBuffs.Duration}";
                return str;
            }
            else
            {
                int itype = (int)this.Category;
                return $"{this.Name}/{this.Price}/{this.Edibility}/" + (this.Category == ObjectCategory.Artifact ? "Arch" : $"Basic {itype}") + $"/{this.LocalizedName()}/{this.LocalizedDescription()}";
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.EdibleBuffs ??= new();
            this.PurchaseRequirements ??= new List<string>();
            this.AdditionalPurchaseData ??= new List<PurchaseData>();
            this.GiftTastes ??= new();
            this.NameLocalization ??= new();
            this.DescriptionLocalization ??= new();
            this.ContextTags ??= new();

            this.PurchaseRequirements.FilterNulls();
            this.AdditionalPurchaseData.FilterNulls();
            this.ContextTags.FilterNulls();
        }
    }
}

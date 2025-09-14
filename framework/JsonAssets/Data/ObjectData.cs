using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using JsonAssets.Framework;
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
    public class ObjectData : DataNeedsIdWithTexture, ITranslatableItem
    {
        /*********
        ** Accessors
        *********/
        [JsonIgnore]
        public Texture2D TextureColor { get; set; }

        /// <inheritdoc />
        public string Description
        {
            get => descript;
            set => descript = value ?? " ";
        }
        private string descript = " ";

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
        internal StardewValley.GameData.Objects.ObjectData GetObjectInformation()
        {
            var ctx = ContextTags.ToList();
            if ( !CanBeGifted )
                ctx.Add("not_giftable");
            var ret = new StardewValley.GameData.Objects.ObjectData()
            {
                Name = this.Name,
                DisplayName = this.LocalizedName(),
                Description = this.LocalizedDescription(),
                Type = Category == ObjectCategory.Artifact ? "Arch" : (Category == ObjectCategory.Ring ? "Ring" : "Basic"),
                Category = (int)this.Category,
                Price = Price,
                Texture = $"JA\\Object\\{Name.FixIdJA("O")}",
                SpriteIndex = 0,
                Edibility = Edibility,
                IsDrink = EdibleIsDrink,
                Buffs = [ new()
                {
                    CustomAttributes = new()
                    {
                        FarmingLevel = EdibleBuffs.Farming,
                        FishingLevel = EdibleBuffs.Fishing,
                        MiningLevel = EdibleBuffs.Mining,
                        LuckLevel = EdibleBuffs.Luck,
                        ForagingLevel = EdibleBuffs.Foraging,
                        MaxStamina = EdibleBuffs.MaxStamina,
                        MagneticRadius = EdibleBuffs.MagnetRadius,
                        Speed = EdibleBuffs.Speed,
                        Defense = EdibleBuffs.Defense,
                        Attack = EdibleBuffs.Attack,
                    },
                    Duration = EdibleBuffs.Duration,
                } ],
                ContextTags = ctx,
                ExcludeFromShippingCollection = HideFromShippingCollection,
            };

            return ret;
        }

        public Texture2D GetTexture()
        {
            if (!this.IsColored)
            {
                return this.Texture;
            }
            else
            {
                Texture2D tex = new Texture2D(Game1.graphics.GraphicsDevice, 32, 16);
                Color[] frame = new Color[16 * 16];

                // Put in the base texture
                this.Texture.GetData(0, new Rectangle(0, 0, 16, 16), frame, 0, 16 * 16);
                tex.SetData(0, 0, new Rectangle(0, 0, 16, 16), frame, 0, 16 * 16);

                // Put in the colored texture
                this.TextureColor.GetData(0, new Rectangle(0, 0, 16, 16), frame, 0, 16 * 16);
                tex.SetData(0, 0, new Rectangle(16, 0, 16, 16), frame, 0, 16 * 16);
                return tex;
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using JsonAssets.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;
using StardewValley;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public class ShirtData : DataSeparateTextureIndex, ITranslatableItem
    {
        /*********
        ** Accessors
        *********/
        [JsonIgnore]
        public Texture2D Texture { get; set; } = null;

        [JsonIgnore]
        public Texture2D TextureMale { get; set; }

        [JsonIgnore]
        public Texture2D TextureFemale { get; set; }

        [JsonIgnore]
        public Texture2D TextureMaleColor { get; set; }

        [JsonIgnore]
        public Texture2D TextureFemaleColor { get; set; }

        /// <inheritdoc />
        public string Description
        {
            get => descript;
            set => descript = value ?? " ";
        }
        private string descript = " ";

        public bool HasFemaleVariant { get; set; } = false;

        public int Price { get; set; }

        public Color DefaultColor { get; set; } = new(255, 235, 203);
        public bool Dyeable { get; set; } = false;
        public bool HasSleeves { get; set; } = true;

        public string Metadata { get; set; } = "";

        /// <inheritdoc />
        public Dictionary<string, string> NameLocalization { get; set; } = new();

        /// <inheritdoc />
        public Dictionary<string, string> DescriptionLocalization { get; set; } = new();

        /// <inheritdoc />
        public string TranslationKey { get; set; }

        /*********
        ** Public methods
        *********/
        public int GetMaleIndex()
        {
            return this.TextureIndex;
        }

        public int GetFemaleIndex()
        {
            return this.HasFemaleVariant
                ? this.TextureIndex + 1
                : -1;
        }

        public Texture2D GetShirtTexture()
        {
            if (this.Texture == null)
            {
                Texture2D newTex = new(Game1.graphics.GraphicsDevice, 256, 32);
                newTex.Name = this.Name.FixIdJA("S");
                Color[] data = new Color[8 * 32];
                TextureMale.GetData(0, new Rectangle(0, 0, 8, 32), data, 0, 8 * 32);
                newTex.SetData(0, 0, new Rectangle(0, 0, 8, 32), data, 0, 8 * 32);
                if (this.HasFemaleVariant)
                {
                    TextureFemale.GetData(0, new Rectangle(0, 0, 8, 32), data, 0, 8 * 32);
                    newTex.SetData(0, 0, new Rectangle(8, 0, 8, 32), data, 0, 8 * 32);
                }
                if (this.Dyeable)
                {
                    TextureMaleColor.GetData(0, new Rectangle(0, 0, 8, 32), data, 0, 8 * 32);
                    newTex.SetData(0, 0, new Rectangle(128, 0, 8, 32), data, 0, 8 * 32);
                    if (this.HasFemaleVariant)
                    {
                        TextureFemaleColor.GetData(0, new Rectangle(0, 0, 8, 32), data, 0, 8 * 32);
                        newTex.SetData(0, 0, new Rectangle(128 + 8, 0, 8, 32), data, 0, 8 * 32);
                    }
                }
                this.Texture = newTex;
                return newTex;
            }
            else
            {
                return this.Texture;
            }
        }

        internal StardewValley.GameData.Shirts.ShirtData GetShirtInformation()
        {
            var ret = new StardewValley.GameData.Shirts.ShirtData()
            {
                Name = this.HasFemaleVariant ? this.Name + "_M" : this.Name,
                DisplayName = this.HasFemaleVariant ? this.LocalizedName() + " (M)": this.LocalizedName(),
                Description = this.LocalizedDescription(),
                Price = this.Price,
                Texture = $"JA\\Shirts\\{Name.FixIdJA("S")}",
                SpriteIndex = 0,
                DefaultColor = this.DefaultColor.R + " " + this.DefaultColor.G + " " + this.DefaultColor.B,
                CanBeDyed = this.Dyeable,
                IsPrismatic = false,
                HasSleeves = this.HasSleeves,
                CanChooseDuringCharacterCustomization = false
            };
            return ret;
        }

        internal StardewValley.GameData.Shirts.ShirtData GetFemaleShirtInformation()
        {
            var ret = new StardewValley.GameData.Shirts.ShirtData()
            {
                Name = this.Name + "_F",
                DisplayName = this.HasFemaleVariant ? this.LocalizedName() + " (F)" : this.LocalizedName(),
                Description = this.LocalizedDescription(),
                Price = this.Price,
                Texture = $"JA\\Shirts\\{Name.FixIdJA("S")}",
                SpriteIndex = 1,
                DefaultColor = this.DefaultColor.R + " " + this.DefaultColor.G + " " + this.DefaultColor.B,
                CanBeDyed = this.Dyeable,
                IsPrismatic = false,
                HasSleeves = this.HasSleeves,
                CanChooseDuringCharacterCustomization = false
            };
            return ret;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.NameLocalization ??= new();
            this.DescriptionLocalization ??= new();
        }
    }
}

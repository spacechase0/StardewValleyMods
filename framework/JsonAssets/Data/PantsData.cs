using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using JsonAssets.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public class PantsData : DataSeparateTextureIndex, ITranslatableItem
    {
        /*********
        ** Accessors
        *********/
        [JsonIgnore]
        public Texture2D Texture { get; set; }

        /// <inheritdoc />
        public string Description
        {
            get => descript;
            set => descript = value ?? " ";
        }
        private string descript = " ";

        public int Price { get; set; }

        public Color DefaultColor { get; set; } = new(255, 235, 203);
        public bool Dyeable { get; set; } = false;

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
        public int GetTextureIndex()
        {
            return this.TextureIndex;
        }

        internal StardewValley.GameData.Pants.PantsData GetPantsInformation()
        {
            var ret = new StardewValley.GameData.Pants.PantsData()
            {
                Name = this.Name,
                DisplayName = this.LocalizedName(),
                Description = this.LocalizedDescription(),
                Price = this.Price,
                Texture = $"JA\\Pants\\{Name.FixIdJA("P")}",
                SpriteIndex = 0,
                DefaultColor = this.DefaultColor.R + " " + this.DefaultColor.G + " " + this.DefaultColor.B,
                CanBeDyed = this.Dyeable,
                IsPrismatic = false,
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

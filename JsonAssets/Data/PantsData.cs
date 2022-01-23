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
        public string Description { get; set; }

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

        internal string GetClothingInformation()
        {
            return $"{this.Name}/{this.LocalizedName()}/{this.LocalizedDescription()}/0/-1/{this.Price}/{this.DefaultColor.R} {this.DefaultColor.G} {this.DefaultColor.B}/{this.Dyeable}/Pants/{this.Metadata}/JA\\Pants\\{this.Name}";
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

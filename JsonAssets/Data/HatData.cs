using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using JsonAssets.Framework;
using SpaceShared;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public class HatData : DataNeedsIdWithTexture, ITranslatableItem
    {
        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public string Description { get; set; }
        public int PurchasePrice { get; set; }
        public bool ShowHair { get; set; }
        public bool IgnoreHairstyleOffset { get; set; }

        public bool CanPurchase { get; set; } = true;

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
        public int GetHatId()
        {
            return this.Id;
        }

        internal string GetHatInformation()
        {
            return $"{this.Name}/{this.LocalizedDescription()}/" + (this.ShowHair ? "true" : "false") + "/" + (this.IgnoreHairstyleOffset ? "true" : "false") + $"/{this.Metadata}/{this.LocalizedName()}";
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

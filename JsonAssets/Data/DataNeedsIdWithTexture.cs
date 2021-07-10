using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public abstract class DataNeedsIdWithTexture : DataNeedsId
    {
        /*********
        ** Accessors
        *********/
        [JsonIgnore]
        public Texture2D Texture
        {
#pragma warning disable 618 // deliberate wrapper for obsolete code
            get => this.texture;
            set => this.texture = value;
#pragma warning restore 618
        }

        [JsonIgnore]
        [Obsolete("Use " + nameof(Texture) + " instead.")]
        public Texture2D texture;

        // The following is mainly data for the Content Patcher integration.

        [JsonIgnore]
        public string Tilesheet { get; set; }

        [JsonIgnore]
        public int TilesheetX { get; set; }

        [JsonIgnore]
        public int TilesheetY { get; set; }
    }
}

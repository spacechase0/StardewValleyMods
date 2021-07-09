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
        public Texture2D Texture { get; set; }

        // The following is mainly data for the Content Patcher integration.

        [JsonIgnore]
        public string Tilesheet { get; set; }

        [JsonIgnore]
        public int TilesheetX { get; set; }

        [JsonIgnore]
        public int TilesheetY { get; set; }
    }
}

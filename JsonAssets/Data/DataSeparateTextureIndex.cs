using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using SpaceShared;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public abstract class DataSeparateTextureIndex : DataNeedsId
    {
        /*********
        ** Accessors
        *********/
        [JsonIgnore]
        internal int TextureIndex { get; set; } = -1;

        // The following is mainly data for the Content Patcher integration.

        [JsonIgnore]
        public string Tilesheet { get; set; }

        [JsonIgnore]
        public int TilesheetX { get; set; }

        [JsonIgnore]
        public int TilesheetY { get; set; }
    }
}

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
        [JsonIgnore]
        public Texture2D texture;

        // The following is mainly data for the Content Patcher integration.

        [JsonIgnore]
        public string tilesheet;

        [JsonIgnore]
        public int tilesheetX;

        [JsonIgnore]
        public int tilesheetY;
    }
}

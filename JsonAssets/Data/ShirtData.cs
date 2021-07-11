using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public class ShirtData : ClothingData
    {
        /*********
        ** Accessors
        *********/
        [JsonIgnore]
        public Texture2D TextureMaleColor { get; set; }

        [JsonIgnore]
        public Texture2D TextureFemaleColor { get; set; }
    }
}

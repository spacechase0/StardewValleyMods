using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace JsonAssets.Data
{
    public class ShirtData : ClothingData
    {
        [JsonIgnore]
        public Texture2D textureMaleColor;
        [JsonIgnore]
        public Texture2D textureFemaleColor;
    }
}

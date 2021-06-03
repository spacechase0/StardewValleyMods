using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace JsonAssets.Data
{
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

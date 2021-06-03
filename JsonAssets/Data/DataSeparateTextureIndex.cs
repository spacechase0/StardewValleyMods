using Newtonsoft.Json;

namespace JsonAssets.Data
{
    public abstract class DataSeparateTextureIndex : DataNeedsId
    {
        [JsonIgnore]
        internal int textureIndex = -1;

        // The following is mainly data for the Content Patcher integration.

        [JsonIgnore]
        public string tilesheet;

        [JsonIgnore]
        public int tilesheetX;

        [JsonIgnore]
        public int tilesheetY;
    }
}

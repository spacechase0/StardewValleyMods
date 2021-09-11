using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace BiggerCraftables.Framework
{
    internal class Entry
    {
        [JsonIgnore]
        public Texture2D Texture { get; set; }

        public string Name { get; set; }
        public string Image { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
    }
}

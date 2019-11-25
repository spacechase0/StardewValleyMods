using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Data
{
    class ShirtData : ClothingData
    {
        [JsonIgnore]
        internal Texture2D textureMaleColor;
        [JsonIgnore]
        internal Texture2D textureFemaleColor;
    }
}

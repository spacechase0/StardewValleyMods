using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Data
{
    class HatData : DataNeedsId
    {
        [JsonIgnore]
        internal Texture2D texture;

        public string Description { get; set; }
        public int PurchasePrice { get; set; }
        public bool ShowHair { get; set; }
        public bool IgnoreHairstyleOffset { get; set; }

        public int GetHatId() { return id; }

        internal string GetHatInformation()
        {
            return $"{Name}/{Description}/" + ( ShowHair ? "true" : "false" ) + "/" + (IgnoreHairstyleOffset ? "true" : "false");
        }
    }
}

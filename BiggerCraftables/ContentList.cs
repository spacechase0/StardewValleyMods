using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiggerCraftables
{
    public class ContentList
    {
        public class Entry
        {
            [JsonIgnore]
            public Texture2D Texture { get; set; }

            public string Name { get; set; }
            public string Image { get; set; }
            public int Width { get; set; }
            public int Length { get; set; }
        }

        public List<Entry> BiggerCraftables { get; set; } = new List<Entry>();
    }
}

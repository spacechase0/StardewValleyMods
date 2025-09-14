using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoonMisadventures.Game.Items
{
    public class NecklaceData
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Texture { get; set; }
        public int TextureIndex { get; set; }
        public bool CanBeSelectedAtAltar { get; set; } = true;
    }
}

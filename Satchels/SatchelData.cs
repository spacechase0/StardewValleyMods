using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satchels
{
    public class SatchelData
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Texture { get; set; } = "spacechase0.Satchels/satchels.png";
        public int TextureIndex { get; set; } = 0;

        public int Capacity { get; set; }
        public int MaxUpgrades { get; set; }
    }
}

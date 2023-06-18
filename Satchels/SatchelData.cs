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
        public string BaseTexture { get; set; } = "spacechase0.Satchels/satchels.png";
        public int BaseTextureIndex { get; set; } = 0;
        public string InlayTexture { get; set; } = "spacechase0.Satchels/satchels.png";
        public int InlayTextureIndex { get; set; }

        public int Capacity { get; set; }
        public int MaxUpgrades { get; set; }
    }
}

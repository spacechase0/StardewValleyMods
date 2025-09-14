using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Utilities;

namespace MageDelve
{
    public class Configuration
    {
        public KeybindList MercenaryInteractModifier { get; set; } = new(StardewModdingAPI.SButton.LeftShift);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Stardew3D
{
    public class Configuration
    {
        public int MultisampleCount { get; set; } = 0;

        public KeybindList RotateLeft { get; set; } = new KeybindList(SButton.Q);
        public KeybindList RotateRight { get; set; } = new KeybindList(SButton.R);
    }
}

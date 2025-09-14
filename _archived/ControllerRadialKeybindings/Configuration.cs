using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Utilities;

namespace ControllerRadialKeybindings
{
    public class Configuration
    {
        public class RadialConfig
        {
            public class Keybinding
            {
                public string ModId { get; set; }
                public string KeybindOption { get; set; }
                public int PressDuration { get; set; } = 1;
            }

            public KeybindList Trigger { get; set; }
            public Keybinding[] Keybindings { get; set; } = new Keybinding[8];
        }

        public RadialConfig A { get; set; } = new();
        public RadialConfig B { get; set; } = new();
    }
}

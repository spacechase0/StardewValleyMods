using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace TerraformingHoe
{
    public class Configuration
    {
        public KeybindList HoeModeKey { get; set; } = new KeybindList(new Keybind(SButton.H));
    }
}

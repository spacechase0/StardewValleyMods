using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace StardewVR
{
    public class Configuration
    {
        public KeybindList ToggleVirtualReality { get; set; } = new(SButton.End);
    }
}

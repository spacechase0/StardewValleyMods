using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AdvancedSocialInteractions
{
    internal class Configuration
    {
        public bool AlwaysTrigger { get; set; } = false;
        public KeybindList TriggerModifier { get; set; } = new KeybindList(SButton.LeftShift);
    }
}

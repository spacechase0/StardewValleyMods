using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CombatOverhaulMod
{
    public class Configuration
    {
        public KeybindList Jump { get; set; } = new KeybindList(SButton.Space);
    }
}

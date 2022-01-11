using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace MisappliedPhysicalities
{
    public class Configuration
    {
        public KeybindList PlacementModifier { get; set; } = new KeybindList( SButton.LeftControl );
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.Tools;

namespace MoonMisadventures.Game.Locations
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_LunarFarmCave" )]
    public class LunarFarmCave : LunarLocation
    {
        public LunarFarmCave()
        {
        }

        public LunarFarmCave( IModContentHelper content )
        : base( content, "MoonFarmCave", "MoonFarmCave" )
        {
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();
            Game1.background = null;
        }
    }
}

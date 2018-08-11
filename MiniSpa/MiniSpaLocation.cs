using SpaceCore.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile;
using MoreBuildings.SpookyShed;
using StardewValley.Monsters;
using Microsoft.Xna.Framework.Graphics;
using SObject = StardewValley.Object;
using PyTK.CustomElementHandler;

namespace MoreBuildings.MiniSpa
{
    public class MiniSpaLocation : GameLocation
    {
        public MiniSpaLocation()
        :   base( "Maps\\MiniSpa", "MiniSpa")
        {
        }

        protected override void resetLocalState()
        {
            Game1.player.changeIntoSwimsuit();
            Game1.player.swimming.Value = true;
        }

        public override int getExtraMillisecondsPerInGameMinuteForThisLocation()
        {
            return 7000;
        }
    }
}

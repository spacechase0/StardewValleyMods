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

namespace MoreBuildings.BigShed
{
    public class BigShedLocation : CustomDecoratableLocation
    {
        public BigShedLocation()
        :   base( "Maps\\Shed2", "Shed2" )
        {
        }

        public override List<Rectangle> getFloors()
        {
            return new List<Rectangle> { new Rectangle(1, 3, 21, 20) };
        }

        public override List<Rectangle> getWalls()
        {
            return new List<Rectangle>{ new Rectangle(1, 1, 21, 3) };
        }
    }
}

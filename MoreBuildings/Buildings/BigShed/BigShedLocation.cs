using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley.Locations;

namespace MoreBuildings.Buildings.BigShed
{
    [XmlType("Mods_spacechase0_BigShedLocation")]
    public class BigShedLocation : DecoratableLocation
    {
        public BigShedLocation()
            : base("Maps\\Shed2_", "Shed2") { }

        public override List<Rectangle> getFloors()
        {
            return new() { new Rectangle(1, 3, 21, 20) };
        }

        public override List<Rectangle> getWalls()
        {
            return new() { new Rectangle(1, 1, 21, 3) };
        }
    }
}

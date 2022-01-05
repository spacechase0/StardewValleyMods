using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace MoreBuildings.Buildings.BigShed
{
    [XmlType("Mods_spacechase0_BigShedBuilding")]
    public class BigShedBuilding : Building
    {
        private static readonly BluePrint Blueprint = new("Shed2");

        public BigShedBuilding()
            : base(BigShedBuilding.Blueprint, Vector2.Zero) { }

        protected override GameLocation getIndoors(string nameOfIndoorsWithoutUnique)
        {
            return new BigShedLocation();
        }
    }
}

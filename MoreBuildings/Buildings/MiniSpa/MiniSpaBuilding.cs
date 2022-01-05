using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace MoreBuildings.Buildings.MiniSpa
{
    [XmlType("Mods_spacechase0_MiniSpaBuilding")]
    public class MiniSpaBuilding : Building
    {
        private static readonly BluePrint Blueprint = new("MiniSpa");

        public MiniSpaBuilding()
            : base(MiniSpaBuilding.Blueprint, Vector2.Zero) { }

        protected override GameLocation getIndoors(string nameOfIndoorsWithoutUnique)
        {
            return new MiniSpaLocation();
        }
    }
}

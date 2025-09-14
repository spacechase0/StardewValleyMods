using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace MoreBuildings.Buildings.SpookyShed
{
    [XmlType("Mods_spacechase0_SpookyShedBuilding")]
    public class SpookyShedBuilding : Building
    {
        private static readonly BluePrint Blueprint = new("SpookyShed");

        public SpookyShedBuilding()
            : base(SpookyShedBuilding.Blueprint, Vector2.Zero) { }

        protected override GameLocation getIndoors(string nameOfIndoorsWithoutUnique)
        {
            return new SpookyShedLocation();
        }
    }
}

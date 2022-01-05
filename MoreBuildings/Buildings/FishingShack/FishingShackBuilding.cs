using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace MoreBuildings.Buildings.FishingShack
{
    [XmlType("Mods_spacechase0_FishingShackBuilding")]
    public class FishingShackBuilding : Building
    {
        private static readonly BluePrint Blueprint = new("FishShack");

        public FishingShackBuilding()
            : base(FishingShackBuilding.Blueprint, Vector2.Zero) { }

        public FishingShackBuilding(BluePrint blueprint, Vector2 tileLocation)
            : base(blueprint, tileLocation) { }

        protected override GameLocation getIndoors(string nameOfIndoorsWithoutUnique)
        {
            return new FishingShackLocation();
        }
    }
}

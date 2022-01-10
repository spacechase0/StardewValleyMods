using System.Xml.Serialization;
using StardewValley;

namespace MoreBuildings.Buildings.MiniSpa
{
    [XmlType("Mods_spacechase0_MiniSpaLocation")]
    public class MiniSpaLocation : GameLocation
    {
        public MiniSpaLocation()
            : base("Maps\\MiniSpa", "MiniSpa") { }

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

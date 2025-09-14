using System.Xml.Serialization;
using StardewModdingAPI;
using StardewValley;

namespace MoonMisadventures.Game.Locations
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_MoonInfuserRoom" )]
    public class MoonInfuserRoom : LunarLocation
    {
        public MoonInfuserRoom() { }
        public MoonInfuserRoom( IModContentHelper content )
        :   base( content, "MoonInfuserRoom", "MoonInfuserRoom")
        {
        }

        protected override void resetLocalState()
        {
            base.resetLocalState();
            Game1.background = null;
        }

        public override bool performAction( string action, Farmer who, xTile.Dimensions.Location tileLocation )
        {
            if ( action == "InfuserSign1" )
                Game1.drawObjectDialogue(I18n.Message_Infuser_1());
            else if ( action == "InfuserSign2" )
                Game1.drawObjectDialogue(I18n.Message_Infuser_2());
            else if ( action == "InfuserSign3" )
                Game1.drawObjectDialogue(I18n.Message_Infuser_3());
            else if ( action == "CelestialInfuser" )
            {
                Game1.activeClickableMenu = new InfuserMenu();
            }
            return base.performAction( action, who, tileLocation );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using xTile;

namespace MisappliedPhysicalities.Game.Locations
{
    [XmlType( "Mods_spacechase0_MisappliedPhysicalities_LunarLocation" )]
    public class LunarLocation : GameLocation
    {
        public LunarLocation() { }
        public LunarLocation( IContentHelper content, string mapPath, string mapName )
        :   base( content.GetActualAssetKey( "assets/" + mapPath + ".tmx" ), mapName )
        {
        }
        protected override void resetLocalState()
        {
            base.resetLocalState();

            Game1.background = new SpaceBackground();
        }

        public override string checkForBuriedItem( int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who )
        {
            Random r = new Random( xLocation * 3000 + yLocation + ( int ) Game1.uniqueIDForThisGame + ( int ) Game1.stats.DaysPlayed + Name.GetDeterministicHashCode() );
            if ( r.NextDouble() < 0.03 )
            {
                Game1.createObjectDebris( 424 /* cheese */, xLocation, yLocation, this );
            }
            return base.checkForBuriedItem( xLocation, yLocation, explosion, detectOnly, who );
        }
    }
}

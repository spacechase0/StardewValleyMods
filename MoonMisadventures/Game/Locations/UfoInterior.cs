using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using xTile;

namespace MoonMisadventures.Game.Locations
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_UfoInterior" )]
    public class UfoInterior : GameLocation
    {
        public UfoInterior() { }
        public UfoInterior( IContentHelper content )
        : base( content.GetActualAssetKey( "assets/maps/UfoInterior.tmx" ), "Custom_MM_UfoInterior" )
        {
        }

        // todo
    }
}

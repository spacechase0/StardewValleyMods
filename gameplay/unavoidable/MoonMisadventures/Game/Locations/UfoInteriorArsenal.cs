using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using xTile;
using xTile.Dimensions;

namespace MoonMisadventures.Game.Locations
{
    [XmlType("Mods_spacechase0_MoonMisadventures_UfoInteriorArsenal")]
    public class UfoInteriorArsenal : GameLocation
    {
        public UfoInteriorArsenal() { }
        public UfoInteriorArsenal( IModContentHelper content )
        : base( content.GetInternalAssetName( "assets/maps/UfoInteriorArsenal.tmx" ).BaseName, "Custom_MM_UfoInteriorArsenal" )
        {
        }

        public override bool performAction(string[] action, Farmer who, Location tileLocation)
        {
            return base.performAction(action, who, tileLocation);
        }
    }
}

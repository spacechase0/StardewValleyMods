using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using xTile.Dimensions;

namespace DeepSeaFishing
{
    [XmlType( "Mods_spacechase0_DeepSeaFishing_DeepSea" )]
    public class DeepSeaLocation : GameLocation
    {
        public DeepSeaLocation()
        :   base( Mod.instance.Helper.ModContent.GetInternalAssetName( "assets/DeepSea.tmx" ).BaseName, "Custom_DeepSeaFishing_DeepSea" )
        {
        }

        public override StardewValley.Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
        {
            return null;
        }

        public override bool performAction(string action, Farmer who, Location tileLocation)
        {
            if (action == "SeaShop")
            {
                Game1.activeClickableMenu = new ShopMenu(MakeShopStock());
                return true;
            }
            else if (action == "Boat")
            {
                Game1.warpFarmer("Beach", 38, 32, false);
                return true;
            }

            return base.performAction(action, who, tileLocation);
        }

        private Dictionary<ISalable, int[]> MakeShopStock()
        {
            Dictionary<ISalable, int[]> ret = new();

            // ...

            return ret;
        }
    }
}

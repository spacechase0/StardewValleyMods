using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SleepyEye
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            MenuEvents.MenuChanged += onMenuChange;
        }

        private void onMenuChange( object sender, EventArgsClickableMenuChanged args )
        {
            var menu = args.NewMenu as ShopMenu;
            if (menu == null || menu.portraitPerson == null)
                return;

            if ( menu.portraitPerson.Name == "Pierre" )
            {
                Log.debug("Adding tent to shop");

                var forSale = Helper.Reflection.GetField<List<Item>>(menu, "forSale").GetValue();
                var itemPriceAndStock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(menu, "itemPriceAndStock").GetValue();

                var item = new TentTool();
                forSale.Add(item);
                itemPriceAndStock.Add(item, new int[] { item.salePrice(), item.Stack });
            } 
        }
    }
}

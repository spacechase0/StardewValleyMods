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
            base.Entry(helper);
            instance = this;

            MenuEvents.MenuChanged += onMenuChange;

            Entoarox.Framework.EntoFramework.GetTypeRegistry().RegisterType<TentTool>();
        }

        private void onMenuChange( object sender, EventArgsClickableMenuChanged args )
        {
            var menu = args.NewMenu as ShopMenu;
            if (menu == null || menu.portraitPerson == null)
                return;

            if ( menu.portraitPerson.name == "Pierre" )
            {
                Log.debug("Adding tent to shop");

                var forSale = Helper.Reflection.GetPrivateValue<List<Item>>(menu, "forSale");
                var itemPriceAndStock = Helper.Reflection.GetPrivateValue<Dictionary<Item, int[]>>(menu, "itemPriceAndStock");

                var item = new TentTool();
                forSale.Add(item);
                itemPriceAndStock.Add(item, new int[] { item.salePrice(), item.Stack });
            } 
        }
    }
}

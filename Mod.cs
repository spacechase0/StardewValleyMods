using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using StardewModdingAPI.Events;

namespace SleepyEye
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            helper.Events.Display.MenuChanged += onMenuChanged;
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onMenuChanged( object sender, MenuChangedEventArgs e )
        {
            if (!(e.NewMenu is ShopMenu menu) || menu.portraitPerson.Name != "Pierre")
                return;

            Log.debug("Adding tent to shop");

            var forSale = Helper.Reflection.GetField<List<Item>>(menu, "forSale").GetValue();
            var itemPriceAndStock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(menu, "itemPriceAndStock").GetValue();

            var item = new TentTool();
            forSale.Add(item);
            itemPriceAndStock.Add(item, new int[] { item.salePrice(), item.Stack });
        }
    }
}

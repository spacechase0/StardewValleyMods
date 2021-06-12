using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;

namespace SleepyEye
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.Display.MenuChanged += this.OnMenuChanged;
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!(e.NewMenu is ShopMenu menu) || menu.portraitPerson.Name != "Pierre")
                return;

            Log.Debug("Adding tent to shop");

            var forSale = menu.forSale;
            var itemPriceAndStock = menu.itemPriceAndStock;

            var item = new TentTool();
            forSale.Add(item);
            itemPriceAndStock.Add(item, new[] { item.salePrice(), item.Stack });
        }
    }
}

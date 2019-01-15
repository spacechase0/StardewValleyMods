using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Menus;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace MoreGrassStarters
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;

            helper.Events.Display.MenuChanged += onMenuChanged;

            if ( File.Exists(Path.Combine(Helper.DirectoryPath, "grass.png")) )
            {
                GrassStarterItem.tex2 = Mod.instance.Helper.Content.Load<Texture2D>("grass.png");
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!(e.NewMenu is ShopMenu menu) || menu.portraitPerson == null)
                return;

            if (menu.portraitPerson.Name == "Pierre")
            {
                var forSale = Helper.Reflection.GetField<List<Item>>(menu, "forSale").GetValue();
                var itemPriceAndStock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(menu, "itemPriceAndStock").GetValue();

                for (int i = Grass.caveGrass; i < 5 + GrassStarterItem.ExtraGrassTypes; ++i)
                {
                    var item = new GrassStarterItem(i);
                    forSale.Add(item);
                    itemPriceAndStock.Add(item, new int[] { 100, int.MaxValue });
                }
            }
        }
    }
}

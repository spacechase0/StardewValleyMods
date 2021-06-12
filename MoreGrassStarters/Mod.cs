using System.IO;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;

namespace MoreGrassStarters
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.Display.MenuChanged += this.OnMenuChanged;

            if (File.Exists(Path.Combine(this.Helper.DirectoryPath, "assets", "grass.png")))
            {
                GrassStarterItem.Tex2 = Mod.Instance.Helper.Content.Load<Texture2D>("assets/grass.png");
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!(e.NewMenu is ShopMenu menu) || menu.portraitPerson == null)
                return;

            if (menu.portraitPerson.Name == "Pierre")
            {
                var forSale = menu.forSale;
                var itemPriceAndStock = menu.itemPriceAndStock;

                for (int i = Grass.caveGrass; i < 5 + GrassStarterItem.ExtraGrassTypes; ++i)
                {
                    var item = new GrassStarterItem(i);
                    forSale.Add(item);
                    itemPriceAndStock.Add(item, new[] { 100, int.MaxValue });
                }
            }
        }
    }
}

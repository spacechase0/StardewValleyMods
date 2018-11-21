using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public override void Entry(IModHelper helper)
        {
            instance = this;
            MenuEvents.MenuChanged += menuChanged;

            if ( File.Exists(Path.Combine(Helper.DirectoryPath, "grass.png")) )
            {
                GrassStarterItem.tex2 = Mod.instance.Helper.Content.Load<Texture2D>("grass.png");
            }
        }
        
        private void menuChanged(object sender, EventArgsClickableMenuChanged args)
        {
            var menu = args.NewMenu as ShopMenu;
            if (menu == null || menu.portraitPerson == null)
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

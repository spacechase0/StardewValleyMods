using SpaceShared;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGameAssets
{
    internal class ShopEntry
    {
        public ISalable Item;
        public int Quantity;
        public int Price;
        public int? CurrencyId;

        public void AddToShop( ShopMenu shop )
        {
            int qty = Quantity;
            if (Item is StardewValley.Object obj && obj.IsRecipe)
                qty = 1;

            Item.Stack = qty;
            shop.forSale.Add( Item );
            if ( CurrencyId == null )
            {
                shop.itemPriceAndStock.Add( Item, new int[]
                {
                    CurrencyId == null ? Price : 0,
                    qty
                } );
            }
            else
            {
                shop.itemPriceAndStock.Add( Item, new int[]
                {
                    0,
                    qty,
                    CurrencyId.Value, // Black magic
                    Price,
                } );
            }
        }

        public void AddToShopStock( Dictionary<ISalable, int[]> stock )
        {
            int qty = Quantity;
            if (Item is StardewValley.Object obj && obj.IsRecipe)
                qty = 1;

            Item.Stack = qty;
            if ( CurrencyId == null )
            {
                stock.Add( Item, new int[]
                {
                    CurrencyId == null ? Price : 0,
                    qty
                } );
            }
            else
            {
                stock.Add( Item, new int[]
                {
                    0,
                    qty,
                    CurrencyId.Value, // Black magic
                    Price,
                } );
            }
        }
    }
}
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets
{
    internal class ShopEntry
    {
        public ISalable Item;
        public int Quantity;
        public int Price;
        public string Currency;

        public void AddToShop( ShopMenu shop )
        {
            Item.Stack = Quantity;
            shop.forSale.Add( Item );
            if ( Currency == null )
            {
                shop.itemPriceAndStock.Add( Item, new int[]
                {
                    Currency == null ? Price : 0,
                    Quantity
                } );
            }
            else
            {
                shop.itemPriceAndStock.Add( Item, new int[]
                {
                    0,
                    Quantity,
                    Currency.GetHashCode(), // Black magic
                    Price,
                } );
            }
        }

        public void AddToShopStock( Dictionary<ISalable, int[]> stock )
        {
            Item.Stack = Quantity;
            if ( Currency == null )
            {
                stock.Add( Item, new int[]
                {
                    Currency == null ? Price : 0,
                    Quantity
                } );
            }
            else
            {
                stock.Add( Item, new int[]
                {
                    0,
                    Quantity,
                    Currency.GetHashCode(), // Black magic
                    Price,
                } );
            }
        }
    }
}

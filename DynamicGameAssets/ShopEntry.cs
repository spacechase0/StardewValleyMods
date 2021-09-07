using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;

namespace DynamicGameAssets
{
    internal class ShopEntry
    {
        public ISalable Item;
        public int Quantity;
        public int Price;
        public int? CurrencyId;

        public void AddToShop(ShopMenu shop)
        {
            int qty = this.Quantity;
            if (this.Item is StardewValley.Object obj && obj.IsRecipe)
                qty = 1;

            this.Item.Stack = qty;
            shop.forSale.Add(this.Item);
            if (this.CurrencyId == null)
            {
                shop.itemPriceAndStock.Add(this.Item, new int[]
                {
                    this.CurrencyId == null ? this.Price : 0,
                    qty
                });
            }
            else
            {
                shop.itemPriceAndStock.Add(this.Item, new int[]
                {
                    0,
                    qty,
                    this.CurrencyId.Value, // Black magic
                    this.Price,
                });
            }
        }

        public void AddToShopStock(Dictionary<ISalable, int[]> stock)
        {
            int qty = this.Quantity;
            if (this.Item is StardewValley.Object obj && obj.IsRecipe)
                qty = 1;

            this.Item.Stack = qty;
            if (this.CurrencyId == null)
            {
                stock.Add(this.Item, new int[]
                {
                    this.CurrencyId == null ? this.Price : 0,
                    qty
                });
            }
            else
            {
                stock.Add(this.Item, new int[]
                {
                    0,
                    qty,
                    this.CurrencyId.Value, // Black magic
                    this.Price,
                });
            }
        }
    }
}

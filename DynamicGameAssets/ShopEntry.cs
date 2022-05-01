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
            // Check to see if this shopEntry has already been added to this shop--if so, don't add
            if (shop.itemPriceAndStock.ContainsKey(this.Item))
                return;

            int qty = this.Quantity;
            if (this.Item is StardewValley.Object { IsRecipe: true })
                qty = 1;

            this.Item.Stack = qty;
            shop.forSale.Add(this.Item);
            if (this.CurrencyId == null)
            {
                shop.itemPriceAndStock.Add(this.Item, new[]
                {
                    this.CurrencyId == null ? this.Price : 0,
                    qty
                });
            }
            // Seeds and saplings need price modified
            else if (this.Item is StardewValley.Object obj && (obj.Category == StardewValley.Object.SeedsCategory || obj.isSapling()))
            {
                shop.itemPriceAndStock.Add(this.Item, new[]
                {
                    0,
                    qty,
                    this.CurrencyId.Value, // Black magic copied
                    (int)((float)this.Price*Game1.MasterPlayer.difficultyModifier),
                });
            }
            else
            {
                shop.itemPriceAndStock.Add(this.Item, new[]
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
            if (this.Item is StardewValley.Object { IsRecipe: true })
                qty = 1;

            this.Item.Stack = qty;
            if (this.CurrencyId == null)
            {
                stock.Add(this.Item, new[]
                {
                    this.CurrencyId == null ? this.Price : 0,
                    qty
                });
            }
            // Seeds and saplings need price modified
            else if (this.Item is StardewValley.Object obj && (obj.Category == StardewValley.Object.SeedsCategory || obj.isSapling()))
            {
                stock.Add(this.Item, new[]
                {
                    0,
                    qty,
                    this.CurrencyId.Value, // Black magic copied
                    (int)((float)this.Price*Game1.MasterPlayer.difficultyModifier),
                });
            }
            else
            {
                stock.Add(this.Item, new[]
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

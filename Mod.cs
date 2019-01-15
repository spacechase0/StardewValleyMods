using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

namespace BetterShopMenu
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            helper.Events.Display.MenuChanged += onMenuChanged;
            helper.Events.GameLoop.UpdateTicked += onUpdateTicked;
            helper.Events.Display.RenderedActiveMenu += onRenderedActiveMenu;
            helper.Events.Input.ButtonPressed += onButtonPressed;
        }

        private ShopMenu shop;
        private bool firstTick = false;
        private List<Item> initialItems;
        private Dictionary<Item, int[]> initialStock;
        private List<int> categories;
        private int currCategory;
        bool hasRecipes;
        private Dictionary<int, string> categoryNames;
        private int sorting = 0;
        private void initShop( ShopMenu shopMenu )
        {
            shop = shopMenu;
            firstTick = true;
        }
        private void initShop2()
        {
            firstTick = false;

            initialItems = Helper.Reflection.GetField<List<Item>>(shop, "forSale").GetValue();
            initialStock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(shop, "itemPriceAndStock").GetValue();

            categories = new List<int>();
            hasRecipes = false;
            foreach ( var item in initialItems )
            {
                var obj = item as SObject;
                if (!categories.Contains(item.Category) && (obj == null || !obj.IsRecipe))
                    categories.Add(item.Category);
                if (obj != null && obj.IsRecipe)
                    hasRecipes = true;
            }
            currCategory = -1;

            categoryNames = new Dictionary<int, string>();
            categoryNames.Add(-1, "Everything");
            categoryNames.Add(0, "Other");
            categoryNames.Add(SObject.GreensCategory, "Greens");
            categoryNames.Add(SObject.GemCategory, "Gems");
            categoryNames.Add(SObject.VegetableCategory, "Vegetables");
            categoryNames.Add(SObject.FishCategory, "Fish");
            categoryNames.Add(SObject.EggCategory, "Egg");
            categoryNames.Add(SObject.MilkCategory, "Milk");
            categoryNames.Add(SObject.CookingCategory, "Cooking");
            categoryNames.Add(SObject.CraftingCategory, "Crafting");
            categoryNames.Add(SObject.BigCraftableCategory, "Big Craftables");
            categoryNames.Add(SObject.FruitsCategory, "Fruits");
            categoryNames.Add(SObject.SeedsCategory, "Seeds");
            categoryNames.Add(SObject.mineralsCategory, "Minerals");
            categoryNames.Add(SObject.flowersCategory, "Flowers");
            categoryNames.Add(SObject.meatCategory, "Meat");
            categoryNames.Add(SObject.metalResources, "Metals");
            categoryNames.Add(SObject.buildingResources, "Building Resources"); //?
            categoryNames.Add(SObject.sellAtPierres, "Sellable @ Pierres");
            categoryNames.Add(SObject.sellAtPierresAndMarnies, "Sellable @ Pierre's/Marnie's");
            categoryNames.Add(SObject.fertilizerCategory, "Fertilizer");
            categoryNames.Add(SObject.junkCategory, "Junk");
            categoryNames.Add(SObject.baitCategory, "Bait");
            categoryNames.Add(SObject.tackleCategory, "Tackle");
            categoryNames.Add(SObject.sellAtFishShopCategory, "Sellable @ Willy's");
            categoryNames.Add(SObject.furnitureCategory, "Furniture");
            categoryNames.Add(SObject.ingredientsCategory, "Ingredients");
            categoryNames.Add(SObject.artisanGoodsCategory, "Artisan Goods");
            categoryNames.Add(SObject.syrupCategory, "Syrups");
            categoryNames.Add(SObject.monsterLootCategory, "Monster Loot");
            categoryNames.Add(SObject.equipmentCategory, "Equipment");
            categoryNames.Add(SObject.hatCategory, "Hats");
            categoryNames.Add(SObject.ringCategory, "Rings");
            categoryNames.Add(SObject.weaponCategory, "Weapons");
            categoryNames.Add(SObject.bootsCategory, "Boots");
            categoryNames.Add(SObject.toolCategory, "Tools");
            categoryNames.Add(categories.Count, "Recipes");

            syncStock();
        }
        private void changeCategory(int amt)
        {
            currCategory += amt;

            if ( currCategory == -2 )
                currCategory = hasRecipes ? categories.Count : ( categories.Count - 1 );
            if (currCategory == categories.Count && !hasRecipes || currCategory > categories.Count)
                currCategory = -1;

            syncStock();
        }
        private void changeSorting(int amt)
        {
            sorting += amt;
            if (sorting > 2)
                sorting = 0;
            else if (sorting < 0)
                sorting = 2;

            syncStock();
        }
        private void syncStock()
        {
            var items = new List<Item>();
            var stock = new Dictionary<Item, int[]>();
            foreach (var item in initialItems)
            {
                if (itemMatchesCategory(item, currCategory))
                {
                    items.Add(item);
                }
            }
            foreach (var item in initialStock)
            {
                if (itemMatchesCategory(item.Key, currCategory))
                {
                    stock.Add(item.Key, item.Value);
                }
            }

            Helper.Reflection.GetField<List<Item>>(shop, "forSale").SetValue(items);
            Helper.Reflection.GetField<Dictionary<Item, int[]>>(shop, "itemPriceAndStock").SetValue(stock);

            doSorting();
        }
        private void doSorting()
        {
            var items = Helper.Reflection.GetField<List<Item>>(shop, "forSale").GetValue();
            var stock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(shop, "itemPriceAndStock").GetValue();
            if ( sorting != 0 )
            {
                if (sorting == 1)
                    items.Sort((a, b) => stock[a][0] - stock[b][0]);
                else if (sorting == 2)
                    items.Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if ( shop != null && firstTick )
                initShop2();
        }

        /// <summary>When a menu is open (<see cref="Game1.activeClickableMenu"/> isn't null), raised after that menu is drawn to the sprite batch but before it's rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (shop == null)
                return;

            Vector2 pos = new Vector2( shop.xPositionOnScreen + 25, shop.yPositionOnScreen + 550 );
            IClickableMenu.drawTextureBox(Game1.spriteBatch, (int)pos.X, (int)pos.Y, 200, 72, Color.White);
            pos.X += 16;
            pos.Y += 16;
            string str = "Category: \n" + categoryNames[((currCategory == -1 || currCategory == categories.Count) ? currCategory : categories[currCategory])];
            Game1.spriteBatch.DrawString(Game1.dialogueFont, str, pos + new Vector2(-1,  1), new Color( 224, 150, 80 ), 0, Vector2.Zero, 0.5f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
            Game1.spriteBatch.DrawString(Game1.dialogueFont, str, pos, new Color( 86, 22, 12 ), 0, Vector2.Zero, 0.5f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);

            pos = new Vector2(shop.xPositionOnScreen + 25, shop.yPositionOnScreen + 630);
            IClickableMenu.drawTextureBox(Game1.spriteBatch, (int)pos.X, (int)pos.Y, 200, 48, Color.White);
            pos.X += 16;
            pos.Y += 16;
            str = "Sorting: " + (sorting == 0 ? "None" : (sorting == 1 ? "Price" : "Name"));
            Game1.spriteBatch.DrawString(Game1.dialogueFont, str, pos + new Vector2(-1, 1), new Color(224, 150, 80), 0, Vector2.Zero, 0.5f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
            Game1.spriteBatch.DrawString(Game1.dialogueFont, str, pos, new Color(86, 22, 12), 0, Vector2.Zero, 0.5f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
            
            shop.drawMouse(Game1.spriteBatch);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onButtonPressed(object sender, ButtonPressedEventArgs e )
        {
            if (shop == null)
                return;

            if (e.Button == SButton.MouseLeft || e.Button == SButton.MouseRight)
            {
                int x = (int) e.Cursor.ScreenPixels.X;
                int y = (int) e.Cursor.ScreenPixels.Y;
                int direction = e.Button == SButton.MouseLeft ? 1 : -1;

                if (new Rectangle(shop.xPositionOnScreen + 25, shop.yPositionOnScreen + 550, 200, 72).Contains(x, y))
                    changeCategory(direction);
                if (new Rectangle(shop.xPositionOnScreen + 25, shop.yPositionOnScreen + 630, 200, 48).Contains(x, y))
                    changeSorting(direction);
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if ( e.NewMenu is ShopMenu shopMenu )
            {
                Log.trace("Found shop menu!");
                initShop(shopMenu);
            }
            else
            {
                shop = null;
            }
        }

        private bool itemMatchesCategory( Item item, int cat )
        {
            var obj = item as SObject;
            if (cat == -1)
                return true;
            if (cat == categories.Count)
                return obj != null && obj.IsRecipe;
            if (categories[ cat ] == item.Category)
                return (obj == null || !obj.IsRecipe);
            return false;
        }
    }
}

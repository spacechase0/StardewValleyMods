using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using SObject = StardewValley.Object;

namespace BetterShopMenu
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static Configuration Config;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Config = helper.ReadConfig<Configuration>();

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
            
            if ( Config.GridLayout )
            {
                drawGridLayout();
            }

            shop.drawMouse(Game1.spriteBatch);
        }

        private void drawGridLayout()
        {
            var forSale = Helper.Reflection.GetField<List<Item>>(shop, "forSale").GetValue();
            var itemPriceAndStock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(shop, "itemPriceAndStock").GetValue();
            var currency = Helper.Reflection.GetField<int>(shop, "currency").GetValue();
            var animations = Helper.Reflection.GetField<List<TemporaryAnimatedSprite>>(shop, "animations").GetValue();
            var poof = Helper.Reflection.GetField<TemporaryAnimatedSprite>(shop, "poof").GetValue();
            var heldItem = Helper.Reflection.GetField<Item>(shop, "heldItem").GetValue();
            var currentItemIndex = Helper.Reflection.GetField<int>(shop, "currentItemIndex").GetValue();
            const int UNIT_WIDTH = 160;
            const int UNIT_HEIGHT = 144;
            int unitsWide = (shop.width - 32) / UNIT_WIDTH;
            Item hover = null;

            //IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), shop.xPositionOnScreen + shop.width - shop.inventory.width - 32 - 24, shop.yPositionOnScreen + shop.height - 256 + 40, shop.inventory.width + 56, shop.height - 448 + 20, Color.White, 4f, true);
            IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), shop.xPositionOnScreen, shop.yPositionOnScreen, shop.width, shop.height - 256 + 32 + 4, Color.White, 4f, true);
            for ( int i = currentItemIndex * unitsWide; i < forSale.Count && i < currentItemIndex * unitsWide + unitsWide * 3; ++i )
            {
                int ix = i % unitsWide;
                int iy = i / unitsWide;
                Rectangle rect = new Rectangle(shop.xPositionOnScreen + 16 + ix * UNIT_WIDTH, shop.yPositionOnScreen + 16 + iy * UNIT_HEIGHT - currentItemIndex * UNIT_HEIGHT, UNIT_WIDTH, UNIT_HEIGHT);
                IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), rect.X, rect.Y, rect.Width, rect.Height, rect.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White, 4f, false);
                forSale[i].drawInMenu(Game1.spriteBatch, new Vector2(rect.X + 48, rect.Y + 16), 1f);
                int price = itemPriceAndStock[forSale[i]][0];
                var priceStr = price.ToString();
                SpriteText.drawString(Game1.spriteBatch, priceStr, rect.Right - SpriteText.getWidthOfString(priceStr) - 16, rect.Y + 80, alpha: ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) >= price ? 1f : 0.5f);
                //Utility.drawWithShadow(Game1.spriteBatch, Game1.mouseCursors, new Vector2(rect.Right - 16, rect.Y + 80), new Rectangle(193 + currency * 9, 373, 9, 10), Color.White, 0, Vector2.Zero, 1, layerDepth: 1);
                if (rect.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                    hover = forSale[i];
            }
            if (forSale.Count == 0)
                SpriteText.drawString(Game1.spriteBatch, Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583"), shop.xPositionOnScreen + shop.width / 2 - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583"), 999999) / 2, shop.yPositionOnScreen + shop.height / 2 - 128, 999999, -1, 999999, 1f, 0.88f, false, -1, "", -1);
            //shop.inventory.draw(Game1.spriteBatch);
            // Moved currency here so above doesn't draw over it
            if (currency == 0)
                Game1.dayTimeMoneyBox.drawMoneyBox(Game1.spriteBatch, shop.xPositionOnScreen - 36, shop.yPositionOnScreen + shop.height - shop.inventory.height + 48);
            for (int index = animations.Count - 1; index >= 0; --index)
            {
                if (animations[index].update(Game1.currentGameTime))
                    animations.RemoveAt(index);
                else
                    animations[index].draw(Game1.spriteBatch, true, 0, 0, 1f);
            }
            if (poof != null)
                poof.draw(Game1.spriteBatch, false, 0, 0, 1f);
            // scrollbar override
            if ( hover != null )
            {
                string hoverText = hover.getDescription();
                string boldTitleText = hover.DisplayName;
                int hoverPrice = itemPriceAndStock == null || !itemPriceAndStock.ContainsKey(hover) ? hover.salePrice() : itemPriceAndStock[hover][0];
                int getHoveredItemExtraItemIndex = -1;
                if (itemPriceAndStock != null && hover != null && (itemPriceAndStock.ContainsKey(hover) && itemPriceAndStock[hover].Length > 2))
                    getHoveredItemExtraItemIndex = itemPriceAndStock[hover][2];
                int getHoveredItemExtraItemAmount = 5;
                IClickableMenu.drawToolTip(Game1.spriteBatch, hoverText, boldTitleText, hover, heldItem != null, -1, currency, getHoveredItemExtraItemIndex, getHoveredItemExtraItemAmount, (CraftingRecipe)null, hoverPrice);
            }
            if (heldItem != null)
                heldItem.drawInMenu(Game1.spriteBatch, new Vector2((float)(Game1.getOldMouseX() + 8), (float)(Game1.getOldMouseY() + 8)), 1f);
            // some other stuff I don't think matters?
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

                if (Config.GridLayout)
                {
                    Helper.Input.Suppress(e.Button);
                    doGridLayoutLeftClick(e);
                }
            }
        }

        private void doGridLayoutLeftClick(ButtonPressedEventArgs e)
        {
            var forSale = Helper.Reflection.GetField<List<Item>>(shop, "forSale").GetValue();
            var itemPriceAndStock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(shop, "itemPriceAndStock").GetValue();
            var currency = Helper.Reflection.GetField<int>(shop, "currency").GetValue();
            var animations = Helper.Reflection.GetField<List<TemporaryAnimatedSprite>>(shop, "animations").GetValue();
            var poof = Helper.Reflection.GetField<TemporaryAnimatedSprite>(shop, "poof").GetValue();
            var heldItem = Helper.Reflection.GetField<Item>(shop, "heldItem").GetValue();
            var currentItemIndex = Helper.Reflection.GetField<int>(shop, "currentItemIndex").GetValue();
            const int UNIT_WIDTH = 160;
            const int UNIT_HEIGHT = 144;
            int unitsWide = (shop.width - 32) / UNIT_WIDTH;
            var sellPercentage = Helper.Reflection.GetField<float>(shop, "sellPercentage").GetValue();

            int x = (int)e.Cursor.ScreenPixels.X;
            int y = (int)e.Cursor.ScreenPixels.Y;

            if (shop.upperRightCloseButton.containsPoint(x, y))
            {
                shop.exitThisMenu(true);
                return;
            }

            // Copying a lot from left click code
            Vector2 clickableComponent = shop.inventory.snapToClickableComponent(x, y);
            if (heldItem == null)
            {
                Item obj = shop.inventory.leftClick(x, y, null, false);
                if (obj != null)
                {
                    ShopMenu.chargePlayer(Game1.player, currency, -((obj is StardewValley.Object ? (int)((double)(obj as StardewValley.Object).sellToStorePrice() * (double)sellPercentage) : (int)((double)(obj.salePrice() / 2) * (double)sellPercentage)) * obj.Stack));
                    int num = obj.Stack / 8 + 2;
                    for (int index = 0; index < num; ++index)
                    {
                        animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, clickableComponent + new Vector2(32f, 32f), false, false)
                        {
                            alphaFade = 0.025f,
                            motion = new Vector2((float)Game1.random.Next(-3, 4), -4f),
                            acceleration = new Vector2(0.0f, 0.5f),
                            delayBeforeAnimationStart = index * 25,
                            scale = 2f
                        });
                        animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, clickableComponent + new Vector2(32f, 32f), false, false)
                        {
                            scale = 4f,
                            alphaFade = 0.025f,
                            delayBeforeAnimationStart = index * 50,
                            motion = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), new Vector2((float)(shop.xPositionOnScreen - 36), (float)(shop.yPositionOnScreen + shop.height - shop.inventory.height - 16)), 8f),
                            acceleration = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), new Vector2((float)(shop.xPositionOnScreen - 36), (float)(shop.yPositionOnScreen + shop.height - shop.inventory.height - 16)), 0.5f)
                        });
                    }
                    if (obj is StardewValley.Object && (obj as StardewValley.Object).Edibility != -300)
                    {
                        Item one = obj.getOne();
                        one.Stack = obj.Stack;
                        (Game1.getLocationFromName("SeedShop") as StardewValley.Locations.SeedShop).itemsToStartSellingTomorrow.Add(one);
                    }
                    Game1.playSound("sell");
                    Game1.playSound("purchase");
                }
            }
            else
            {
                heldItem = shop.inventory.leftClick(x, y, heldItem, true);
                Helper.Reflection.GetField<Item>(shop, "heldItem").SetValue(heldItem);
            }
            for (int i = currentItemIndex * unitsWide; i < forSale.Count && i < currentItemIndex * unitsWide + unitsWide * 3; ++i)
            {
                int ix = i % unitsWide;
                int iy = i / unitsWide;
                Rectangle rect = new Rectangle(shop.xPositionOnScreen + 16 + ix * UNIT_WIDTH, shop.yPositionOnScreen + 16 + iy * UNIT_HEIGHT - currentItemIndex * UNIT_HEIGHT, UNIT_WIDTH, UNIT_HEIGHT);
                if (rect.Contains(x, y) && forSale[i] != null )
                {
                    int numberToBuy = Math.Min(Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? Math.Min(Math.Min(5, ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) / Math.Max(1, itemPriceAndStock[forSale[i]][0])), Math.Max(1, itemPriceAndStock[forSale[i]][1])) : 1, forSale[i].maximumStackSize());
                    if (numberToBuy == -1)
                        numberToBuy = 1;
                    var tryToPurchaseItem = Helper.Reflection.GetMethod(shop, "tryToPurchaseItem");
                    if (numberToBuy > 0 && tryToPurchaseItem.Invoke<bool>(forSale[i], heldItem, numberToBuy, x, y, i))
                    {
                        itemPriceAndStock.Remove(forSale[i]);
                        forSale.RemoveAt(i);
                    }
                    else if (numberToBuy <= 0)
                    {
                        Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                        Game1.playSound("cancel");
                    }
                    if (heldItem != null && Game1.options.SnappyMenus && (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShopMenu) && Game1.player.addItemToInventoryBool(heldItem, false))
                    {
                        heldItem = (Item)null;
                        Helper.Reflection.GetField<Item>(shop, "heldItem").SetValue(heldItem);
                        DelayedAction.playSoundAfterDelay("coin", 100, (GameLocation)null);
                    }
                }
            }
        }

        private void doGridLayoutRightClick(ButtonPressedEventArgs e)
        {
            var forSale = Helper.Reflection.GetField<List<Item>>(shop, "forSale").GetValue();
            var itemPriceAndStock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(shop, "itemPriceAndStock").GetValue();
            var currency = Helper.Reflection.GetField<int>(shop, "currency").GetValue();
            var animations = Helper.Reflection.GetField<List<TemporaryAnimatedSprite>>(shop, "animations").GetValue();
            var poof = Helper.Reflection.GetField<TemporaryAnimatedSprite>(shop, "poof").GetValue();
            var heldItem = Helper.Reflection.GetField<Item>(shop, "heldItem").GetValue();
            var currentItemIndex = Helper.Reflection.GetField<int>(shop, "currentItemIndex").GetValue();
            const int UNIT_WIDTH = 160;
            const int UNIT_HEIGHT = 144;
            int unitsWide = (shop.width - 32) / UNIT_WIDTH;
            var sellPercentage = Helper.Reflection.GetField<float>(shop, "sellPercentage").GetValue();

            int x = (int)e.Cursor.ScreenPixels.X;
            int y = (int)e.Cursor.ScreenPixels.Y;

            if (shop.upperRightCloseButton.containsPoint(x, y))
            {
                shop.exitThisMenu(true);
                return;
            }

            // Copying a lot from right click code
            Vector2 clickableComponent = shop.inventory.snapToClickableComponent(x, y);
            if (heldItem == null)
            {
                Item obj = shop.inventory.rightClick(x, y, null, false);
                if (obj != null)
                {
                    ShopMenu.chargePlayer(Game1.player, currency, -((obj is StardewValley.Object ? (int)((double)(obj as StardewValley.Object).sellToStorePrice() * (double)sellPercentage) : (int)((double)(obj.salePrice() / 2) * (double)sellPercentage)) * obj.Stack));
                    Item obj2 = (Item)null;
                    if (Game1.mouseClickPolling > 300)
                        Game1.playSound("purchaseRepeat");
                    else
                        Game1.playSound("purchaseClick");
                    animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 64, 256, 64, 64), 9999f, 1, 999, clickableComponent + new Vector2(32f, 32f), false, false)
                    {
                        alphaFade = 0.025f,
                        motion = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), Game1.dayTimeMoneyBox.position + new Vector2(96f, 196f), 12f),
                        acceleration = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), Game1.dayTimeMoneyBox.position + new Vector2(96f, 196f), 0.5f)
                    });
                    if (obj is StardewValley.Object && (obj as StardewValley.Object).Edibility != -300)
                    {
                        (Game1.getLocationFromName("SeedShop") as StardewValley.Locations.SeedShop).itemsToStartSellingTomorrow.Add(obj.getOne());
                    }
                    if (shop.inventory.getItemAt(x, y) == null)
                    {
                        Game1.playSound("sell");
                        animations.Add(new TemporaryAnimatedSprite(5, clickableComponent + new Vector2(32f, 32f), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
                        {
                            motion = new Vector2(0.0f, -0.5f)
                        });
                    }
                }
            }
            else
            {
                heldItem = shop.inventory.leftClick(x, y, heldItem, true);
                Helper.Reflection.GetField<Item>(shop, "heldItem").SetValue(heldItem);
            }
            for (int i = currentItemIndex * unitsWide; i < forSale.Count && i < currentItemIndex * unitsWide + unitsWide * 3; ++i)
            {
                int ix = i % unitsWide;
                int iy = i / unitsWide;
                Rectangle rect = new Rectangle(shop.xPositionOnScreen + 16 + ix * UNIT_WIDTH, shop.yPositionOnScreen + 16 + iy * UNIT_HEIGHT - currentItemIndex * UNIT_HEIGHT, UNIT_WIDTH, UNIT_HEIGHT);
                if (rect.Contains(x, y) && forSale[i] != null)
                {
                    int index2 = i;
                    if (forSale[index2] == null)
                        break;
                    int numberToBuy = Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? Math.Min(Math.Min(5, ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) / itemPriceAndStock[forSale[index2]][0]), itemPriceAndStock[forSale[index2]][1]) : 1;
                    var tryToPurchaseItem = Helper.Reflection.GetMethod(shop, "tryToPurchaseItem");
                    if (numberToBuy > 0 && tryToPurchaseItem.Invoke<bool>(forSale[index2], heldItem, numberToBuy, x, y, index2))
                    {
                        itemPriceAndStock.Remove(forSale[index2]);
                        forSale.RemoveAt(index2);
                    }
                    if (heldItem == null || !Game1.options.SnappyMenus || (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is ShopMenu)) || !Game1.player.addItemToInventoryBool(heldItem, false))
                        break;
                    heldItem = (Item)null;
                    Helper.Reflection.GetField<Item>(shop, "heldItem").SetValue(heldItem);
                    DelayedAction.playSoundAfterDelay("coin", 100, (GameLocation)null);
                    break;
                }
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

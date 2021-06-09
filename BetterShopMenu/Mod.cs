using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using SpaceShared.APIs;
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
            Mod.instance = this;
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.onGameLaunched;
            helper.Events.Display.MenuChanged += this.onMenuChanged;
            helper.Events.GameLoop.UpdateTicked += this.onUpdateTicked;
            helper.Events.Display.RenderedActiveMenu += this.onRenderedActiveMenu;
            helper.Events.Input.ButtonPressed += this.onButtonPressed;
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = this.Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(this.ModManifest, () => Mod.Config = new Configuration(), () => this.Helper.WriteConfig(Mod.Config));
                capi.RegisterSimpleOption(this.ModManifest, "Grid Layout)", "Whether or not to use the grid layout in shops.", () => Mod.Config.GridLayout, (bool val) => Mod.Config.GridLayout = val);
            }
        }

        private ShopMenu shop;
        private bool firstTick = false;
        private List<ISalable> initialItems;
        private Dictionary<ISalable, int[]> initialStock;
        private List<int> categories;
        private int currCategory;
        private bool hasRecipes;
        private Dictionary<int, string> categoryNames;
        private int sorting = 0;
        private TextBox search;
        private void initShop(ShopMenu shopMenu)
        {
            this.shop = shopMenu;
            this.firstTick = true;
        }
        private void initShop2()
        {
            this.firstTick = false;

            this.initialItems = this.Helper.Reflection.GetField<List<ISalable>>(this.shop, "forSale").GetValue();
            this.initialStock = this.Helper.Reflection.GetField<Dictionary<ISalable, int[]>>(this.shop, "itemPriceAndStock").GetValue();

            this.categories = new List<int>();
            this.hasRecipes = false;
            foreach (var salable in this.initialItems)
            {
                var item = salable as Item;
                var obj = item as SObject;
                if (!this.categories.Contains(item?.Category ?? 0) && (obj == null || !obj.IsRecipe))
                    this.categories.Add(item?.Category ?? 0);
                if (obj != null && obj.IsRecipe)
                    this.hasRecipes = true;
            }
            this.currCategory = -1;

            this.categoryNames = new Dictionary<int, string>();
            this.categoryNames.Add(-1, "Everything");
            this.categoryNames.Add(0, "Other");
            this.categoryNames.Add(SObject.GreensCategory, "Greens");
            this.categoryNames.Add(SObject.GemCategory, "Gems");
            this.categoryNames.Add(SObject.VegetableCategory, "Vegetables");
            this.categoryNames.Add(SObject.FishCategory, "Fish");
            this.categoryNames.Add(SObject.EggCategory, "Egg");
            this.categoryNames.Add(SObject.MilkCategory, "Milk");
            this.categoryNames.Add(SObject.CookingCategory, "Cooking");
            this.categoryNames.Add(SObject.CraftingCategory, "Crafting");
            this.categoryNames.Add(SObject.BigCraftableCategory, "Big Craftables");
            this.categoryNames.Add(SObject.FruitsCategory, "Fruits");
            this.categoryNames.Add(SObject.SeedsCategory, "Seeds");
            this.categoryNames.Add(SObject.mineralsCategory, "Minerals");
            this.categoryNames.Add(SObject.flowersCategory, "Flowers");
            this.categoryNames.Add(SObject.meatCategory, "Meat");
            this.categoryNames.Add(SObject.metalResources, "Metals");
            this.categoryNames.Add(SObject.buildingResources, "Building Resources"); //?
            this.categoryNames.Add(SObject.sellAtPierres, "Sellable @ Pierres");
            this.categoryNames.Add(SObject.sellAtPierresAndMarnies, "Sellable @ Pierre's/Marnie's");
            this.categoryNames.Add(SObject.fertilizerCategory, "Fertilizer");
            this.categoryNames.Add(SObject.junkCategory, "Junk");
            this.categoryNames.Add(SObject.baitCategory, "Bait");
            this.categoryNames.Add(SObject.tackleCategory, "Tackle");
            this.categoryNames.Add(SObject.sellAtFishShopCategory, "Sellable @ Willy's");
            this.categoryNames.Add(SObject.furnitureCategory, "Furniture");
            this.categoryNames.Add(SObject.ingredientsCategory, "Ingredients");
            this.categoryNames.Add(SObject.artisanGoodsCategory, "Artisan Goods");
            this.categoryNames.Add(SObject.syrupCategory, "Syrups");
            this.categoryNames.Add(SObject.monsterLootCategory, "Monster Loot");
            this.categoryNames.Add(SObject.equipmentCategory, "Equipment");
            this.categoryNames.Add(SObject.hatCategory, "Hats");
            this.categoryNames.Add(SObject.ringCategory, "Rings");
            this.categoryNames.Add(SObject.weaponCategory, "Weapons");
            this.categoryNames.Add(SObject.bootsCategory, "Boots");
            this.categoryNames.Add(SObject.toolCategory, "Tools");
            this.categoryNames.Add(this.categories.Count == 0 ? 1 : this.categories.Count, "Recipes");

            this.search = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor); ;

            this.syncStock();
        }
        private void changeCategory(int amt)
        {
            this.currCategory += amt;

            if (this.currCategory == -2)
                this.currCategory = this.hasRecipes ? this.categories.Count : (this.categories.Count - 1);
            if (this.currCategory == this.categories.Count && !this.hasRecipes || this.currCategory > this.categories.Count)
                this.currCategory = -1;

            this.syncStock();
        }
        private void changeSorting(int amt)
        {
            this.sorting += amt;
            if (this.sorting > 2)
                this.sorting = 0;
            else if (this.sorting < 0)
                this.sorting = 2;

            this.syncStock();
        }
        private void syncStock()
        {
            var items = new List<ISalable>();
            var stock = new Dictionary<ISalable, int[]>();
            foreach (var item in this.initialItems)
            {
                if (this.itemMatchesCategory(item, this.currCategory) && (this.search.Text == null || item.DisplayName.ToLower().Contains(this.search.Text.ToLower())))
                {
                    items.Add(item);
                }
            }
            foreach (var item in this.initialStock)
            {
                if (this.itemMatchesCategory(item.Key, this.currCategory) && (this.search.Text == null || item.Key.DisplayName.ToLower().Contains(this.search.Text.ToLower())))
                {
                    stock.Add(item.Key, item.Value);
                }
            }

            this.Helper.Reflection.GetField<List<ISalable>>(this.shop, "forSale").SetValue(items);
            this.Helper.Reflection.GetField<Dictionary<ISalable, int[]>>(this.shop, "itemPriceAndStock").SetValue(stock);

            this.doSorting();
        }
        private void doSorting()
        {
            var items = this.Helper.Reflection.GetField<List<ISalable>>(this.shop, "forSale").GetValue();
            var stock = this.Helper.Reflection.GetField<Dictionary<ISalable, int[]>>(this.shop, "itemPriceAndStock").GetValue();
            if (this.sorting != 0)
            {
                if (this.sorting == 1)
                    items.Sort((a, b) => stock[a][0] - stock[b][0]);
                else if (this.sorting == 2)
                    items.Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (this.shop != null)
            {
                if (this.firstTick)
                    this.initShop2();
                this.search.Update();
            }
        }

        /// <summary>When a menu is open (<see cref="Game1.activeClickableMenu"/> isn't null), raised after that menu is drawn to the sprite batch but before it's rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (this.shop == null)
                return;

            Vector2 pos = new Vector2(this.shop.xPositionOnScreen + 25, this.shop.yPositionOnScreen + 525);
            IClickableMenu.drawTextureBox(Game1.spriteBatch, (int)pos.X, (int)pos.Y, 200, 72, Color.White);
            pos.X += 16;
            pos.Y += 16;
            string str = "Category: \n" + this.categoryNames[((this.currCategory == -1 || this.currCategory == this.categories.Count) ? this.currCategory : this.categories[this.currCategory])];
            Game1.spriteBatch.DrawString(Game1.dialogueFont, str, pos + new Vector2(-1, 1), new Color(224, 150, 80), 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            Game1.spriteBatch.DrawString(Game1.dialogueFont, str, pos, new Color(86, 22, 12), 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);

            pos = new Vector2(this.shop.xPositionOnScreen + 25, this.shop.yPositionOnScreen + 600);
            IClickableMenu.drawTextureBox(Game1.spriteBatch, (int)pos.X, (int)pos.Y, 200, 48, Color.White);
            pos.X += 16;
            pos.Y += 16;
            str = "Sorting: " + (this.sorting == 0 ? "None" : (this.sorting == 1 ? "Price" : "Name"));
            Game1.spriteBatch.DrawString(Game1.dialogueFont, str, pos + new Vector2(-1, 1), new Color(224, 150, 80), 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            Game1.spriteBatch.DrawString(Game1.dialogueFont, str, pos, new Color(86, 22, 12), 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);

            if (Mod.Config.GridLayout)
            {
                this.drawGridLayout();
            }

            pos.X = this.shop.xPositionOnScreen + 25;
            pos.Y = this.shop.yPositionOnScreen + 650;
            //Game1.spriteBatch.DrawString( Game1.dialogueFont, "Search: ", pos, Game1.textColor, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0 );
            this.search.X = (int)(pos.X);// + Game1.dialogueFont.MeasureString( "Search: " ).X);
            this.search.Y = (int)pos.Y;
            this.search.Draw(Game1.spriteBatch);

            this.shop.drawMouse(Game1.spriteBatch);
        }

        private void drawGridLayout()
        {
            var forSale = this.Helper.Reflection.GetField<List<ISalable>>(this.shop, "forSale").GetValue();
            var itemPriceAndStock = this.Helper.Reflection.GetField<Dictionary<ISalable, int[]>>(this.shop, "itemPriceAndStock").GetValue();
            var currency = this.Helper.Reflection.GetField<int>(this.shop, "currency").GetValue();
            var animations = this.Helper.Reflection.GetField<List<TemporaryAnimatedSprite>>(this.shop, "animations").GetValue();
            var poof = this.Helper.Reflection.GetField<TemporaryAnimatedSprite>(this.shop, "poof").GetValue();
            var heldItem = this.Helper.Reflection.GetField<Item>(this.shop, "heldItem").GetValue();
            var currentItemIndex = this.Helper.Reflection.GetField<int>(this.shop, "currentItemIndex").GetValue();
            var scrollBar = this.Helper.Reflection.GetField<ClickableTextureComponent>(this.shop, "scrollBar").GetValue();
            var scrollBarRunner = this.Helper.Reflection.GetField<Rectangle>(this.shop, "scrollBarRunner").GetValue();
            const int UNIT_WIDTH = 160;
            const int UNIT_HEIGHT = 144;
            int unitsWide = (this.shop.width - 32) / UNIT_WIDTH;
            ISalable hover = null;

            Texture2D purchase_texture = Game1.mouseCursors;
            Rectangle purchase_window_border = new Rectangle(384, 373, 18, 18);
            Rectangle purchase_item_rect = new Rectangle(384, 396, 15, 15);
            int purchase_item_text_color = -1;
            bool purchase_draw_item_background = true;
            Rectangle purchase_item_background = new Rectangle(296, 363, 18, 18);
            Color purchase_selected_color = Color.Wheat;
            if (this.shop.storeContext == "QiGemShop")
            {
                purchase_texture = Game1.mouseCursors2;
                purchase_window_border = new Rectangle(0, 256, 18, 18);
                purchase_item_rect = new Rectangle(18, 256, 15, 15);
                purchase_item_text_color = 4;
                purchase_selected_color = Color.Blue;
                purchase_draw_item_background = true;
                purchase_item_background = new Rectangle(33, 256, 18, 18);
            }


            //IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), shop.xPositionOnScreen + shop.width - shop.inventory.width - 32 - 24, shop.yPositionOnScreen + shop.height - 256 + 40, shop.inventory.width + 56, shop.height - 448 + 20, Color.White, 4f, true);
            //IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), shop.xPositionOnScreen, shop.yPositionOnScreen, shop.width, shop.height - 256 + 32 + 4, Color.White, 4f, true);
            IClickableMenu.drawTextureBox(Game1.spriteBatch, purchase_texture, purchase_window_border, this.shop.xPositionOnScreen, this.shop.yPositionOnScreen, this.shop.width, this.shop.height - 256 + 32 + 4, Color.White, 4f);
            for (int i = currentItemIndex * unitsWide; i < forSale.Count && i < currentItemIndex * unitsWide + unitsWide * 3; ++i)
            {
                bool failedCanPurchaseCheck = false;
                if (this.shop.canPurchaseCheck != null && !this.shop.canPurchaseCheck(i))
                {
                    failedCanPurchaseCheck = true;
                }
                int ix = i % unitsWide;
                int iy = i / unitsWide;
                Rectangle rect = new Rectangle(this.shop.xPositionOnScreen + 16 + ix * UNIT_WIDTH, this.shop.yPositionOnScreen + 16 + iy * UNIT_HEIGHT - currentItemIndex * UNIT_HEIGHT, UNIT_WIDTH, UNIT_HEIGHT);
                IClickableMenu.drawTextureBox(Game1.spriteBatch, purchase_texture, purchase_item_rect, rect.X, rect.Y, rect.Width, rect.Height, rect.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? purchase_selected_color : Color.White, 4f, false);
                ISalable item = forSale[i];
                bool buyInStacks = item.Stack > 1 && item.Stack != int.MaxValue && itemPriceAndStock[item][1] == int.MaxValue;
                StackDrawType stackDrawType;
                if (this.shop.storeContext == "QiGemShop")
                {
                    stackDrawType = StackDrawType.HideButShowQuality;
                    buyInStacks = (item.Stack > 1);
                }
                else if (this.shop.itemPriceAndStock[item][1] == int.MaxValue)
                {
                    stackDrawType = StackDrawType.HideButShowQuality;
                }
                else
                {
                    stackDrawType = StackDrawType.Draw_OneInclusive;
                    if (this.Helper.Reflection.GetField<bool>(this.shop, "_isStorageShop").GetValue())
                    {
                        stackDrawType = StackDrawType.Draw;
                    }
                }
                if (forSale[i].ShouldDrawIcon())
                {
                    if (purchase_draw_item_background)
                    {
                        //Game1.spriteBatch.Draw( purchase_texture, new Vector2( rect.X + 48 + 4, rect.Y + 16 ), purchase_item_background, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f );
                    }
                    item.drawInMenu(Game1.spriteBatch, new Vector2(rect.X + 48, rect.Y + 16), 1f, 1, 1, stackDrawType, Color.White, true);
                }
                int price = itemPriceAndStock[forSale[i]][0];
                var priceStr = price.ToString();
                if (price > 0)
                {
                    SpriteText.drawString(Game1.spriteBatch, priceStr, rect.Right - SpriteText.getWidthOfString(priceStr) - 16, rect.Y + 80, alpha: ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) >= price && !failedCanPurchaseCheck ? 1f : 0.5f, color: purchase_item_text_color);
                    //Utility.drawWithShadow(Game1.spriteBatch, Game1.mouseCursors, new Vector2(rect.Right - 16, rect.Y + 80), new Rectangle(193 + currency * 9, 373, 9, 10), Color.White, 0, Vector2.Zero, 1, layerDepth: 1);
                }
                else if (itemPriceAndStock[forSale[i]].Length > 2)
                {
                    int required_item_count = 5;
                    int requiredItem = itemPriceAndStock[forSale[i]][2];
                    if (itemPriceAndStock[forSale[i]].Length > 3)
                    {
                        required_item_count = itemPriceAndStock[forSale[i]][3];
                    }
                    bool hasEnoughToTrade = Game1.player.hasItemInInventory(requiredItem, required_item_count);
                    if (this.shop.canPurchaseCheck != null && !this.shop.canPurchaseCheck(i))
                    {
                        hasEnoughToTrade = false;
                    }
                    float textWidth = SpriteText.getWidthOfString("x" + required_item_count);
                    Utility.drawWithShadow(Game1.spriteBatch, Game1.objectSpriteSheet, new Vector2((float)(rect.Right - 64) - textWidth, rect.Y + 80 - 4), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, requiredItem, 16, 16), Color.White * (hasEnoughToTrade ? 1f : 0.25f), 0f, Vector2.Zero, 3, flipped: false, -1f, -1, -1, hasEnoughToTrade ? 0.35f : 0f);
                    SpriteText.drawString(Game1.spriteBatch, "x" + required_item_count, rect.Right - (int)textWidth - 16, rect.Y + 80, 999999, -1, 999999, hasEnoughToTrade ? 1f : 0.5f, 0.88f, junimoText: false, -1, "", purchase_item_text_color);
                }
                if (rect.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
                    hover = forSale[i];
            }
            if (forSale.Count == 0)
                SpriteText.drawString(Game1.spriteBatch, Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583"), this.shop.xPositionOnScreen + this.shop.width / 2 - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583"), 999999) / 2, this.shop.yPositionOnScreen + this.shop.height / 2 - 128, 999999, -1, 999999, 1f, 0.88f, false, -1, "", -1);
            //shop.inventory.draw(Game1.spriteBatch);
            // Moved currency here so above doesn't draw over it
            //if (currency == 0)
            //    Game1.dayTimeMoneyBox.drawMoneyBox(Game1.spriteBatch, shop.xPositionOnScreen - 36, shop.yPositionOnScreen + shop.height - shop.inventory.height + 48);
            for (int index = animations.Count - 1; index >= 0; --index)
            {
                if (animations[index].update(Game1.currentGameTime))
                    animations.RemoveAt(index);
                else
                    animations[index].draw(Game1.spriteBatch, true, 0, 0, 1f);
            }
            if (poof != null)
                poof.draw(Game1.spriteBatch, false, 0, 0, 1f);
            // arrows already drawn
            if (forSale.Count > 18)
            {
                IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f, true);
                scrollBar.draw(Game1.spriteBatch);
            }
            if (hover != null)
            {
                string hoverText = hover.getDescription();
                string boldTitleText = hover.DisplayName;
                int hoverPrice = itemPriceAndStock == null || !itemPriceAndStock.ContainsKey(hover) ? hover.salePrice() : itemPriceAndStock[hover][0];
                int getHoveredItemExtraItemIndex = -1;
                if (itemPriceAndStock != null && hover != null && (itemPriceAndStock.ContainsKey(hover) && itemPriceAndStock[hover].Length > 2))
                    getHoveredItemExtraItemIndex = itemPriceAndStock[hover][2];
                int getHoveredItemExtraItemAmount = 5;
                if (itemPriceAndStock != null && hover != null && itemPriceAndStock.ContainsKey(hover) && itemPriceAndStock[hover].Length > 3)
                    getHoveredItemExtraItemAmount = itemPriceAndStock[hover][3];
                IClickableMenu.drawToolTip(Game1.spriteBatch, hoverText, boldTitleText, hover as Item, heldItem != null, -1, currency, getHoveredItemExtraItemIndex, getHoveredItemExtraItemAmount, (CraftingRecipe)null, hoverPrice);
            }
            if (heldItem != null)
                heldItem.drawInMenu(Game1.spriteBatch, new Vector2((float)(Game1.getOldMouseX() + 8), (float)(Game1.getOldMouseY() + 8)), 1f);
            // some other stuff I don't think matters?
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this.shop == null)
                return;

            if (e.Button == SButton.MouseLeft || e.Button == SButton.MouseRight)
            {
                int x = (int)e.Cursor.ScreenPixels.X;
                int y = (int)e.Cursor.ScreenPixels.Y;
                int direction = e.Button == SButton.MouseLeft ? 1 : -1;

                if (new Rectangle(this.shop.xPositionOnScreen + 25, this.shop.yPositionOnScreen + 525, 200, 72).Contains(x, y))
                    this.changeCategory(direction);
                if (new Rectangle(this.shop.xPositionOnScreen + 25, this.shop.yPositionOnScreen + 600, 200, 48).Contains(x, y))
                    this.changeSorting(direction);

                if (Mod.Config.GridLayout)
                {
                    this.Helper.Input.Suppress(e.Button);
                    if (e.Button == SButton.MouseRight)
                        this.doGridLayoutRightClick(e);
                    else
                        this.doGridLayoutLeftClick(e);
                }
            }
            else if ((e.Button >= SButton.A && e.Button <= SButton.Z || e.Button == SButton.Space || e.Button == SButton.Back) && this.search.Selected)
            {
                this.Helper.Input.Suppress(e.Button);
                this.syncStock();
            }
        }

        private void doGridLayoutLeftClick(ButtonPressedEventArgs e)
        {
            var forSale = this.Helper.Reflection.GetField<List<ISalable>>(this.shop, "forSale").GetValue();
            var itemPriceAndStock = this.Helper.Reflection.GetField<Dictionary<ISalable, int[]>>(this.shop, "itemPriceAndStock").GetValue();
            var currency = this.Helper.Reflection.GetField<int>(this.shop, "currency").GetValue();
            var animations = this.Helper.Reflection.GetField<List<TemporaryAnimatedSprite>>(this.shop, "animations").GetValue();
            var poof = this.Helper.Reflection.GetField<TemporaryAnimatedSprite>(this.shop, "poof").GetValue();
            var heldItem = this.Helper.Reflection.GetField<ISalable>(this.shop, "heldItem").GetValue();
            var currentItemIndex = this.Helper.Reflection.GetField<int>(this.shop, "currentItemIndex").GetValue();
            var sellPercentage = this.Helper.Reflection.GetField<float>(this.shop, "sellPercentage").GetValue();
            var scrollBar = this.Helper.Reflection.GetField<ClickableTextureComponent>(this.shop, "scrollBar").GetValue();
            var scrollBarRunner = this.Helper.Reflection.GetField<Rectangle>(this.shop, "scrollBarRunner").GetValue();
            var downArrow = this.Helper.Reflection.GetField<ClickableTextureComponent>(this.shop, "downArrow").GetValue();
            var upArrow = this.Helper.Reflection.GetField<ClickableTextureComponent>(this.shop, "upArrow").GetValue();
            const int UNIT_WIDTH = 160;
            const int UNIT_HEIGHT = 144;
            int unitsWide = (this.shop.width - 32) / UNIT_WIDTH;

            int x = (int)e.Cursor.ScreenPixels.X;
            int y = (int)e.Cursor.ScreenPixels.Y;

            if (this.shop.upperRightCloseButton.containsPoint(x, y))
            {
                this.shop.exitThisMenu(true);
                return;
            }

            // Copying a lot from left click code
            if (downArrow.containsPoint(x, y) && currentItemIndex < Math.Max(0, forSale.Count - 18))
            {
                downArrow.scale = downArrow.baseScale;
                this.Helper.Reflection.GetField<int>(this.shop, "currentItemIndex").SetValue(currentItemIndex += 1);
                if (forSale.Count > 0)
                {
                    scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, (forSale.Count / 6) - 1 + 1) * currentItemIndex + upArrow.bounds.Bottom + 4;
                    if (currentItemIndex == forSale.Count / 6 - 1)
                    {
                        scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
                    }
                }
                Game1.playSound("shwip");
            }
            else if (upArrow.containsPoint(x, y) && currentItemIndex > 0)
            {
                upArrow.scale = upArrow.baseScale;
                this.Helper.Reflection.GetField<int>(this.shop, "currentItemIndex").SetValue(currentItemIndex -= 1);
                if (forSale.Count > 0)
                {
                    scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, (forSale.Count / 6) - 1 + 1) * currentItemIndex + upArrow.bounds.Bottom + 4;
                    if (currentItemIndex == forSale.Count / 6 - 1)
                    {
                        scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
                    }
                }
                Game1.playSound("shwip");
            }
            else if (scrollBarRunner.Contains(x, y))
            {
                int y1 = scrollBar.bounds.Y;
                scrollBar.bounds.Y = Math.Min(this.shop.yPositionOnScreen + this.shop.height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, this.shop.yPositionOnScreen + upArrow.bounds.Height + 20));
                currentItemIndex = Math.Min(forSale.Count / 6 - 1, Math.Max(0, (int)((double)forSale.Count / 6 * (double)((float)(y - scrollBarRunner.Y) / (float)scrollBarRunner.Height))));
                this.Helper.Reflection.GetField<int>(this.shop, "currentItemIndex").SetValue(currentItemIndex);
                if (forSale.Count > 0)
                {
                    scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, (forSale.Count / 6) - 1 + 1) * currentItemIndex + upArrow.bounds.Bottom + 4;
                    if (currentItemIndex == forSale.Count / 6 - 1)
                    {
                        scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
                    }
                }
                int y2 = scrollBar.bounds.Y;
                if (y1 == y2)
                    return;
                Game1.playSound("shiny4");
            }
            Vector2 clickableComponent = this.shop.inventory.snapToClickableComponent(x, y);
            if (heldItem == null)
            {
                Item obj = this.shop.inventory.leftClick(x, y, null, false);
                if (obj != null)
                {
                    if (this.shop.onSell != null)
                    {
                        this.shop.onSell(obj);
                    }
                    else
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
                                motion = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), new Vector2((float)(this.shop.xPositionOnScreen - 36), (float)(this.shop.yPositionOnScreen + this.shop.height - this.shop.inventory.height - 16)), 8f),
                                acceleration = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), new Vector2((float)(this.shop.xPositionOnScreen - 36), (float)(this.shop.yPositionOnScreen + this.shop.height - this.shop.inventory.height - 16)), 0.5f)
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
            }
            else
            {
                heldItem = this.shop.inventory.leftClick(x, y, (Item)heldItem, true);
                this.Helper.Reflection.GetField<ISalable>(this.shop, "heldItem").SetValue(heldItem);
            }
            for (int i = currentItemIndex * unitsWide; i < forSale.Count && i < currentItemIndex * unitsWide + unitsWide * 3; ++i)
            {
                int ix = i % unitsWide;
                int iy = i / unitsWide;
                Rectangle rect = new Rectangle(this.shop.xPositionOnScreen + 16 + ix * UNIT_WIDTH, this.shop.yPositionOnScreen + 16 + iy * UNIT_HEIGHT - currentItemIndex * UNIT_HEIGHT, UNIT_WIDTH, UNIT_HEIGHT);
                if (rect.Contains(x, y) && forSale[i] != null)
                {
                    int numberToBuy = Math.Min(Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? Math.Min(Math.Min(5, ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) / Math.Max(1, itemPriceAndStock[forSale[i]][0])), Math.Max(1, itemPriceAndStock[forSale[i]][1])) : 1, forSale[i].maximumStackSize());
                    if (numberToBuy == -1)
                        numberToBuy = 1;
                    var tryToPurchaseItem = this.Helper.Reflection.GetMethod(this.shop, "tryToPurchaseItem");
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
                    if (heldItem != null && Game1.options.SnappyMenus && (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShopMenu) && Game1.player.addItemToInventoryBool((Item)heldItem, false))
                    {
                        heldItem = (Item)null;
                        this.Helper.Reflection.GetField<ISalable>(this.shop, "heldItem").SetValue(heldItem);
                        DelayedAction.playSoundAfterDelay("coin", 100, (GameLocation)null);
                    }
                }
            }
        }

        private void doGridLayoutRightClick(ButtonPressedEventArgs e)
        {
            var forSale = this.Helper.Reflection.GetField<List<ISalable>>(this.shop, "forSale").GetValue();
            var itemPriceAndStock = this.Helper.Reflection.GetField<Dictionary<ISalable, int[]>>(this.shop, "itemPriceAndStock").GetValue();
            var currency = this.Helper.Reflection.GetField<int>(this.shop, "currency").GetValue();
            var animations = this.Helper.Reflection.GetField<List<TemporaryAnimatedSprite>>(this.shop, "animations").GetValue();
            var poof = this.Helper.Reflection.GetField<TemporaryAnimatedSprite>(this.shop, "poof").GetValue();
            var heldItem = this.Helper.Reflection.GetField<ISalable>(this.shop, "heldItem").GetValue();
            var currentItemIndex = this.Helper.Reflection.GetField<int>(this.shop, "currentItemIndex").GetValue();
            var sellPercentage = this.Helper.Reflection.GetField<float>(this.shop, "sellPercentage").GetValue();
            const int UNIT_WIDTH = 160;
            const int UNIT_HEIGHT = 144;
            int unitsWide = (this.shop.width - 32) / UNIT_WIDTH;

            int x = (int)e.Cursor.ScreenPixels.X;
            int y = (int)e.Cursor.ScreenPixels.Y;

            if (this.shop.upperRightCloseButton.containsPoint(x, y))
            {
                this.shop.exitThisMenu(true);
                return;
            }

            // Copying a lot from right click code
            Vector2 clickableComponent = this.shop.inventory.snapToClickableComponent(x, y);
            if (heldItem == null)
            {
                Item obj = this.shop.inventory.rightClick(x, y, null, false);
                if (obj != null)
                {
                    if (this.shop.onSell != null)
                    {
                        this.shop.onSell(obj);
                    }
                    else
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
                        if (this.shop.inventory.getItemAt(x, y) == null)
                        {
                            Game1.playSound("sell");
                            animations.Add(new TemporaryAnimatedSprite(5, clickableComponent + new Vector2(32f, 32f), Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
                            {
                                motion = new Vector2(0.0f, -0.5f)
                            });
                        }
                    }
                }
            }
            else
            {
                heldItem = this.shop.inventory.leftClick(x, y, (Item)heldItem, true);
                this.Helper.Reflection.GetField<ISalable>(this.shop, "heldItem").SetValue(heldItem);
            }
            for (int i = currentItemIndex * unitsWide; i < forSale.Count && i < currentItemIndex * unitsWide + unitsWide * 3; ++i)
            {
                int ix = i % unitsWide;
                int iy = i / unitsWide;
                Rectangle rect = new Rectangle(this.shop.xPositionOnScreen + 16 + ix * UNIT_WIDTH, this.shop.yPositionOnScreen + 16 + iy * UNIT_HEIGHT - currentItemIndex * UNIT_HEIGHT, UNIT_WIDTH, UNIT_HEIGHT);
                if (rect.Contains(x, y) && forSale[i] != null)
                {
                    int index2 = i;
                    if (forSale[index2] == null)
                        break;
                    int numberToBuy = Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? Math.Min(Math.Min(5, ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) / itemPriceAndStock[forSale[index2]][0]), itemPriceAndStock[forSale[index2]][1]) : 1;
                    var tryToPurchaseItem = this.Helper.Reflection.GetMethod(this.shop, "tryToPurchaseItem");
                    if (numberToBuy > 0 && tryToPurchaseItem.Invoke<bool>(forSale[index2], heldItem, numberToBuy, x, y, index2))
                    {
                        itemPriceAndStock.Remove(forSale[index2]);
                        forSale.RemoveAt(index2);
                    }
                    if (heldItem == null || !Game1.options.SnappyMenus || (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is ShopMenu)) || !Game1.player.addItemToInventoryBool((Item)heldItem, false))
                        break;
                    heldItem = (Item)null;
                    this.Helper.Reflection.GetField<ISalable>(this.shop, "heldItem").SetValue(heldItem);
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
            if (e.NewMenu is ShopMenu shopMenu)
            {
                Log.trace("Found shop menu!");
                this.initShop(shopMenu);
            }
            else
            {
                this.shop = null;
                if (this.search != null)
                {
                    this.search.Selected = false;
                    this.search = null;
                }
            }
        }

        private bool itemMatchesCategory(ISalable item, int cat)
        {
            var obj = item as SObject;
            if (cat == -1)
                return true;
            if (cat == this.categories.Count)
                return obj != null && obj.IsRecipe;
            if (this.categories[cat] == ((item as Item)?.Category ?? 0))
                return (obj == null || !obj.IsRecipe);
            return false;
        }
    }
}

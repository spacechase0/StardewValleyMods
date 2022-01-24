using System;
using System.Collections.Generic;
using BetterShopMenu.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using SObject = StardewValley.Object;
using HarmonyLib;

namespace BetterShopMenu
{
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static Configuration Config;

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);

            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            //helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            //helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
            //helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            //helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;

            var harmony = new Harmony(this.ModManifest.UniqueID);
            System.Reflection.MethodInfo mInfo;

            // these patches only patch the source method out when the grid layout is enabled

            // this patches out the ShopMenu mouse wheel code.
            mInfo = harmony.Patch(original: AccessTools.Method(typeof(StardewValley.Menus.ShopMenu), nameof(StardewValley.Menus.ShopMenu.receiveScrollWheelAction)),
                                  prefix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(ShopMenuPatches.ShopMenu_receiveScrollWheelAction_Prefix))
                                 );

            // this patches out the ShopMenu mouse right click code.
            // this allows us to trigger a delay for doing the right click, hold auto purchase.
            mInfo = harmony.Patch(original: AccessTools.Method(typeof(StardewValley.Menus.ShopMenu), nameof(StardewValley.Menus.ShopMenu.receiveRightClick)),
                                  prefix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(ShopMenuPatches.ShopMenu_receiveRightClick_Prefix))
                                 );

            // this patches out ShopMenu.draw.
            // excluding the grid layout draw, our draw procedure is really just a copy of the Stardew ShopMenu.draw code.
            System.Type[] ovlParams = new System.Type[] { typeof(SpriteBatch) };
            mInfo = harmony.Patch(original: AccessTools.Method(typeof(StardewValley.Menus.ShopMenu), nameof(StardewValley.Menus.ShopMenu.draw), ovlParams),
                                  prefix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(ShopMenuPatches.ShopMenu_draw_Prefix))
                                 );
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => Mod.Config = new Configuration(),
                    save: () => this.Helper.WriteConfig(Mod.Config)
                );
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: I18n.Config_GridLayout_Name,
                    tooltip: I18n.Config_GridLayout_Tooltip,
                    getValue: () => Mod.Config.ExperimentalGridLayout,
                    setValue: value => Mod.Config.ExperimentalGridLayout = value
                );
            }
        }

        private ShopMenu Shop;
        private bool FirstTick;
        private List<ISalable> InitialItems;
        private Dictionary<ISalable, int[]> InitialStock;
        private List<int> Categories;
        private int CurrCategory;
        private bool HasRecipes;
        private Dictionary<int, string> CategoryNames;
        private int Sorting;
        private TextBox Search;

        public const int UnitWidth = 160;
        public const int UnitHeight = 144;
        public const int UnitsHigh = 3;
        public const int UnitsWide = 6;//(Shop.width - 32) / UnitWidth

        private bool HaveStockList;
        private Dictionary<int, string> CropData;
        private const int SeedsOtherCategory = -174; //seeds - 100;

        private Point PurchasePoint;
  
        private void InitShop(ShopMenu shopMenu)
        {
            this.Shop = shopMenu;
            this.FirstTick = true;

            this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this.Helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
        }

        private void InitShop2()
        {
            this.FirstTick = false;

            this.InitialItems = this.Shop.forSale;
            this.InitialStock = this.Shop.itemPriceAndStock;

            this.CropData = null;
            this.HaveStockList = Game1.MasterPlayer.hasOrWillReceiveMail("PierreStocklist");
            if (this.HaveStockList)
            {
                this.CropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
            }

            this.Categories = new List<int>();
            this.HasRecipes = false;
            foreach (var salable in this.InitialItems)
            {
                var item = salable as Item;
                var obj = item as SObject;
                int sCat = item?.Category ?? 0;

                if (!this.Categories.Contains(sCat) && (obj == null || !obj.IsRecipe))
                    this.Categories.Add(sCat);

                if ((sCat == SObject.SeedsCategory) && this.HaveStockList && !this.Categories.Contains(SeedsOtherCategory))
                    this.Categories.Add(SeedsOtherCategory);

                if (obj?.IsRecipe == true)
                    this.HasRecipes = true;
            }
            this.CurrCategory = -1;

            this.CategoryNames = new Dictionary<int, string>
            {
                [-1] = I18n.Categories_Everything(),
                [0] = I18n.Categories_Other(),
                [SObject.GreensCategory] = I18n.Categories_Greens(),
                [SObject.GemCategory] = I18n.Categories_Gems(),
                [SObject.VegetableCategory] = I18n.Categories_Vegetables(),
                [SObject.FishCategory] = I18n.Categories_Fish(),
                [SObject.EggCategory] = I18n.Categories_Eggs(),
                [SObject.MilkCategory] = I18n.Categories_Milk(),
                [SObject.CookingCategory] = I18n.Categories_Cooking(),
                [SObject.CraftingCategory] = I18n.Categories_Crafting(),
                [SObject.BigCraftableCategory] = I18n.Categories_BigCraftables(),
                [SObject.FruitsCategory] = I18n.Categories_Fruits(),
                [SObject.SeedsCategory] = I18n.Categories_Seeds(),
                [SeedsOtherCategory] = I18n.Categories_SeedsOther(),
                [SObject.mineralsCategory] = I18n.Categories_Minerals(),
                [SObject.flowersCategory] = I18n.Categories_Flowers(),
                [SObject.meatCategory] = I18n.Categories_Meat(),
                [SObject.metalResources] = I18n.Categories_Metals(),
                [SObject.buildingResources] = I18n.Categories_BuildingResources(), //?
                [SObject.sellAtPierres] = I18n.Categories_SellToPierre(),
                [SObject.sellAtPierresAndMarnies] = I18n.Categories_SellToPierreOrMarnie(),
                [SObject.fertilizerCategory] = I18n.Categories_Fertilizer(),
                [SObject.junkCategory] = I18n.Categories_Junk(),
                [SObject.baitCategory] = I18n.Categories_Bait(),
                [SObject.tackleCategory] = I18n.Categories_Tackle(),
                [SObject.sellAtFishShopCategory] = I18n.Categories_SellToWilly(),
                [SObject.furnitureCategory] = I18n.Categories_Furniture(),
                [SObject.ingredientsCategory] = I18n.Categories_Ingredients(),
                [SObject.artisanGoodsCategory] = I18n.Categories_ArtisanGoods(),
                [SObject.syrupCategory] = I18n.Categories_Syrups(),
                [SObject.monsterLootCategory] = I18n.Categories_MonsterLoot(),
                [SObject.equipmentCategory] = I18n.Categories_Equipment(),
                [SObject.hatCategory] = I18n.Categories_Hats(),
                [SObject.ringCategory] = I18n.Categories_Rings(),
                [SObject.weaponCategory] = I18n.Categories_Weapons(),
                [SObject.bootsCategory] = I18n.Categories_Boots(),
                [SObject.toolCategory] = I18n.Categories_Tools(),
                [SObject.clothingCategory] = I18n.Categories_Clothing(),
                [this.Categories.Count == 0 ? 1 : this.Categories.Count] = I18n.Categories_Recipes()
            };

            this.Search = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor);

            this.SyncStock();
        }

        private void ChangeCategory(int amt)
        {
            this.CurrCategory += amt;

            if (this.CurrCategory == -2)
                this.CurrCategory = this.HasRecipes ? this.Categories.Count : (this.Categories.Count - 1);
            if (this.CurrCategory == this.Categories.Count && !this.HasRecipes || this.CurrCategory > this.Categories.Count)
                this.CurrCategory = -1;

            this.SyncStock();
        }
        private void ChangeSorting(int amt)
        {
            this.Sorting += amt;
            if (this.Sorting > 2)
                this.Sorting = 0;
            else if (this.Sorting < 0)
                this.Sorting = 2;

            this.SyncStock();
        }

        private bool SeedsFilter(ISalable item, bool inSeason)
        {
            if (this.HaveStockList && (item is Item thisItem))
            {
                int seedIndex = thisItem.ParentSheetIndex;

                if (this.CropData.ContainsKey(seedIndex))
                {
                    string[] split = this.CropData[seedIndex].Split('/');
                    return split[1].Contains(Game1.currentSeason) == inSeason;
                }
                return inSeason; //have this stuff show in the in season list. saplings are like this.
            }
            return true;
        }

        private bool ItemMatchesCategory(ISalable item, int cat)
        {
            var obj = item as SObject;
            if (cat == -1)
                return true;
            if (cat == this.Categories.Count)
                return obj?.IsRecipe == true;
            if ((this.Categories[cat] == SeedsOtherCategory) && (item is Item seedItem) && (seedItem.Category == SObject.SeedsCategory))
                return true;
            if (this.Categories[cat] == ((item as Item)?.Category ?? 0))
                return (obj == null || !obj.IsRecipe);
            return false;
        }

        private void SyncStock()
        {
            var items = new List<ISalable>();
            var stock = new Dictionary<ISalable, int[]>();

            this.Shop.currentItemIndex = 0;

            int curCat = this.CurrCategory;
            int sCat = 0;
            bool inSeason = true;
            if ((curCat >= 0) && (curCat < this.Categories.Count))
            {
                sCat = this.Categories[curCat];
                inSeason = (sCat == SObject.SeedsCategory);
            }
            string search = this.Search.Text.ToLower();

            foreach (var item in this.InitialItems)
            {
                if (this.ItemMatchesCategory(item, curCat) && (this.Search.Text == null || item.DisplayName.ToLower().Contains(search)))
                {
                    if (
                        (curCat < 0) ||
                        ((sCat != SObject.SeedsCategory) && (sCat != SeedsOtherCategory)) ||
                        this.SeedsFilter(item, inSeason)
                       )
                    {
                        items.Add(item);
                    }
                }
            }
            foreach (var item in this.InitialStock)
            {
                if (this.ItemMatchesCategory(item.Key, curCat) && (this.Search.Text == null || item.Key.DisplayName.ToLower().Contains(search)))
                {
                    if (
                        (curCat < 0) ||
                        ((sCat != SObject.SeedsCategory) && (sCat != SeedsOtherCategory)) ||
                        this.SeedsFilter(item.Key, inSeason)
                       )
                    {
                        stock.Add(item.Key, item.Value);
                    }
                }
            }

            this.Shop.forSale = items;
            this.Shop.itemPriceAndStock = stock;

            this.DoSorting();
        }
        private void DoSorting()
        {
            var items = this.Shop.forSale;
            var stock = this.Shop.itemPriceAndStock;
            if (this.Sorting != 0)
            {
                if (this.Sorting == 1)
                    items.Sort((a, b) => stock[a][0] - stock[b][0]);
                else if (this.Sorting == 2)
                    items.Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));
            }
        }

        /// <summary>When a menu is open (<see cref="Game1.activeClickableMenu"/> isn't null), raised after that menu is drawn to the sprite batch but before it's rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (this.Shop == null)
                return;

            if (Mod.Config.ExperimentalGridLayout)
                this.DrawGridLayout(e.SpriteBatch);
            else
                this.DrawNewFields(e.SpriteBatch);

            this.Shop.drawMouse(e.SpriteBatch);
        }

        private void DrawNewFields(SpriteBatch b)
        {
            Vector2 pos = new Vector2(this.Shop.xPositionOnScreen + 25, this.Shop.yPositionOnScreen + 525);
            IClickableMenu.drawTextureBox(b, (int)pos.X, (int)pos.Y, 200, 72, Color.White);
            pos.X += 16;
            pos.Y += 16;
            string str = $"{I18n.Filter_Category()}\n" + this.CategoryNames[((this.CurrCategory == -1 || this.CurrCategory == this.Categories.Count) ? this.CurrCategory : this.Categories[this.CurrCategory])];
            b.DrawString(Game1.dialogueFont, str, pos + new Vector2(-1, 1), new Color(224, 150, 80), 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            b.DrawString(Game1.dialogueFont, str, pos, new Color(86, 22, 12), 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);

            pos = new Vector2(this.Shop.xPositionOnScreen + 25, this.Shop.yPositionOnScreen + 600);
            IClickableMenu.drawTextureBox(b, (int)pos.X, (int)pos.Y, 200, 48, Color.White);
            pos.X += 16;
            pos.Y += 16;
            str = I18n.Filter_Sorting() + " " + (this.Sorting == 0 ? I18n.Sort_None() : (this.Sorting == 1 ? I18n.Sort_Price() : I18n.Sort_Name()));
            b.DrawString(Game1.dialogueFont, str, pos + new Vector2(-1, 1), new Color(224, 150, 80), 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            b.DrawString(Game1.dialogueFont, str, pos, new Color(86, 22, 12), 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);

            pos.X = this.Shop.xPositionOnScreen + 25;
            pos.Y = this.Shop.yPositionOnScreen + 650;
            //e.SpriteBatch.DrawString( Game1.dialogueFont, "Search: ", pos, Game1.textColor, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0 );
            this.Search.X = (int)pos.X;// + Game1.dialogueFont.MeasureString( "Search: " ).X);
            this.Search.Y = (int)pos.Y;
            this.Search.Draw(b);
        }

        private void DrawGridLayout(SpriteBatch b)
        {
            var shop = this.Shop;
            var forSale = shop.forSale;
            var itemPriceAndStock = shop.itemPriceAndStock;
            int currency = shop.currency;
            var animations = this.Helper.Reflection.GetField<List<TemporaryAnimatedSprite>>(shop, "animations").GetValue();
            var poof = this.Helper.Reflection.GetField<TemporaryAnimatedSprite>(shop, "poof").GetValue();
            var heldItem = shop.heldItem;
            int currentItemIndex = shop.currentItemIndex;
            var scrollBar = shop.scrollBar;
            var scrollBarRunner = this.Helper.Reflection.GetField<Rectangle>(shop, "scrollBarRunner").GetValue();
            ISalable hover = null;

            if (!Game1.options.showMenuBackground)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            }

            Texture2D purchaseTexture = Game1.mouseCursors;
            Rectangle purchaseWindowBorder = new Rectangle(384, 373, 18, 18);
            Rectangle purchaseItemRect = new Rectangle(384, 396, 15, 15);
            int purchaseItemTextColor = -1;
            Color purchaseSelectedColor = Color.Wheat;
            if (shop.storeContext == "QiGemShop")
            {
                purchaseTexture = Game1.mouseCursors2;
                purchaseWindowBorder = new Rectangle(0, 256, 18, 18);
                purchaseItemRect = new Rectangle(18, 256, 15, 15);
                purchaseItemTextColor = 4;
                purchaseSelectedColor = Color.Blue;
            }

            //IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), shop.xPositionOnScreen, shop.yPositionOnScreen, shop.width, shop.height - 256 + 32 + 4, Color.White, 4f, true);
            IClickableMenu.drawTextureBox(b, purchaseTexture, purchaseWindowBorder, shop.xPositionOnScreen, shop.yPositionOnScreen, shop.width, shop.height - 256 + 32 + 4, Color.White, 4f);
            for (int i = currentItemIndex * UnitsWide; i < forSale.Count && i < currentItemIndex * UnitsWide + UnitsWide * 3; ++i)
            {
                bool failedCanPurchaseCheck = shop.canPurchaseCheck != null && !shop.canPurchaseCheck(i);
                int ix = i % UnitsWide;
                int iy = i / UnitsWide;
                Rectangle rect = new Rectangle(shop.xPositionOnScreen + 16 + ix * UnitWidth, shop.yPositionOnScreen + 16 + iy * UnitHeight - currentItemIndex * UnitHeight, UnitWidth, UnitHeight);
                IClickableMenu.drawTextureBox(b, purchaseTexture, purchaseItemRect, rect.X, rect.Y, rect.Width, rect.Height, rect.Contains(Game1.getOldMouseX(true), Game1.getOldMouseY(true)) ? purchaseSelectedColor : Color.White, 4f, false);
                ISalable item = forSale[i];
                StackDrawType stackDrawType;
                if (shop.storeContext == "QiGemShop")
                    stackDrawType = StackDrawType.HideButShowQuality;
                else if (shop.itemPriceAndStock[item][1] == int.MaxValue)
                    stackDrawType = StackDrawType.HideButShowQuality;
                else
                {
                    stackDrawType = StackDrawType.Draw_OneInclusive;
                    if (this.Helper.Reflection.GetField<bool>(shop, "_isStorageShop").GetValue())
                        stackDrawType = StackDrawType.Draw;
                }
                if (forSale[i].ShouldDrawIcon())
                {
                    item.drawInMenu(b, new Vector2(rect.X + 48, rect.Y + 16), 1f, 1, 1, stackDrawType, Color.White, true);
                }
                int price = itemPriceAndStock[forSale[i]][0];
                string priceStr = price.ToString();
                if (price > 0)
                {
                    SpriteText.drawString(b,
                                          priceStr,
                                          rect.Right - SpriteText.getWidthOfString(priceStr) - 16,
                                          rect.Y + 80,
                                          alpha: ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) >= price && !failedCanPurchaseCheck ? 1f : 0.5f,
                                          color: purchaseItemTextColor);
                    //Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(rect.Right - 16, rect.Y + 80), new Rectangle(193 + currency * 9, 373, 9, 10), Color.White, 0, Vector2.Zero, 1, layerDepth: 1);
                }
                else if (itemPriceAndStock[forSale[i]].Length > 2)
                {
                    int requiredItemCount = 5;
                    int requiredItem = itemPriceAndStock[forSale[i]][2];
                    if (itemPriceAndStock[forSale[i]].Length > 3)
                    {
                        requiredItemCount = itemPriceAndStock[forSale[i]][3];
                    }
                    bool hasEnoughToTrade = Game1.player.hasItemInInventory(requiredItem, requiredItemCount);
                    if (shop.canPurchaseCheck != null && !shop.canPurchaseCheck(i))
                    {
                        hasEnoughToTrade = false;
                    }
                    float textWidth = SpriteText.getWidthOfString("x" + requiredItemCount);
                    Utility.drawWithShadow(b, Game1.objectSpriteSheet, new Vector2(rect.Right - 64 - textWidth, rect.Y + 80 - 4), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, requiredItem, 16, 16), Color.White * (hasEnoughToTrade ? 1f : 0.25f), 0f, Vector2.Zero, 3, flipped: false, -1f, -1, -1, hasEnoughToTrade ? 0.35f : 0f);
                    SpriteText.drawString(b, "x" + requiredItemCount, rect.Right - (int)textWidth - 16, rect.Y + 80, 999999, -1, 999999, hasEnoughToTrade ? 1f : 0.5f, 0.88f, junimoText: false, -1, "", purchaseItemTextColor);
                }
                if (rect.Contains(Game1.getOldMouseX(true), Game1.getOldMouseY(true)))
                    hover = forSale[i];
            }
            if (forSale.Count == 0)
                SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583"), shop.xPositionOnScreen + shop.width / 2 - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583")) / 2, shop.yPositionOnScreen + shop.height / 2 - 128);

            shop.drawCurrency(b);
            //if (currency == 0)
            //    Game1.dayTimeMoneyBox.drawMoneyBox(b, shop.xPositionOnScreen - 36, shop.yPositionOnScreen + shop.height - shop.inventory.height + 48);

            // background for the inventory menu
            // support bigger backpack
            IClickableMenu.drawTextureBox(b,
                                          Game1.mouseCursors,
                                          purchaseWindowBorder,
                                          shop.xPositionOnScreen + shop.width - shop.inventory.width - 32 - 24,
                                          shop.yPositionOnScreen + shop.height - 256 + 40,
                                          shop.inventory.width + 56,
                                          shop.height - 448 + 20 + (shop.inventory.capacity > 36 ? 64 : 0),
                                          Color.White, 4f, true);

            shop.inventory.draw(b);

            for (int index = animations.Count - 1; index >= 0; --index)
            {
                if (animations[index].update(Game1.currentGameTime))
                    animations.RemoveAt(index);
                else
                    animations[index].draw(b, true);
            }
            poof?.draw(b);

            for (int i = 0; i < shop.tabButtons.Count; i++)
            {
                shop.tabButtons[i].draw(b);
            }

            shop.upperRightCloseButton.draw(b);
            shop.upArrow.draw(b);
            shop.downArrow.draw(b);
            if (forSale.Count > (UnitsWide*UnitsHigh))
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f);
                scrollBar.draw(b);
            }

            int portrait_draw_position = shop.xPositionOnScreen - 320;
            if (portrait_draw_position > 0 && Game1.options.showMerchantPortraits)
            {
                if (shop.portraitPerson != null)
                {
                    Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(portrait_draw_position, shop.yPositionOnScreen), new Rectangle(603, 414, 74, 74), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.91f);
                    if (shop.portraitPerson.Portrait != null)
                    {
                        b.Draw(shop.portraitPerson.Portrait, new Vector2(portrait_draw_position + 20, shop.yPositionOnScreen + 20), new Rectangle(0, 0, 64, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.92f);
                    }
                }
                if (shop.potraitPersonDialogue != null)
                {
                    portrait_draw_position = shop.xPositionOnScreen - (int)Game1.dialogueFont.MeasureString(shop.potraitPersonDialogue).X - 64;
                    if (portrait_draw_position > 0)
                    {
                        IClickableMenu.drawHoverText(b, shop.potraitPersonDialogue, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, portrait_draw_position, shop.yPositionOnScreen + ((shop.portraitPerson != null) ? 312 : 0));
                    }
                }
            }

            this.DrawNewFields(b);// we want hover text to cover our new fields

            if (hover != null)
            {
                // get hover price & stock
                if (itemPriceAndStock == null || !itemPriceAndStock.TryGetValue(hover, out int[] hoverPriceAndStock))
                    hoverPriceAndStock = null;

                // render tooltip
                string hoverText = hover.getDescription();
                string boldTitleText = hover.DisplayName;
                int hoverPrice = hoverPriceAndStock?[0] ?? hover.salePrice();
                int getHoveredItemExtraItemIndex = -1;
                if (hoverPriceAndStock?.Length > 2)
                    getHoveredItemExtraItemIndex = hoverPriceAndStock[2];
                int getHoveredItemExtraItemAmount = 5;
                if (hoverPriceAndStock?.Length > 3)
                    getHoveredItemExtraItemAmount = hoverPriceAndStock[3];
                IClickableMenu.drawToolTip(b, hoverText, boldTitleText, hover as Item, heldItem != null, -1, currency, getHoveredItemExtraItemIndex, getHoveredItemExtraItemAmount, null, hoverPrice);
            }

            heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX(true) + 8, Game1.getOldMouseY(true) + 8), 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, true);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this.Shop == null)
                return;

            if (e.Button is SButton.MouseLeft or SButton.MouseRight)
            {
                var uiCursor = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
                int x = (int)uiCursor.X;
                int y = (int)uiCursor.Y;
                int direction = e.Button == SButton.MouseLeft ? 1 : -1;

                if (new Rectangle(this.Shop.xPositionOnScreen + 25, this.Shop.yPositionOnScreen + 525, 200, 72).Contains(x, y))
                    this.ChangeCategory(direction);
                else if (new Rectangle(this.Shop.xPositionOnScreen + 25, this.Shop.yPositionOnScreen + 600, 200, 48).Contains(x, y))
                    this.ChangeSorting(direction);
                else if (Mod.Config.ExperimentalGridLayout)
                {
                    Point pt = new Point(x, y);
                    if (e.Button == SButton.MouseRight)
                    {
                        //this.Helper.Input.Suppress(e.Button);
                        this.DoGridLayoutRightClick(e, pt);
                    }
                    else
                    {
                        this.Helper.Input.Suppress(e.Button);
                        this.DoGridLayoutLeftClick(e, pt);
                    }
                }
            }
            else if ((e.Button is (>= SButton.A and <= SButton.Z) or SButton.Space or SButton.Back) && this.Search.Selected)
            {
                this.Helper.Input.Suppress(e.Button);
                this.SyncStock();
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (this.Shop != null)
            {
                if (this.FirstTick)
                    this.InitShop2();

                bool oldMode = Game1.uiMode;
                Game1.uiMode = true;
                this.Search.Update();
                Game1.uiMode = oldMode;
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShopMenu shopMenu)
            {
                Log.Trace("Found shop menu!");
                this.InitShop(shopMenu);
            }
            else
            {
                if (this.Shop != null)
                {
                    this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
                    this.Helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
                    this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
                    this.Helper.Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;

                    this.Shop = null;
                    this.CropData = null;

                    if (this.Search != null)
                    {
                        this.Search.Selected = false;
                        this.Search = null;
                    }
                }
            }
        }

        private void DoScroll(int direction)
        {
            var forSale = this.Shop.forSale;
            int currentItemIndex = this.Shop.currentItemIndex;
            var scrollBar = this.Shop.scrollBar;
            var scrollBarRunner = this.Helper.Reflection.GetField<Rectangle>(this.Shop, "scrollBarRunner").GetValue();
            var downArrow = this.Shop.downArrow;
            var upArrow = this.Shop.upArrow;
            int rows = (forSale.Count / UnitsWide);
            if ((forSale.Count % UnitsWide) != 0)
                rows++;
            int rowsH = rows - UnitsHigh;//this may go negative. thats okay.

            if (direction < 0)
            {
                if (currentItemIndex < rowsH)
                {
                    downArrow.scale = downArrow.baseScale;
                    this.Shop.currentItemIndex = currentItemIndex += 1;
                    if (forSale.Count > 0)
                    {
                        scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, rowsH) * currentItemIndex + upArrow.bounds.Bottom + 4;
                        if (currentItemIndex >= rowsH)
                        {
                            scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
                        }
                    }
                    Game1.playSound("shwip");
                }
            }
            else if (direction > 0)
            {
                if (currentItemIndex > 0)
                {
                    upArrow.scale = upArrow.baseScale;
                    this.Shop.currentItemIndex = currentItemIndex -= 1;
                    if (forSale.Count > 0)
                    {
                        scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, rowsH) * currentItemIndex + upArrow.bounds.Bottom + 4;
                        if (currentItemIndex >= rowsH)
                        {
                            scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
                        }
                    }
                    Game1.playSound("shwip");
                }
            }
        }

        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if ((this.Shop == null) || !Mod.Config.ExperimentalGridLayout)
                return;

            this.DoScroll(e.Delta);
        }

        private void DoGridLayoutLeftClick(ButtonPressedEventArgs e, Point pt)
        {
            var forSale = this.Shop.forSale;
            var itemPriceAndStock = this.Shop.itemPriceAndStock;
            int currency = this.Shop.currency;
            var heldItem = this.Shop.heldItem;
            int currentItemIndex = this.Shop.currentItemIndex;
            var animations = this.Helper.Reflection.GetField<List<TemporaryAnimatedSprite>>(this.Shop, "animations").GetValue();
            float sellPercentage = this.Helper.Reflection.GetField<float>(this.Shop, "sellPercentage").GetValue();
            var scrollBarRunner = this.Helper.Reflection.GetField<Rectangle>(this.Shop, "scrollBarRunner").GetValue();
            var scrollBar = this.Shop.scrollBar;
            var downArrow = this.Shop.downArrow;
            var upArrow = this.Shop.upArrow;
            int rows = (forSale.Count / UnitsWide);
            if ((forSale.Count % UnitsWide) != 0)
                rows++;

            //var uiCursor = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
            //int x = (int)uiCursor.X;
            //int y = (int)uiCursor.Y;
            int x = pt.X;
            int y = pt.Y;

            if (this.Shop.upperRightCloseButton.containsPoint(x, y))
            {
                this.Shop.exitThisMenu();
                return;
            }
            else if (downArrow.containsPoint(x, y))
            {
                this.DoScroll(-1);
                return;
            }
            else if (upArrow.containsPoint(x, y))
            {
                this.DoScroll(1);
                return;
            }
            else if (scrollBarRunner.Contains(x, y))
            {
                int y1 = scrollBar.bounds.Y;
                scrollBar.bounds.Y = Math.Min(this.Shop.yPositionOnScreen + this.Shop.height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, this.Shop.yPositionOnScreen + upArrow.bounds.Height + 20));
                currentItemIndex = (int)Math.Round((double)Math.Max(1, rows-UnitsHigh) * ((y - scrollBarRunner.Y) / (float)scrollBarRunner.Height));
                this.Shop.currentItemIndex = currentItemIndex;
                if (forSale.Count > 0)
                {
                    scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, rows-UnitsHigh) * currentItemIndex + upArrow.bounds.Bottom + 4;
                    if (currentItemIndex >= rows-UnitsHigh)
                    {
                        scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
                    }
                }
                int y2 = scrollBar.bounds.Y;
                if (y1 == y2)
                    return;
                Game1.playSound("shiny4");
                return;
            }

            Vector2 clickableComponent = this.Shop.inventory.snapToClickableComponent(x, y);
            if (heldItem == null)
            {
                Item item = this.Shop.inventory.leftClick(x, y, null, false);
                if (item != null)
                {
                    if (this.Shop.onSell != null)
                    {
                        this.Shop.onSell(item);
                    }
                    else
                    {
                        ShopMenu.chargePlayer(Game1.player, currency, -((item is SObject obj ? (int)(obj.sellToStorePrice() * (double)sellPercentage) : (int)(item.salePrice() / 2 * (double)sellPercentage)) * item.Stack));
                        int num = item.Stack / 8 + 2;
                        for (int index = 0; index < num; ++index)
                        {
                            animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, clickableComponent + new Vector2(32f, 32f), false, false)
                            {
                                alphaFade = 0.025f,
                                motion = new Vector2(Game1.random.Next(-3, 4), -4f),
                                acceleration = new Vector2(0.0f, 0.5f),
                                delayBeforeAnimationStart = index * 25,
                                scale = 2f
                            });
                            animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, clickableComponent + new Vector2(32f, 32f), false, false)
                            {
                                scale = 4f,
                                alphaFade = 0.025f,
                                delayBeforeAnimationStart = index * 50,
                                motion = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), new Vector2(this.Shop.xPositionOnScreen - 36, this.Shop.yPositionOnScreen + this.Shop.height - this.Shop.inventory.height - 16), 8f),
                                acceleration = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), new Vector2(this.Shop.xPositionOnScreen - 36, this.Shop.yPositionOnScreen + this.Shop.height - this.Shop.inventory.height - 16), 0.5f)
                            });
                        }
                        if (item is SObject o && o.Edibility != -300)
                        {
                            Item one = item.getOne();
                            one.Stack = item.Stack;
                            (Game1.getLocationFromName("SeedShop") as StardewValley.Locations.SeedShop).itemsToStartSellingTomorrow.Add(one);
                        }
                        Game1.playSound("sell");
                        Game1.playSound("purchase");
                    }
                }
            }
            else
            {
                heldItem = this.Shop.inventory.leftClick(x, y, (Item)heldItem);
                this.Shop.heldItem = heldItem;
            }

            for (int i = currentItemIndex * UnitsWide; i < forSale.Count && i < currentItemIndex * UnitsWide + UnitsWide * 3; ++i)
            {
                int ix = i % UnitsWide;
                int iy = i / UnitsWide;
                Rectangle rect = new Rectangle(this.Shop.xPositionOnScreen + 16 + ix * UnitWidth, this.Shop.yPositionOnScreen + 16 + iy * UnitHeight - currentItemIndex * UnitHeight, UnitWidth, UnitHeight);
                if (rect.Contains(x, y) && forSale[i] != null)
                {
                    int numberToBuy = (!e.IsDown(SButton.LeftShift) ? 1 : Math.Min(Math.Min(e.IsDown(SButton.LeftControl) ? 25 : 5, ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) / Math.Max(1, itemPriceAndStock[forSale[i]][0])), Math.Max(1, itemPriceAndStock[forSale[i]][1])));
                    numberToBuy = Math.Min(numberToBuy, forSale[i].maximumStackSize());
                    //if (numberToBuy == -1)
                    //    numberToBuy = 1;

                    var tryToPurchaseItem = this.Helper.Reflection.GetMethod(this.Shop, "tryToPurchaseItem");
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
                    if (heldItem != null && Game1.options.SnappyMenus && Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool((Item)heldItem))
                    {
                        heldItem = null;
                        this.Shop.heldItem = heldItem;
                        DelayedAction.playSoundAfterDelay("coin", 100);
                    }
                }
            }
        }

        // the mouse state is always released by this point. even if the mouse is held down.
        // it's the input supression that does this.
        // causes an immediate mouse up when you suppress a mouse down.
        // if we suppress ShopMenu.receiveRightClick via Harmony then we can get the right clicks
        // while still suppressing the left clicks.
        //if (this.Helper.Input.IsDown(SButton.X))
        //     doing this pressing/holding X while right clicking can get the same hold repeat purchase. no Harmony suppress needed.
        //     in this alternate we can still suppress the right click. right click starts it and X takes over for the repeat.
        //     X is the keyboard equiv of right click.
        private void MyDelayFunc()
        {
            if (this.Shop != null)
            {
                //if (this.Helper.Input.IsDown(SButton.X))
                if (Game1.input.GetMouseState().RightButton == ButtonState.Pressed)
                {
                    this.DoGridLayoutRightClick(null, this.PurchasePoint);
                }
            }
        }

        private void DoGridLayoutRightClick(ButtonPressedEventArgs e, Point pt)
        {
            var forSale = this.Shop.forSale;
            var itemPriceAndStock = this.Shop.itemPriceAndStock;
            int currency = this.Shop.currency;
            var animations = this.Helper.Reflection.GetField<List<TemporaryAnimatedSprite>>(this.Shop, "animations").GetValue();
            var heldItem = this.Shop.heldItem;
            int currentItemIndex = this.Shop.currentItemIndex;
            float sellPercentage = this.Helper.Reflection.GetField<float>(this.Shop, "sellPercentage").GetValue();

            //var uiCursor = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
            //int x = (int)uiCursor.X;
            //int y = (int)uiCursor.Y;
            int x = pt.X;
            int y = pt.Y;
            this.PurchasePoint = pt;

            // Copying a lot from right click code
            Vector2 clickableComponent = this.Shop.inventory.snapToClickableComponent(x, y);
            if (heldItem == null)
            {
                Item item = this.Shop.inventory.rightClick(x, y, null, false);
                if (item != null)
                {
                    if (this.Shop.onSell != null)
                    {
                        this.Shop.onSell(item);
                    }
                    else
                    {
                        ShopMenu.chargePlayer(Game1.player, currency, -((item is SObject obj ? (int)(obj.sellToStorePrice() * (double)sellPercentage) : (int)(item.salePrice() / 2 * (double)sellPercentage)) * item.Stack));
                        Game1.playSound(Game1.mouseClickPolling > 300 ? "purchaseRepeat" : "purchaseClick");
                        animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 64, 256, 64, 64), 9999f, 1, 999, clickableComponent + new Vector2(32f, 32f), false, false)
                        {
                            alphaFade = 0.025f,
                            motion = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), Game1.dayTimeMoneyBox.position + new Vector2(96f, 196f), 12f),
                            acceleration = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), Game1.dayTimeMoneyBox.position + new Vector2(96f, 196f), 0.5f)
                        });
                        if (item is SObject o && o.Edibility != -300)
                        {
                            (Game1.getLocationFromName("SeedShop") as StardewValley.Locations.SeedShop).itemsToStartSellingTomorrow.Add(item.getOne());
                        }
                        if (this.Shop.inventory.getItemAt(x, y) == null)
                        {
                            Game1.playSound("sell");
                            animations.Add(new TemporaryAnimatedSprite(5, clickableComponent + new Vector2(32f, 32f), Color.White)
                            {
                                motion = new Vector2(0.0f, -0.5f)
                            });
                        }
                    }
                }
            }
            else
            {
                heldItem = this.Shop.inventory.leftClick(x, y, (Item)heldItem);
                this.Shop.heldItem = heldItem;
            }

            for (int i = currentItemIndex * UnitsWide; i < forSale.Count && i < currentItemIndex * UnitsWide + UnitsWide * 3; ++i)
            {
                int ix = i % UnitsWide;
                int iy = i / UnitsWide;
                Rectangle rect = new Rectangle(this.Shop.xPositionOnScreen + 16 + ix * UnitWidth, this.Shop.yPositionOnScreen + 16 + iy * UnitHeight - currentItemIndex * UnitHeight, UnitWidth, UnitHeight);
                if (rect.Contains(x, y) && forSale[i] != null)
                {
                    bool leftShiftDown = e != null ? e.IsDown(SButton.LeftShift) : this.Helper.Input.IsDown(SButton.LeftShift);
                    bool leftCtrlDown = e != null ? e.IsDown(SButton.LeftControl) : this.Helper.Input.IsDown(SButton.LeftControl);
                    int numberToBuy = (!leftShiftDown ? 1 : Math.Min(Math.Min(leftCtrlDown ? 25 : 5, ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) / Math.Max(1, itemPriceAndStock[forSale[i]][0])), Math.Max(1, itemPriceAndStock[forSale[i]][1])));
                    numberToBuy = Math.Min(numberToBuy, forSale[i].maximumStackSize());

                    var tryToPurchaseItem = this.Helper.Reflection.GetMethod(this.Shop, "tryToPurchaseItem");
                    if (numberToBuy > 0 && tryToPurchaseItem.Invoke<bool>(forSale[i], heldItem, numberToBuy, x, y, i))
                    {
                        itemPriceAndStock.Remove(forSale[i]);
                        forSale.RemoveAt(i);
                    }
                    if (heldItem == null || !Game1.options.SnappyMenus || Game1.activeClickableMenu is not ShopMenu || !Game1.player.addItemToInventoryBool((Item)heldItem))
                    {
                        StardewValley.DelayedAction.functionAfterDelay(this.MyDelayFunc, 150);
                        break;
                    }

                    heldItem = null;
                    this.Shop.heldItem = heldItem;
                    DelayedAction.playSoundAfterDelay("coin", 100);
                    break;
                }
            }
        }
    }

    internal class ShopMenuPatches
    {
        public static bool ShopMenu_receiveScrollWheelAction_Prefix(StardewValley.Object __instance, int direction)
        {
            try
            {
                if (Mod.Config.ExperimentalGridLayout)
                    return false; // don't run original logic

                return true;
            }
            catch (Exception ex)
            {
                Mod.Instance.Monitor.Log($"Failed in {nameof(ShopMenu_receiveScrollWheelAction_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool ShopMenu_receiveRightClick_Prefix(StardewValley.Object __instance, int x, int y, bool playSound = true)
        {
            try
            {
                if (Mod.Config.ExperimentalGridLayout)
                    return false; // don't run original logic

                return true;
            }
            catch (Exception ex)
            {
                Mod.Instance.Monitor.Log($"Failed in {nameof(ShopMenu_receiveRightClick_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool ShopMenu_draw_Prefix(StardewValley.Object __instance, SpriteBatch b)
        {
            try
            {
                if (Mod.Config.ExperimentalGridLayout)
                    return false; // don't run original logic

                return true;
            }
            catch (Exception ex)
            {
                Mod.Instance.Monitor.Log($"Failed in {nameof(ShopMenu_draw_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

    }
}

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
using Pathoschild.Stardew.ChestsAnywhere;

namespace BetterShopMenu
{
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static Configuration Config;
        internal bool ChestsAnywhereActive;
        internal IChestsAnywhereApi ChestsAnywhereApi;

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);

            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();

            this.GridClickableButton = new ClickableTextureComponent(Rectangle.Empty,
                                                                     helper.ModContent.Load<Texture2D>("assets/buttonGrid.png"),
                                                                     new Rectangle(0, 0, 16, 16),
                                                                     4f);
            this.LinearClickableButton = new ClickableTextureComponent(Rectangle.Empty,
                                                                       helper.ModContent.Load<Texture2D>("assets/buttonStd.png"),
                                                                       new Rectangle(0, 0, 16, 16),
                                                                       4f);
            this.GridLayoutActive = Config.GridLayout;
            this.ActiveButton = (this.GridLayoutActive ? this.LinearClickableButton : this.GridClickableButton);

            this.Quantity_OKButton = null;
            this.Quantity_TextBox = null;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            //helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            //helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
            //helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            //helper.Events.Input.ButtonReleased += this.OnButtonReleased;
            //helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;

            var harmony = new Harmony(this.ModManifest.UniqueID);
            System.Reflection.MethodInfo mInfo;

            // these patches only patch the source method out when the grid layout is enabled

            // this patches out the ShopMenu mouse wheel code.
            mInfo = harmony.Patch(original: AccessTools.Method(typeof(StardewValley.Menus.ShopMenu), nameof(StardewValley.Menus.ShopMenu.receiveScrollWheelAction)),
                                  prefix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(ShopMenuPatches.ShopMenu_receiveScrollWheelAction_Prefix))
                                 );

            // this patches the ShopMenu performHoverAction code.
            // we block the ShopMenu code from the grid layout area. otherwise we allow it. e.g. inventory menu.
            mInfo = harmony.Patch(original: AccessTools.Method(typeof(StardewValley.Menus.ShopMenu), nameof(StardewValley.Menus.ShopMenu.performHoverAction)),
                                  prefix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(ShopMenuPatches.ShopMenu_performHoverAction_Prefix))
                                 );

            // this patches out the ShopMenu mouse right click code.
            // this allows us to trigger a delay for doing the right click, hold auto purchase.
            // SMAPI input supression (preferred) gets in the way of detecting a hold.
            mInfo = harmony.Patch(original: AccessTools.Method(typeof(StardewValley.Menus.ShopMenu), nameof(StardewValley.Menus.ShopMenu.receiveRightClick)),
                                  prefix: new HarmonyMethod(typeof(ShopMenuPatches), nameof(ShopMenuPatches.ShopMenu_receiveRightClick_Prefix))
                                 );

            // this patches out ShopMenu.draw.
            // excluding the grid layout draw, our draw procedure is really just a copy of the Stardew ShopMenu.draw code.
            System.Type[] drawParams = new System.Type[] { typeof(SpriteBatch) };
            mInfo = harmony.Patch(original: AccessTools.Method(typeof(StardewValley.Menus.ShopMenu), nameof(StardewValley.Menus.ShopMenu.draw), drawParams),
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
                    getValue: () => Mod.Config.GridLayout,
                    setValue: value => Mod.Config.GridLayout = value);
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: I18n.Config_QuantityDialog_Name,
                    tooltip: I18n.Config_QuantityDialog_Tooltip,
                    getValue: () => Mod.Config.QuantityDialog,
                    setValue: value => Mod.Config.QuantityDialog = value
                );
            }

            this.ChestsAnywhereActive = false;
            this.ChestsAnywhereApi = this.Helper.ModRegistry.GetApi<IChestsAnywhereApi>("Pathoschild.ChestsAnywhere");
        }

        internal ShopMenu Shop;
        private bool FirstTick;
        private List<ISalable> InitialItems;
        private Dictionary<ISalable, int[]> InitialStock;
        private List<int> Categories;
        private int CurrCategory;
        private bool HasRecipes;
        private Dictionary<int, string> CategoryNames;
        private int Sorting;
        private TextBox Search;

        public const int UnitWidth = 170;
        public const int UnitHeight = 144;
        public const int UnitsHigh = 3;
        public const int UnitsWide = 6;//(Shop.width - 32) / UnitWidth

        private bool HaveStockList;
        private Dictionary<int, string> CropData;
        private const int SeedsOtherCategory = -174; //seeds - 100;

        private Point PurchasePoint;
        private bool RightClickDown;
        private int Purchase_Countdown;
        private const int Purchase_CountdownStart = 60 * 600 / 1000;//600ms
        private const int Purchase_CountdownRepeat = 60 * 100 / 1000;//100ms

        internal ClickableTextureComponent LinearClickableButton;
        internal ClickableTextureComponent GridClickableButton;
        internal ClickableTextureComponent ActiveButton;
        internal bool GridLayoutActive;

        internal ClickableTextureComponent Quantity_OKButton;
        internal TextBox Quantity_TextBox;
        internal int QuantityIndex;

        IReflectedField<Rectangle> Reflect_scrollBarRunner;
        IReflectedField<List<TemporaryAnimatedSprite>> Reflect_animations;
        IReflectedField<TemporaryAnimatedSprite> Reflect_poof;
        IReflectedField<bool> Reflect_isStorageShop;
        IReflectedField<float> Reflect_sellPercentage;
        IReflectedField<string> Reflect_hoverText;
        IReflectedField<string> Reflect_boldTitleText;
        IReflectedField<int> Reflect_hoverPrice;
        IReflectedMethod Reflect_tryToPurchaseItem;

        private void InitShop(ShopMenu shopMenu)
        {
            this.Shop = shopMenu;
            this.FirstTick = true;

            this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this.Helper.Events.Input.ButtonReleased += this.OnButtonReleased;
            this.Helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;

            this.Reflect_scrollBarRunner = this.Helper.Reflection.GetField<Rectangle>(shopMenu, "scrollBarRunner");
            this.Reflect_animations = this.Helper.Reflection.GetField<List<TemporaryAnimatedSprite>>(shopMenu, "animations");
            this.Reflect_poof = this.Helper.Reflection.GetField<TemporaryAnimatedSprite>(shopMenu, "poof");
            this.Reflect_isStorageShop = this.Helper.Reflection.GetField<bool>(shopMenu, "_isStorageShop");
            this.Reflect_sellPercentage = this.Helper.Reflection.GetField<float>(shopMenu, "sellPercentage");
            this.Reflect_hoverText = this.Helper.Reflection.GetField<string>(shopMenu, "hoverText");
            this.Reflect_boldTitleText = this.Helper.Reflection.GetField<string>(shopMenu, "boldTitleText");
            this.Reflect_hoverPrice = this.Helper.Reflection.GetField<int>(shopMenu, "hoverPrice");
            this.Reflect_tryToPurchaseItem = this.Helper.Reflection.GetMethod(shopMenu, "tryToPurchaseItem");

            Rectangle bounds = new Rectangle(shopMenu.xPositionOnScreen - 48, shopMenu.yPositionOnScreen + 530, 64, 64);
            this.LinearClickableButton.bounds = bounds;
            this.GridClickableButton.bounds = bounds;

            this.ChestsAnywhereActive = (this.ChestsAnywhereApi != null) && this.ChestsAnywhereApi.IsOverlayActive();

            this.Quantity_OKButton = null;
            this.Quantity_TextBox = null;
            this.QuantityIndex = -1;
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

            this.RightClickDown = false;
            this.Purchase_Countdown = -1;

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
                    return split[1].Contains(Game1.currentSeason, StringComparison.OrdinalIgnoreCase) == inSeason;
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

            foreach (var item in this.InitialItems)
            {
                if (this.ItemMatchesCategory(item, curCat) &&
                    (this.Search.Text == null || item.DisplayName.Contains(this.Search.Text, StringComparison.CurrentCultureIgnoreCase)))
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
                if (this.ItemMatchesCategory(item.Key, curCat) &&
                    (this.Search.Text == null || item.Key.DisplayName.Contains(this.Search.Text, StringComparison.CurrentCultureIgnoreCase)))
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
            if (this.Shop != null)
            {
                //if (Game1.activeClickableMenu != this.Shop)
                //{
                //    Log.Debug($"OnRenderedActiveMenu Game1.activeClickableMenu != shop. {Game1.activeClickableMenu}");
                //    return;
                //}

                bool background = false;
                if (this.ChestsAnywhereActive && this.ChestsAnywhereApi.IsOverlayModal())
                    background = true;

                if (this.GridLayoutActive)
                    this.DrawGridLayout(e.SpriteBatch, background);
                else
                    this.DrawNewFields(e.SpriteBatch);

                if (this.Quantity_TextBox != null)
                {
                    this.Quantity_TextBox.Draw(e.SpriteBatch);
                    this.Quantity_OKButton.draw(e.SpriteBatch);
                }

                this.Shop.drawMouse(e.SpriteBatch);
            }
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

            this.ActiveButton.draw(b);
            if (this.ActiveButton.bounds.Contains(Game1.getOldMouseX(true), Game1.getOldMouseY(true)))
            {
                IClickableMenu.drawHoverText(b,
                                             this.GridLayoutActive ? I18n.Button_StdLayout_Tooltip() : I18n.Button_GridLayout_Tooltip(),
                                             Game1.smallFont);
            }
        }

        private void DrawGridLayout(SpriteBatch b, bool background)
        {
            var shop = this.Shop;
            var forSale = shop.forSale;
            var itemPriceAndStock = shop.itemPriceAndStock;
            int currency = shop.currency;
            var animations = this.Reflect_animations.GetValue();
            var poof = this.Reflect_poof.GetValue();
            var heldItem = shop.heldItem;
            int currentItemIndex = shop.currentItemIndex;
            var scrollBar = shop.scrollBar;
            var scrollBarRunner = this.Reflect_scrollBarRunner.GetValue();
            ISalable hover = null;

            if (!Game1.options.showMenuBackground)
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

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
                
                Rectangle rect = new Rectangle(shop.xPositionOnScreen + 16 + ix * UnitWidth,
                                               shop.yPositionOnScreen + 16 + iy * UnitHeight - currentItemIndex * UnitHeight,
                                               UnitWidth, UnitHeight);
                bool selectedItem = rect.Contains(Game1.getOldMouseX(true), Game1.getOldMouseY(true));
                IClickableMenu.drawTextureBox(b, purchaseTexture, purchaseItemRect, rect.X, rect.Y, rect.Width, rect.Height, selectedItem ? purchaseSelectedColor : Color.White, 4f, false);

                ISalable item = forSale[i];
                if (selectedItem)
                    hover = item;

                StackDrawType stackDrawType;
                if (shop.storeContext == "QiGemShop")
                    stackDrawType = StackDrawType.HideButShowQuality;
                else if (shop.itemPriceAndStock[item][1] == int.MaxValue)
                    stackDrawType = StackDrawType.HideButShowQuality;
                else
                {
                    stackDrawType = StackDrawType.Draw_OneInclusive;
                    if (this.Reflect_isStorageShop.GetValue())
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
                    if (price < 1000000)
                    {
                        SpriteText.drawString(b,
                                              priceStr,
                                              rect.X + ((rect.Width - SpriteText.getWidthOfString(priceStr)) / 2),//rect.Right - SpriteText.getWidthOfString(priceStr) - 16,
                                              rect.Y + 80,
                                              alpha: ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) >= price && !failedCanPurchaseCheck ? 1f : 0.5f,
                                              color: purchaseItemTextColor);
                    }
                    else
                    {
                        // SpriteText font is too big/long. this is about all I can do. we lose the alpha ability.
                        SpriteFont font = Game1.dialogueFont;

                        Utility.drawTextWithShadow(b,
                                                   priceStr,
                                                   font,
                                                   new Vector2(rect.X + ((rect.Width - font.MeasureString(priceStr).X) / 2), rect.Y + 80),
                                                   purchaseItemTextColor == -1 ? new Color(86, 22, 12) : Color.White);
                        //Utility.drawBoldText(b,
                        //                     priceStr,
                        //                     font,
                        //                     new Vector2(rect.X + ((rect.Width - font.MeasureString(priceStr).X) / 2), rect.Y + 80),
                        //                     purchaseItemTextColor == -1 ? new Color(86, 22, 12) : Color.White);
                    }
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
                    Utility.drawWithShadow(b,
                                           Game1.objectSpriteSheet,
                                           new Vector2(rect.Right - 64 - textWidth, rect.Y + 80 - 4),
                                           Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, requiredItem, 16, 16),
                                           Color.White * (hasEnoughToTrade ? 1f : 0.25f),
                                           0f,
                                           Vector2.Zero,
                                           3, flipped: false,
                                           -1f, -1, -1,
                                           hasEnoughToTrade ? 0.35f : 0f);
                    SpriteText.drawString(b,
                                          "x" + requiredItemCount,
                                          rect.Right - (int)textWidth - 16, rect.Y + 80, 999999,
                                          -1, 999999, hasEnoughToTrade ? 1f : 0.5f, 0.88f, junimoText: false, -1, "", purchaseItemTextColor);
                }
            }
            if (forSale.Count == 0)
                SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583"), shop.xPositionOnScreen + shop.width / 2 - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583")) / 2, shop.yPositionOnScreen + shop.height / 2 - 128);

            shop.drawCurrency(b);
            //if (currency == 0)
            //    Game1.dayTimeMoneyBox.drawMoneyBox(b, shop.xPositionOnScreen - 36, shop.yPositionOnScreen + shop.height - shop.inventory.height + 48);

            // background for the inventory menu
            // support the bigger backpack mod
            int biggerPack = (shop.inventory.capacity > 36 ? 64 : 0);
            IClickableMenu.drawTextureBox(b,
                                          Game1.mouseCursors,
                                          new Rectangle(384, 373, 18, 18),
                                          shop.xPositionOnScreen + shop.width - shop.inventory.width - 32 - 24,
                                          shop.yPositionOnScreen + shop.height - 256 + 40,
                                          shop.inventory.width + 56,
                                          shop.height - 448 + 20 + biggerPack,
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

            // dressers, furniture catalog, floor/wallpaper catalog, have tabs
            for (int i = 0; i < shop.tabButtons.Count; i++)
                shop.tabButtons[i].draw(b);

            shop.upperRightCloseButton.draw(b);
            shop.upArrow.draw(b);
            shop.downArrow.draw(b);
            if (forSale.Count > (UnitsWide * UnitsHigh))
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f);
                scrollBar.draw(b);
            }

            int portrait_draw_position = shop.xPositionOnScreen - 320;
            if ((portrait_draw_position > 0) && Game1.options.showMerchantPortraits)
            {
                if (shop.portraitPerson != null)
                {
                    Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(portrait_draw_position, shop.yPositionOnScreen), new Rectangle(603, 414, 74, 74), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.91f);
                    if (shop.portraitPerson.Portrait != null)
                    {
                        b.Draw(shop.portraitPerson.Portrait, new Vector2(portrait_draw_position + 20, shop.yPositionOnScreen + 20), new Rectangle(0, 0, 64, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.92f);
                    }
                }
                if ((shop.potraitPersonDialogue != null) && !background)
                {
                    portrait_draw_position = shop.xPositionOnScreen - (int)Game1.dialogueFont.MeasureString(shop.potraitPersonDialogue).X - 64;
                    if (portrait_draw_position > 0)
                    {
                        IClickableMenu.drawHoverText(b, shop.potraitPersonDialogue, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, portrait_draw_position, shop.yPositionOnScreen + ((shop.portraitPerson != null) ? 312 : 0));
                    }
                }
            }

            this.DrawNewFields(b);// we want hover text to cover our new fields

            if (!background)
            {
                shop.hoveredItem = hover;// lookup anything mod examines the hoveredItem field. maybe others.

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
                else
                {
                    // the inventory may have created some hover text (via ShopMenu.performHoverAction).
                    // typically this is an item that can be sold to the vendor. other times clothing gets a hover.
                    int price = this.Reflect_hoverPrice.GetValue();
                    string hoverText = this.Reflect_hoverText.GetValue();
                    string boldTitleText = this.Reflect_boldTitleText.GetValue();
                    if (!hoverText.Equals(""))
                        IClickableMenu.drawToolTip(b, hoverText, boldTitleText, null,
                                                   currencySymbol: currency,
                                                   moneyAmountToShowAtBottom: price);
                }

                heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX(true) + 8, Game1.getOldMouseY(true) + 8), 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, true);
            }
        }

        private void CloseQuantityDialog(TextBox sender)
        {
            int amount;
            bool ok = int.TryParse(this.Quantity_TextBox.Text, out amount);
            if (amount > 999)
                amount = 999;

            int idx = this.QuantityIndex;

            this.Quantity_TextBox.Selected = false;
            this.Quantity_TextBox = null;
            this.Quantity_OKButton = null;
            this.QuantityIndex = -1;

            //call the purchase code here
            if (ok && (idx >= 0))
                this.PurchaseItem(amount, idx);
        }

        private void CreateQuantityDialog(Vector2 cursorPos)
        {
            int X = (int)cursorPos.X + Game1.tileSize;
            int Y = (int)cursorPos.Y;

            this.Quantity_TextBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor);
            this.Quantity_TextBox.X = X;
            this.Quantity_TextBox.Y = Y;
            int width = this.Quantity_TextBox.Width;

            this.Quantity_OKButton = new ClickableTextureComponent(
                                               new Rectangle(X + width + Game1.pixelZoom, // pixelzoom used to give gap
                                                             Y,
                                                             Game1.tileSize, Game1.tileSize),
                                               Game1.mouseCursors,
                                               Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1),
                                               1f,
                                               false);

            this.Quantity_TextBox.OnEnterPressed += this.CloseQuantityDialog;
            this.Quantity_TextBox.numbersOnly = true;
            this.Quantity_TextBox.SelectMe();
        }

        private bool GetQuantityIndex()
        {
            var shop = this.Shop;
            var forSale = shop.forSale;

            for (int i = 0; i < forSale.Count; i++)
            {
                if (forSale[i] == shop.hoveredItem)
                {
                    this.QuantityIndex = i;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            var shop = this.Shop;

            if (shop == null)
                return;
            else if (Game1.activeClickableMenu != this.Shop)
            {
                //Log.Debug($"OnButtonPressed Game1.activeClickableMenu != shop. {Game1.activeClickableMenu}");
                return;
            }
            else if (this.ChestsAnywhereActive && this.ChestsAnywhereApi.IsOverlayModal())
                return; // Chests Anywhere's options / dropdown view is handling input

            if (e.Button is SButton.MouseLeft or SButton.MouseRight)
            {
                var uiCursor = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
                int x = (int)uiCursor.X;
                int y = (int)uiCursor.Y;
                int direction = e.Button == SButton.MouseLeft ? 1 : -1;

                var categoryRect = new Rectangle(shop.xPositionOnScreen + 25, shop.yPositionOnScreen + 525, 200, 72);
                var sortRect = new Rectangle(shop.xPositionOnScreen + 25, shop.yPositionOnScreen + 600, 200, 48);
                //var menuRect = new Rectangle(shop.xPositionOnScreen, shop.yPositionOnScreen, shop.width, shop.height - 256 + 32 + 4);

                if (categoryRect.Contains(x, y))
                    this.ChangeCategory(direction);
                else if (sortRect.Contains(x, y))
                    this.ChangeSorting(direction);
                else if ((e.Button == SButton.MouseLeft) && this.ActiveButton.bounds.Contains(x, y))
                {
                    this.GridLayoutActive = !this.GridLayoutActive;
                    this.ActiveButton = (this.GridLayoutActive ? this.LinearClickableButton : this.GridClickableButton);
                    this.Shop.currentItemIndex = 0;
                }
                else if ((this.Quantity_OKButton != null) && (e.Button == SButton.MouseLeft) && this.Quantity_OKButton.bounds.Contains(x, y))
                {
                    this.Helper.Input.Suppress(e.Button);
                    this.CloseQuantityDialog(this.Quantity_TextBox);
                }
                else if (
                         Config.QuantityDialog &&
                         (e.Button == SButton.MouseRight) &&
                         e.IsDown(SButton.LeftAlt) &&
                         (shop.hoveredItem != null) &&
                         this.GetQuantityIndex() &&
                         (this.Shop.forSale[this.QuantityIndex].maximumStackSize() > 1)
                        )
                {
                    this.Helper.Input.Suppress(e.Button);
                    this.CreateQuantityDialog(uiCursor);
                }
                else if (this.GridLayoutActive)
                {
                    Point pt = new Point(x, y);
                    if (e.Button == SButton.MouseRight)
                    {
                        // the mouse state is always released if we suppress input via SMAPI.
                        // the supression causes an immediate mouse up when you suppress a mouse down.
                        // this gets in the way of detecting a mouse button down hold. e.g. shop menu repeat purchase feature.
                        //this.Helper.Input.Suppress(e.Button); suppressed via Harmony
                        this.RightClickDown = true;
                        this.DoGridLayoutRightClick(e, pt);
                    }
                    else
                    {
                        this.Helper.Input.Suppress(e.Button);
                        this.DoGridLayoutLeftClick(e, pt);
                    }
                }
            }
            //else if ((e.Button is (>= SButton.A and <= SButton.Z) or SButton.Space or SButton.Back) && this.Search.Selected)
            // sync on any search box input. not just simple ascii A..Z.
            else if (this.Search.Selected)
            {
                this.Helper.Input.Suppress(e.Button);
                this.SyncStock();
            }
        }

        /// <summary>Raised after the player releases a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button == SButton.MouseRight)
            {
                this.RightClickDown = false;
                this.Purchase_Countdown = -1;
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
                else if (Game1.activeClickableMenu != this.Shop)
                {
                    //Log.Debug($"OnUpdateTicked Game1.activeClickableMenu != shop. {Game1.activeClickableMenu}");
                    return;
                }
                else if (this.ChestsAnywhereActive && this.ChestsAnywhereApi.IsOverlayModal())
                    return; // Chests Anywhere's options / dropdown view is handling input

                bool oldMode = Game1.uiMode;
                Game1.uiMode = true;
                this.Search.Update();
                Game1.uiMode = oldMode;

                if (this.GridLayoutActive && this.RightClickDown && (this.Purchase_Countdown > 0))
                {
                    this.Purchase_Countdown--;
                    if (this.Purchase_Countdown == 0)
                    {
                        if (Game1.input.GetMouseState().RightButton == ButtonState.Pressed)
                        {
                            this.DoGridLayoutRightClick(null, this.PurchasePoint);
                        }
                    }
                }
            }
        }

        private void CloseShopMenu()
        {
            if (this.Shop != null)
            {
                Log.Trace("Closing shop menu.");
                this.Shop = null;
                this.CropData = null;

                if (this.Search != null)
                {
                    this.Search.Selected = false;
                    this.Search = null;
                }

                this.QuantityIndex = -1;
                this.Quantity_OKButton = null;
                if (this.Quantity_TextBox != null)
                {
                    this.Quantity_TextBox.Selected = false;
                    this.Quantity_TextBox = null;
                }

                this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
                this.Helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
                this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
                this.Helper.Events.Input.ButtonReleased -= this.OnButtonReleased;
                this.Helper.Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;

                this.Reflect_scrollBarRunner = null;
                this.Reflect_animations = null;
                this.Reflect_poof = null;
                this.Reflect_isStorageShop = null;
                this.Reflect_sellPercentage = null;
                this.Reflect_hoverText = null;
                this.Reflect_boldTitleText = null;
                this.Reflect_hoverPrice = null;
                this.Reflect_tryToPurchaseItem = null;
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.OldMenu is ShopMenu oldMenu)
            {
                if (oldMenu == this.Shop)
                    this.CloseShopMenu();
            }

            if (e.NewMenu is ShopMenu shopMenu)
            {
                Log.Trace($"Found new shop menu!");
                this.InitShop(shopMenu);
            }
            else
            {
                // oldMenu above should catch a close, but just do this as a safety net.
                if (this.Shop != null)
                    this.CloseShopMenu();
            }
        }

        private void DoScroll(int direction)
        {
            var forSale = this.Shop.forSale;
            int currentItemIndex = this.Shop.currentItemIndex;
            var scrollBar = this.Shop.scrollBar;
            var scrollBarRunner = this.Reflect_scrollBarRunner.GetValue();
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
            if ((this.Shop != null) && this.GridLayoutActive)
                this.DoScroll(e.Delta);
        }

        private void PurchaseItem(int numberToBuy, int idx)
        {
            var shop = this.Shop;
            var forSale = shop.forSale;
            var itemPriceAndStock = shop.itemPriceAndStock;
            int currency = shop.currency;
            if (idx < 0)
                return;

            numberToBuy = Math.Min(
                                   Math.Min(numberToBuy, ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) / Math.Max(1, itemPriceAndStock[forSale[idx]][0])),
                                   Math.Max(1, itemPriceAndStock[forSale[idx]][1])
                                  );

            numberToBuy = Math.Min(numberToBuy, forSale[idx].maximumStackSize());
            if (numberToBuy == -1)
                numberToBuy = 1;

            //tryToPurchase may change heldItem.
            if (numberToBuy > 0 && this.Reflect_tryToPurchaseItem.Invoke<bool>(forSale[idx], shop.heldItem, numberToBuy, this.PurchasePoint.X, this.PurchasePoint.Y, idx))
            {
                itemPriceAndStock.Remove(forSale[idx]);
                forSale.RemoveAt(idx);
            }
            else if (numberToBuy <= 0)
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                Game1.playSound("cancel");
            }

            if (
                (shop.heldItem != null) &&
                (this.Reflect_isStorageShop.GetValue() || Game1.options.SnappyMenus) &&
                (Game1.activeClickableMenu is ShopMenu) &&
                Game1.player.addItemToInventoryBool(shop.heldItem as Item)
               )
            {
                shop.heldItem = null;
                DelayedAction.playSoundAfterDelay("coin", 100);
            }
        }

        private void DoGridLayoutLeftClick(ButtonPressedEventArgs e, Point pt)
        {
            var shop = this.Shop;
            var forSale = shop.forSale;
            var itemPriceAndStock = shop.itemPriceAndStock;
            int currency = shop.currency;
            int currentItemIndex = shop.currentItemIndex;
            var animations = this.Reflect_animations.GetValue();
            float sellPercentage = this.Reflect_sellPercentage.GetValue();
            var scrollBarRunner = this.Reflect_scrollBarRunner.GetValue();
            var scrollBar = shop.scrollBar;
            var downArrow = shop.downArrow;
            var upArrow = shop.upArrow;
            int rows = (forSale.Count / UnitsWide);
            if ((forSale.Count % UnitsWide) != 0)
                rows++;

            int x = pt.X;
            int y = pt.Y;
            this.PurchasePoint = pt;

            if (shop.upperRightCloseButton.containsPoint(x, y))
            {
                shop.exitThisMenu();
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
                scrollBar.bounds.Y = Math.Min(shop.yPositionOnScreen + shop.height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, shop.yPositionOnScreen + upArrow.bounds.Height + 20));
                currentItemIndex = (int)Math.Round((double)Math.Max(1, rows - UnitsHigh) * ((y - scrollBarRunner.Y) / (float)scrollBarRunner.Height));
                shop.currentItemIndex = currentItemIndex;
                if (forSale.Count > 0)
                {
                    scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, rows - UnitsHigh) * currentItemIndex + upArrow.bounds.Bottom + 4;
                    if (currentItemIndex >= rows - UnitsHigh)
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
            else
            {
                for (int i = 0; i < shop.tabButtons.Count; i++)
                {
                    if (shop.tabButtons[i].containsPoint(x, y))
                    {
                        // switchTab changes the forSale list based on the tab.
                        shop.switchTab(i);
                        this.InitialItems = this.Shop.forSale;
                        this.InitialStock = this.Shop.itemPriceAndStock;

                        // the tabs filter but we do have our filter and some items/filters may overlap (dressers), so redo our filter.
                        this.SyncStock();
                        return;
                    }
                }
            }

            Vector2 clickableComponent = shop.inventory.snapToClickableComponent(x, y);
            if (shop.heldItem == null)
            {
                Item item = shop.inventory.leftClick(x, y, null, false);
                if (item != null)
                {
                    if (shop.onSell != null)
                    {
                        shop.onSell(item);
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
                                motion = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), new Vector2(shop.xPositionOnScreen - 36, shop.yPositionOnScreen + shop.height - shop.inventory.height - 16), 8f),
                                acceleration = Utility.getVelocityTowardPoint(new Point((int)clickableComponent.X + 32, (int)clickableComponent.Y + 32), new Vector2(shop.xPositionOnScreen - 36, shop.yPositionOnScreen + shop.height - shop.inventory.height - 16), 0.5f)
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
                shop.heldItem = shop.inventory.leftClick(x, y, (Item)shop.heldItem);
            }

            for (int i = currentItemIndex * UnitsWide; i < forSale.Count && i < currentItemIndex * UnitsWide + UnitsWide * 3; ++i)
            {
                int ix = i % UnitsWide;
                int iy = i / UnitsWide;
                Rectangle rect = new Rectangle(shop.xPositionOnScreen + 16 + ix * UnitWidth,
                                               shop.yPositionOnScreen + 16 + iy * UnitHeight - currentItemIndex * UnitHeight,
                                               UnitWidth, UnitHeight);
                if (rect.Contains(x, y) && forSale[i] != null)
                {
                    //int numberToBuy = (!e.IsDown(SButton.LeftShift) ? 1 : Math.Min(Math.Min(e.IsDown(SButton.LeftControl) ? 25 : 5, ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) / Math.Max(1, itemPriceAndStock[forSale[i]][0])), Math.Max(1, itemPriceAndStock[forSale[i]][1])));
                    int numberToBuy = (!e.IsDown(SButton.LeftShift) ? 1 : (e.IsDown(SButton.LeftControl) ? 25 : 5));

                    this.PurchaseItem(numberToBuy, i);
                    //numberToBuy = Math.Min(numberToBuy, forSale[i].maximumStackSize());
                    //if (numberToBuy == -1)
                    //    numberToBuy = 1;

                    ////tryToPurchase may change heldItem.
                    //if (numberToBuy > 0 && this.Reflect_tryToPurchaseItem.Invoke<bool>(forSale[i], shop.heldItem, numberToBuy, x, y, i))
                    //{
                    //    itemPriceAndStock.Remove(forSale[i]);
                    //    forSale.RemoveAt(i);
                    //}
                    //else if (numberToBuy <= 0)
                    //{
                    //    Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                    //    Game1.playSound("cancel");
                    //}

                    //if (
                    //    (shop.heldItem != null) &&
                    //    (this.Reflect_isStorageShop.GetValue() || Game1.options.SnappyMenus) &&
                    //    (Game1.activeClickableMenu is ShopMenu) &&
                    //    Game1.player.addItemToInventoryBool(shop.heldItem as Item)
                    //   )
                    //{
                    //    shop.heldItem = null;
                    //    DelayedAction.playSoundAfterDelay("coin", 100);
                    //}
                    break;
                }
            }
        }

        private void DoGridLayoutRightClick(ButtonPressedEventArgs e, Point pt)
        {
            var shop = this.Shop;
            var forSale = shop.forSale;
            var itemPriceAndStock = shop.itemPriceAndStock;
            int currency = shop.currency;
            var animations = this.Reflect_animations.GetValue();
            int currentItemIndex = shop.currentItemIndex;
            float sellPercentage = this.Reflect_sellPercentage.GetValue();
            int delayTime = Purchase_CountdownStart;

            int x = pt.X;
            int y = pt.Y;
            this.PurchasePoint = pt;

            // Copying a lot from right click code
            Vector2 clickableComponent = shop.inventory.snapToClickableComponent(x, y);
            if (shop.heldItem == null)
            {
                Item item = shop.inventory.rightClick(x, y, null, false);
                if (item != null)
                {
                    if (shop.onSell != null)
                    {
                        shop.onSell(item);
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
                        if (shop.inventory.getItemAt(x, y) == null)
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
                if (this.Purchase_Countdown == 0)
                    delayTime = Purchase_CountdownRepeat;
                shop.heldItem = shop.inventory.rightClick(x, y, shop.heldItem as Item);
            }

            for (int i = currentItemIndex * UnitsWide; i < forSale.Count && i < currentItemIndex * UnitsWide + UnitsWide * 3; ++i)
            {
                int ix = i % UnitsWide;
                int iy = i / UnitsWide;
                Rectangle rect = new Rectangle(shop.xPositionOnScreen + 16 + ix * UnitWidth,
                                               shop.yPositionOnScreen + 16 + iy * UnitHeight - currentItemIndex * UnitHeight,
                                               UnitWidth, UnitHeight);
                if (rect.Contains(x, y) && forSale[i] != null)
                {
                    bool leftShiftDown = e != null ? e.IsDown(SButton.LeftShift) : this.Helper.Input.IsDown(SButton.LeftShift);
                    bool leftCtrlDown = e != null ? e.IsDown(SButton.LeftControl) : this.Helper.Input.IsDown(SButton.LeftControl);
                    int numberToBuy = (!leftShiftDown ? 1 : Math.Min(Math.Min(leftCtrlDown ? 25 : 5, ShopMenu.getPlayerCurrencyAmount(Game1.player, currency) / Math.Max(1, itemPriceAndStock[forSale[i]][0])), Math.Max(1, itemPriceAndStock[forSale[i]][1])));
                    numberToBuy = Math.Min(numberToBuy, forSale[i].maximumStackSize());

                    //tryToPurchase may change heldItem.
                    if (numberToBuy > 0 && this.Reflect_tryToPurchaseItem.Invoke<bool>(forSale[i], shop.heldItem, numberToBuy, x, y, i))
                    {
                        itemPriceAndStock.Remove(forSale[i]);
                        forSale.RemoveAt(i);
                    }

                    if (
                        (shop.heldItem != null) &&
                        (this.Reflect_isStorageShop.GetValue() || Game1.options.SnappyMenus) &&
                        (Game1.activeClickableMenu is ShopMenu) &&
                        Game1.player.addItemToInventoryBool(shop.heldItem as Item)
                       )
                    {
                        shop.heldItem = null;
                        DelayedAction.playSoundAfterDelay("coin", 100);
                    }
                    else
                    {
                        this.Purchase_Countdown = delayTime;
                    }
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
                if (Mod.Instance.GridLayoutActive)
                    return false; // don't run original logic

                return true;
            }
            catch (Exception ex)
            {
                Mod.Instance.Monitor.Log($"Failed in {nameof(ShopMenu_receiveScrollWheelAction_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool ShopMenu_performHoverAction_Prefix(StardewValley.Object __instance, int x, int y)
        {
            try
            {
                if (Mod.Instance.GridLayoutActive)
                {
                    var shop = Mod.Instance.Shop;
                    if (shop != null)
                    {
                        // just do these
                        shop.upperRightCloseButton.tryHover(x, y, 0.5f);
                        //shop.upArrow.tryHover(x, y);
                        //shop.downArrow.tryHover(x, y);
                        //shop.scrollBar.tryHover(x, y);

                        // if in the grid layout area, then patch hover out. otherwise allow. e.g. inventory menu
                        var menuRect = new Rectangle(shop.xPositionOnScreen, shop.yPositionOnScreen, shop.width, shop.height - 256 + 32 + 4);
                        if (menuRect.Contains(x, y))
                            return false;// don't run original logic
                    }
                }

                Mod.Instance.ActiveButton.tryHover(x, y, 0.4f);

                return true;
            }
            catch (Exception ex)
            {
                Mod.Instance.Monitor.Log($"Failed in {nameof(ShopMenu_performHoverAction_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool ShopMenu_receiveRightClick_Prefix(StardewValley.Object __instance, int x, int y, bool playSound = true)
        {
            try
            {
                if (Mod.Instance.GridLayoutActive)
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
                if (Mod.Instance.GridLayoutActive)
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

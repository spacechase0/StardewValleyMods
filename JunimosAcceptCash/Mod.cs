using System.Collections.Generic;
using JunimosAcceptCash.Framework;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

namespace JunimosAcceptCash
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public Mod Instance;
        public Configuration Config;

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            this.Instance = this;
            Log.Monitor = this.Monitor;

            this.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => this.Config = new Configuration(),
                    save: () => this.Helper.WriteConfig(this.Config)
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: I18n.Config_CostMultiplier_Name,
                    tooltip: I18n.Config_CostMultiplier_Tooltip,
                    getValue: () => this.Config.CostMultiplier,
                    setValue: value => this.Config.CostMultiplier = value
                );
            }
        }

        private JunimoNoteMenu ActiveMenu;
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is JunimoNoteMenu menu)
            {
                this.ActiveMenu = menu;
                this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderMenu;
                this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdated;
            }
            else if (e.OldMenu is JunimoNoteMenu)
            {
                this.ActiveMenu = null;
                this.Helper.Events.Display.RenderedActiveMenu -= this.OnRenderMenu;
                this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdated;
            }
        }

        private void OnUpdated(object sender, UpdateTickedEventArgs e)
        {
            if (this.ActiveMenu == null)
                return;

            var currentPageBundle = this.Helper.Reflection.GetField<Bundle>(this.ActiveMenu, "currentPageBundle").GetValue();
            if (currentPageBundle == null || !this.Helper.Reflection.GetField<bool>(this.ActiveMenu, "specificBundlePage").GetValue())
                return;

            if (this.PurchaseButton == null)
                this.PurchaseButton = new ClickableTextureComponent(new Rectangle(this.ActiveMenu.xPositionOnScreen + 800, this.ActiveMenu.yPositionOnScreen + 504, 260, 72), this.ActiveMenu.noteTexture, new Rectangle(517, 286, 65, 20), 4f);
            else
            {
                this.PurchaseButton.bounds.Y = this.ActiveMenu.yPositionOnScreen;
            }

            this.PurchaseButton.tryHover(Game1.getOldMouseX(), Game1.getOldMouseY());

            if (this.PurchaseButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && this.Helper.Input.IsDown(SButton.MouseLeft))
            {
                this.Helper.Input.Suppress(SButton.MouseLeft);

                // Copied from JunimoNoteMenu, modified
                int stack = this.CalculateActiveBundleCost();
                if (Game1.player.Money >= stack)
                {
                    Game1.player.Money -= stack;
                    Game1.playSound("select");
                    if (this.PurchaseButton != null)
                        this.PurchaseButton.scale = this.PurchaseButton.baseScale * 0.75f;
                    for (int i = 0; i < currentPageBundle.numberOfIngredientSlots; ++i)
                    {
                        this.ActiveMenu.ingredientSlots[i].item = new Object(currentPageBundle.ingredients[i].id, currentPageBundle.ingredients[i].stack, false, -1, currentPageBundle.ingredients[i].quality);
                    }
                    this.Helper.Reflection.GetMethod(this.ActiveMenu, "checkIfBundleIsComplete").Invoke();

                    this.Helper.Reflection.GetMethod(this.ActiveMenu, "closeBundlePage").Invoke();
                }
                else
                    Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
            }
        }

        private ClickableTextureComponent PurchaseButton;
        private void OnRenderMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (this.ActiveMenu == null)
                return;

            var currentPageBundle = this.Helper.Reflection.GetField<Bundle>(this.ActiveMenu, "currentPageBundle").GetValue();
            if (currentPageBundle == null || !this.Helper.Reflection.GetField<bool>(this.ActiveMenu, "specificBundlePage").GetValue())
                return;

            if (this.PurchaseButton == null)
                return;

            if (this.ActiveMenu.purchaseButton != null)
                return;

            StardewValley.BellsAndWhistles.SpriteText.drawString(e.SpriteBatch, $"{this.CalculateActiveBundleCost()}g", this.PurchaseButton.bounds.X - 150, this.PurchaseButton.bounds.Y + 10);
            this.PurchaseButton.draw(e.SpriteBatch);
            Game1.dayTimeMoneyBox.drawMoneyBox(e.SpriteBatch);
            this.ActiveMenu.drawMouse(e.SpriteBatch);
        }

        // TODO: Cache this value
        private int CalculateActiveBundleCost()
        {
            var bundle = this.Helper.Reflection.GetField<Bundle>(this.ActiveMenu, "currentPageBundle").GetValue();

            int cost = 0;
            List<int> used = new List<int>();
            for (int i = 0; i < bundle.numberOfIngredientSlots; ++i)
            {
                if (this.ActiveMenu.ingredientSlots[i].item != null)
                    continue;

                int mostExpensiveSlot = 0;
                int mostExpensiveCost = new SObject(bundle.ingredients[0].id, bundle.ingredients[0].stack, false, -1, bundle.ingredients[0].quality).sellToStorePrice() * bundle.ingredients[0].stack;
                for (int j = 1; j < bundle.ingredients.Count; ++j)
                {
                    if (used.Contains(j))
                        continue;

                    int itemCost = new SObject(bundle.ingredients[j].id, bundle.ingredients[j].stack, false, -1, bundle.ingredients[j].quality).sellToStorePrice() * bundle.ingredients[j].stack;
                    if (cost > mostExpensiveCost)
                    {
                        mostExpensiveSlot = j;
                        mostExpensiveCost = itemCost;
                    }
                }
                used.Add(mostExpensiveSlot);
                cost += mostExpensiveCost;
            }

            return cost * this.Config.CostMultiplier;
        }
    }
}

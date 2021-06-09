using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace JunimosAcceptCash
{
    public class Mod : StardewModdingAPI.Mod
    {
        public Mod instance;
        public Configuration Config;

        public override void Entry(IModHelper helper)
        {
            this.instance = this;
            Log.Monitor = this.Monitor;

            this.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.onGameLaunched;
            helper.Events.Display.MenuChanged += this.onMenuChanged;
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var gmcm = this.Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (gmcm != null)
            {
                gmcm.RegisterModConfig(this.ModManifest, () => this.Config = new Configuration(), () => this.Helper.WriteConfig(this.Config));
                gmcm.RegisterSimpleOption(this.ModManifest, "Cost Multiplier", "The multiplier for the cost of the items to charge.", () => this.Config.CostMultiplier, (i) => this.Config.CostMultiplier = i);
            }
        }

        private JunimoNoteMenu activeMenu = null;
        private void onMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is JunimoNoteMenu menu)
            {
                this.activeMenu = menu;
                this.Helper.Events.Display.RenderedActiveMenu += this.onRenderMenu;
                this.Helper.Events.GameLoop.UpdateTicked += this.onUpdated;
            }
            else if (e.OldMenu is JunimoNoteMenu)
            {
                this.activeMenu = null;
                this.Helper.Events.Display.RenderedActiveMenu -= this.onRenderMenu;
                this.Helper.Events.GameLoop.UpdateTicked -= this.onUpdated;
            }
        }

        private void onUpdated(object sender, UpdateTickedEventArgs e)
        {
            if (this.activeMenu == null)
                return;

            var currentPageBundle = this.Helper.Reflection.GetField<Bundle>(this.activeMenu, "currentPageBundle").GetValue();
            if (currentPageBundle == null || !this.Helper.Reflection.GetField<bool>(this.activeMenu, "specificBundlePage").GetValue())
                return;

            if (this.purchaseButton == null)
                this.purchaseButton = new ClickableTextureComponent(new Rectangle(this.activeMenu.xPositionOnScreen + 800, this.activeMenu.yPositionOnScreen + 504, 260, 72), this.activeMenu.noteTexture, new Rectangle(517, 286, 65, 20), 4f, false);
            else
            {
                this.purchaseButton.bounds.Y = this.activeMenu.yPositionOnScreen;
            }

            this.purchaseButton.tryHover(Game1.getOldMouseX(), Game1.getOldMouseY());

            if (this.purchaseButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && this.Helper.Input.IsDown(SButton.MouseLeft))
            {
                this.Helper.Input.Suppress(SButton.MouseLeft);

                int whichArea = this.Helper.Reflection.GetField<int>(this.activeMenu, "whichArea").GetValue();

                // Copied from JunimoNoteMenu, modified
                int stack = this.calculateActiveBundleCost();
                if (Game1.player.Money >= stack)
                {
                    Game1.player.Money -= stack;
                    Game1.playSound("select");
                    if (this.purchaseButton != null)
                        this.purchaseButton.scale = this.purchaseButton.baseScale * 0.75f;
                    for (int i = 0; i < currentPageBundle.numberOfIngredientSlots; ++i)
                    {
                        this.activeMenu.ingredientSlots[i].item = new Object(currentPageBundle.ingredients[i].index, currentPageBundle.ingredients[i].stack, false, -1, currentPageBundle.ingredients[i].quality);
                    }
                    this.Helper.Reflection.GetMethod(this.activeMenu, "checkIfBundleIsComplete").Invoke();

                    this.Helper.Reflection.GetMethod(this.activeMenu, "closeBundlePage").Invoke();
                }
                else
                    Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
            }
        }

        private ClickableTextureComponent purchaseButton;
        private void onRenderMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (this.activeMenu == null)
                return;

            var currentPageBundle = this.Helper.Reflection.GetField<Bundle>(this.activeMenu, "currentPageBundle").GetValue();
            if (currentPageBundle == null || !this.Helper.Reflection.GetField<bool>(this.activeMenu, "specificBundlePage").GetValue())
                return;

            if (this.purchaseButton == null)
                return;

            if (this.Helper.Reflection.GetField<ClickableTextureComponent>(this.activeMenu, "purchaseButton").GetValue() != null)
                return;

            StardewValley.BellsAndWhistles.SpriteText.drawString(e.SpriteBatch, $"{this.calculateActiveBundleCost()}g", this.purchaseButton.bounds.X - 150, this.purchaseButton.bounds.Y + 10);
            this.purchaseButton.draw(e.SpriteBatch);
            Game1.dayTimeMoneyBox.drawMoneyBox(e.SpriteBatch, -1, -1);
            this.activeMenu.drawMouse(e.SpriteBatch);
        }

        // TODO: Cache this value
        private int calculateActiveBundleCost()
        {
            var bundle = this.Helper.Reflection.GetField<Bundle>(this.activeMenu, "currentPageBundle").GetValue();

            int cost = 0;
            List<int> used = new List<int>();
            for (int i = 0; i < bundle.numberOfIngredientSlots; ++i)
            {
                if (this.activeMenu.ingredientSlots[i].item != null)
                    continue;

                int mostExpensiveSlot = 0;
                int mostExpensiveCost = new StardewValley.Object(bundle.ingredients[0].index, bundle.ingredients[0].stack, false, -1, bundle.ingredients[0].quality).sellToStorePrice() * bundle.ingredients[0].stack;
                for (int j = 1; j < bundle.ingredients.Count; ++j)
                {
                    if (used.Contains(j))
                        continue;

                    int itemCost = new StardewValley.Object(bundle.ingredients[j].index, bundle.ingredients[j].stack, false, -1, bundle.ingredients[j].quality).sellToStorePrice() * bundle.ingredients[j].stack;
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

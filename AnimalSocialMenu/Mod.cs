using AnimalSocialMenu.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace AnimalSocialMenu
{
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        private static int MyTabId;

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
            Mod.MyTabId = SpaceCore.Menus.ReserveGameMenuTab("animals");
        }

        private int MyTabIndex = -1;
        private void OnMenuChanged(object sender, MenuChangedEventArgs args)
        {
            if (args.NewMenu is GameMenu gm)
            {
                var pages = gm.pages;
                var tabs = gm.tabs;

                this.MyTabIndex = tabs.Count;
                tabs.Add(new ClickableComponent(new Rectangle(gm.xPositionOnScreen + 192, gm.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64 - 64, 64, 64), "animals", I18n.TabTooltip())
                {
                    myID = 912342,
                    downNeighborID = 12342,
                    rightNeighborID = 12343,
                    leftNeighborID = 12341,
                    tryDefaultIfNoDownNeighborExists = true,
                    fullyImmutable = true
                });
                tabs[1].upNeighborID = 912342;
                pages.Add(new AnimalSocialPage(gm.xPositionOnScreen, gm.yPositionOnScreen, gm.width, gm.height));

                this.Helper.Events.Display.RenderedActiveMenu += this.DrawSocialIcon;
            }
            else if (args.OldMenu is GameMenu)
            {
                this.Helper.Events.Display.RenderedActiveMenu -= this.DrawSocialIcon;
            }
        }

        // The tab by default is rendered with the inventory icon due to how the tabs are hard-coded
        // This draws over it with the social icon instead of the inventory one
        private void DrawSocialIcon(object sender, RenderedActiveMenuEventArgs e)
        {
            // For some reason this check is necessary despite removing it in the onMenuChanged event.
            if (Game1.activeClickableMenu is not GameMenu menu)
            {
                this.Helper.Events.Display.RenderedActiveMenu -= this.DrawSocialIcon;
                return;
            }
            if (menu.invisible || this.MyTabIndex == -1)
                return;

            var tabs = menu.tabs;
            if (tabs.Count <= this.MyTabIndex)
            {
                return;
            }
            var tab = tabs[this.MyTabIndex];
            e.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(tab.bounds.X, tab.bounds.Y + (menu.currentTab == menu.getTabNumberFromName(tab.name) ? 8 : 0)), new Rectangle(2 * 16, 368, 16, 16), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);

            if (!Game1.options.hardwareCursor)
            {
                e.SpriteBatch.Draw(Game1.mouseCursors, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.gamepadControls ? 44 : 0, 16, 16), Color.White, 0.0f, Vector2.Zero, (float)(4.0 + Game1.dialogueButtonScale / 150.0), SpriteEffects.None, 1f);
            }
        }
    }
}

using System.Linq;
using GenericModConfigMenu.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace GenericModConfigMenu
{
    public class ModConfigMenu : IClickableMenu
    {
        private RootElement ui;
        public Table table;
        public static IClickableMenu ActiveConfigMenu;
        private bool ingame;

        public ModConfigMenu(bool inGame)
        {
            this.ingame = inGame;

            this.ui = new RootElement();

            this.table = new Table();
            this.table.RowHeight = 50;
            this.table.LocalPosition = new Vector2((Game1.viewport.Width - 800) / 2, 64);
            this.table.Size = new Vector2(800, Game1.viewport.Height - 128);

            var heading = new Label() { String = "Configure Mods", Bold = true };
            heading.LocalPosition = new Vector2((800 - heading.Measure().X) / 2, heading.LocalPosition.Y);
            this.table.AddRow(new Element[] { heading });

            foreach (var modConfigEntry in Mod.instance.configs.OrderBy(pair => pair.Key.Name))
            {
                if (this.ingame && !modConfigEntry.Value.HasAnyInGame)
                    continue;
                var label = new Label() { String = modConfigEntry.Key.Name };
                label.Callback = (Element e) => this.changeToModPage(modConfigEntry.Key);
                this.table.AddRow(new Element[] { label });
            }

            this.ui.AddChild(this.table);

            if (this.ingame || Constants.TargetPlatform == GamePlatform.Android)
                this.initializeUpperRightCloseButton();

            ActiveConfigMenu = this;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.upperRightCloseButton != null && this.readyToClose() && this.upperRightCloseButton.containsPoint(x, y))
            {
                if (playSound)
                    Game1.playSound("bigDeSelect");
                if (!this.ingame && TitleMenu.subMenu != null && Game1.activeClickableMenu != null)
                    TitleMenu.subMenu = null;
            }
        }

        public void receiveScrollWheelActionSmapi(int direction)
        {
            if (TitleMenu.subMenu == this || this.ingame)
                this.table.Scrollbar.ScrollBy(direction / -120);
            else
                ActiveConfigMenu = null;
        }

        public override void update(GameTime time)
        {
            base.update(time);
            this.ui.Update();
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Color(0, 0, 0, 192));
            this.ui.Draw(b);
            if (this.upperRightCloseButton != null)
                this.upperRightCloseButton.draw(b); // bring it above the backdrop
            if (this.ingame)
                this.drawMouse(b);
        }

        private void changeToModPage(IManifest modManifest)
        {
            Log.trace("Changing to mod config page for mod " + modManifest.UniqueID);
            Game1.playSound("bigSelect");
            if (!this.ingame)
                TitleMenu.subMenu = new SpecificModConfigMenu(modManifest, this.ingame);
            else
                Game1.activeClickableMenu = new SpecificModConfigMenu(modManifest, this.ingame);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.ui = new RootElement();

            Vector2 newSize = new Vector2(800, Game1.viewport.Height - 128);
            this.table.LocalPosition = new Vector2((Game1.viewport.Width - 800) / 2, 64);
            foreach (Element opt in this.table.Children)
                opt.LocalPosition = new Vector2(newSize.X / (this.table.Size.X / opt.LocalPosition.X), opt.LocalPosition.Y);

            this.table.Size = newSize;
            this.table.Scrollbar.Update();
            this.ui.AddChild(this.table);
        }
    }
}

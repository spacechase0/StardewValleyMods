using System.Linq;
using GenericModConfigMenu.Framework.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace GenericModConfigMenu.Framework
{
    internal class ModConfigMenu : IClickableMenu
    {
        private RootElement Ui;
        public Table Table;
        public static IClickableMenu ActiveConfigMenu;
        private bool Ingame;

        public ModConfigMenu(bool inGame)
        {
            this.Ingame = inGame;

            this.Ui = new RootElement();

            this.Table = new Table();
            this.Table.RowHeight = 50;
            this.Table.LocalPosition = new Vector2((Game1.viewport.Width - 800) / 2, 64);
            this.Table.Size = new Vector2(800, Game1.viewport.Height - 128);

            var heading = new Label() { String = "Configure Mods", Bold = true };
            heading.LocalPosition = new Vector2((800 - heading.Measure().X) / 2, heading.LocalPosition.Y);
            this.Table.AddRow(new Element[] { heading });

            foreach (var modConfigEntry in Mod.Instance.Configs.OrderBy(pair => pair.Key.Name))
            {
                if (this.Ingame && !modConfigEntry.Value.HasAnyInGame)
                    continue;
                var label = new Label() { String = modConfigEntry.Key.Name };
                label.Callback = (Element e) => this.ChangeToModPage(modConfigEntry.Key);
                this.Table.AddRow(new Element[] { label });
            }

            this.Ui.AddChild(this.Table);

            if (this.Ingame || Constants.TargetPlatform == GamePlatform.Android)
                this.initializeUpperRightCloseButton();

            ModConfigMenu.ActiveConfigMenu = this;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.upperRightCloseButton != null && this.readyToClose() && this.upperRightCloseButton.containsPoint(x, y))
            {
                if (playSound)
                    Game1.playSound("bigDeSelect");
                if (!this.Ingame && TitleMenu.subMenu != null && Game1.activeClickableMenu != null)
                    TitleMenu.subMenu = null;
            }
        }

        public void ReceiveScrollWheelActionSmapi(int direction)
        {
            if (TitleMenu.subMenu == this || this.Ingame)
                this.Table.Scrollbar.ScrollBy(direction / -120);
            else
                ModConfigMenu.ActiveConfigMenu = null;
        }

        public override void update(GameTime time)
        {
            base.update(time);
            this.Ui.Update();
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Color(0, 0, 0, 192));
            this.Ui.Draw(b);
            if (this.upperRightCloseButton != null)
                this.upperRightCloseButton.draw(b); // bring it above the backdrop
            if (this.Ingame)
                this.drawMouse(b);
        }

        private void ChangeToModPage(IManifest modManifest)
        {
            Log.Trace("Changing to mod config page for mod " + modManifest.UniqueID);
            Game1.playSound("bigSelect");
            if (!this.Ingame)
                TitleMenu.subMenu = new SpecificModConfigMenu(modManifest, this.Ingame);
            else
                Game1.activeClickableMenu = new SpecificModConfigMenu(modManifest, this.Ingame);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.Ui = new RootElement();

            Vector2 newSize = new Vector2(800, Game1.viewport.Height - 128);
            this.Table.LocalPosition = new Vector2((Game1.viewport.Width - 800) / 2, 64);
            foreach (Element opt in this.Table.Children)
                opt.LocalPosition = new Vector2(newSize.X / (this.Table.Size.X / opt.LocalPosition.X), opt.LocalPosition.Y);

            this.Table.Size = newSize;
            this.Table.Scrollbar.Update();
            this.Ui.AddChild(this.Table);
        }
    }
}

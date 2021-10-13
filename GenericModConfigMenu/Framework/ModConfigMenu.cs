using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.UI;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace GenericModConfigMenu.Framework
{
    internal class ModConfigMenu : IClickableMenu
    {
        /*********
        ** Fields
        *********/
        private RootElement Ui;
        private readonly Table Table;
        private readonly bool InGame;
        private readonly int ScrollSpeed;
        private readonly Action<IManifest> OpenModMenu;


        /*********
        ** Accessors
        *********/
        public static IClickableMenu ActiveConfigMenu;


        /*********
        ** Public methods
        *********/
        public ModConfigMenu(bool inGame, int scrollSpeed, Action<IManifest> openModMenu, ModConfigManager configs)
        {
            this.InGame = inGame;
            this.ScrollSpeed = scrollSpeed;
            this.OpenModMenu = openModMenu;

            this.Ui = new RootElement();

            this.Table = new Table
            {
                RowHeight = 50,
                LocalPosition = new Vector2((Game1.uiViewport.Width - 800) / 2, 64),
                Size = new Vector2(800, Game1.uiViewport.Height - 128)
            };

            var heading = new Label
            {
                String = I18n.List_Heading(),
                Bold = true
            };
            heading.LocalPosition = new Vector2((800 - heading.Measure().X) / 2, heading.LocalPosition.Y);
            this.Table.AddRow(new Element[] { heading });

            foreach (var entry in configs.GetAll().OrderBy(entry => entry.ModName))
            {
                if (this.InGame && !entry.AnyEditableInGame)
                    continue;
                var label = new Label
                {
                    String = entry.ModName,
                    Callback = _ => this.ChangeToModPage(entry.ModManifest)
                };
                this.Table.AddRow(new Element[] { label });
            }

            this.Ui.AddChild(this.Table);

            if (Constants.TargetPlatform == GamePlatform.Android)
                this.initializeUpperRightCloseButton();
            else
                this.upperRightCloseButton = null;

            ModConfigMenu.ActiveConfigMenu = this;
        }

        /// <inheritdoc />
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.upperRightCloseButton != null && this.readyToClose() && this.upperRightCloseButton.containsPoint(x, y))
            {
                if (playSound)
                    Game1.playSound("bigDeSelect");
                if (!this.InGame && TitleMenu.subMenu != null && Game1.activeClickableMenu != null)
                    TitleMenu.subMenu = null;
            }
        }

        public void ReceiveScrollWheelActionSmapi(int direction)
        {
            if (TitleMenu.subMenu == this || this.InGame)
                this.Table.Scrollbar.ScrollBy(direction / -this.ScrollSpeed);
            else
                ModConfigMenu.ActiveConfigMenu = null;
        }

        /// <inheritdoc />
        public override void update(GameTime time)
        {
            base.update(time);
            this.Ui.Update();
        }

        /// <inheritdoc />
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Color(0, 0, 0, 192));
            this.Ui.Draw(b);
            this.upperRightCloseButton?.draw(b); // bring it above the backdrop
            if (this.InGame)
                this.drawMouse(b);
        }

        /// <inheritdoc />
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.Ui = new RootElement();

            Vector2 newSize = new Vector2(800, Game1.uiViewport.Height - 128);
            this.Table.LocalPosition = new Vector2((Game1.uiViewport.Width - 800) / 2, 64);
            foreach (Element opt in this.Table.Children)
                opt.LocalPosition = new Vector2(newSize.X / (this.Table.Size.X / opt.LocalPosition.X), opt.LocalPosition.Y);

            this.Table.Size = newSize;
            this.Table.Scrollbar.Update();
            this.Ui.AddChild(this.Table);
        }


        /*********
        ** Private methods
        *********/
        private void ChangeToModPage(IManifest modManifest)
        {
            Log.Trace("Changing to mod config page for mod " + modManifest.UniqueID);
            Game1.playSound("bigSelect");

            this.OpenModMenu(modManifest);
        }
    }
}

using GenericModConfigMenu.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu
{
    public class ModConfigMenu : IClickableMenu
    {
        private RootElement ui;
        public Table table;
        public static IClickableMenu ActiveConfigMenu;
        private bool ingame;

        public ModConfigMenu( bool inGame )
        {
            ingame = inGame;

            ui = new RootElement();

            table = new Table();
            table.RowHeight = 50;
            table.LocalPosition = new Vector2((Game1.viewport.Width - 800) / 2, 64);
            table.Size = new Vector2(800, Game1.viewport.Height - 128);

            var heading = new Label() { String = "Configure Mods", Bold = true };
            heading.LocalPosition = new Vector2((800 - heading.Measure().X) / 2, heading.LocalPosition.Y);
            table.AddRow( new Element[] { heading } );

            foreach (var modConfigEntry in Mod.instance.configs.OrderBy(pair => pair.Key.Name))
            {
                if ( ingame && !modConfigEntry.Value.HasAnyInGame )
                    continue;
                var label = new Label() { String = modConfigEntry.Key.Name };
                label.Callback = (Element e) => changeToModPage(modConfigEntry.Key);
                table.AddRow( new Element[] { label } );
            }

            ui.AddChild(table);

            if (ingame || Constants.TargetPlatform == GamePlatform.Android)
                initializeUpperRightCloseButton();

            ActiveConfigMenu = this;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (upperRightCloseButton != null && readyToClose() && upperRightCloseButton.containsPoint(x, y))
            {
                if (playSound)
                    Game1.playSound("bigDeSelect");
                if (!ingame && TitleMenu.subMenu != null && Game1.activeClickableMenu != null)
                    TitleMenu.subMenu = null;
            }
        }

        public void receiveScrollWheelActionSmapi(int direction)
        {
            if (TitleMenu.subMenu == this || ingame)
                table.Scrollbar.ScrollBy(direction / -120);
            else
                ActiveConfigMenu = null;
        }

        public override void update(GameTime time)
        {
            base.update(time);
            ui.Update();
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), new Color(0, 0, 0, 192));
            ui.Draw(b);
            if ( upperRightCloseButton != null )
                upperRightCloseButton.draw(b); // bring it above the backdrop
            if ( ingame )
                drawMouse( b );
        }

        private void changeToModPage( IManifest modManifest )
        {
            Log.trace("Changing to mod config page for mod " + modManifest.UniqueID);
            Game1.playSound("bigSelect");
            if ( !ingame )
                TitleMenu.subMenu = new SpecificModConfigMenu( modManifest, ingame );
            else
                Game1.activeClickableMenu = new SpecificModConfigMenu( modManifest, ingame );
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            ui = new RootElement();

            Vector2 newSize = new Vector2(800, Game1.viewport.Height - 128);
            table.LocalPosition = new Vector2((Game1.viewport.Width - 800) / 2, 64);
            foreach (Element opt in table.Children)
                opt.LocalPosition = new Vector2(newSize.X / (table.Size.X / opt.LocalPosition.X), opt.LocalPosition.Y);

            table.Size = newSize;
            table.Scrollbar.Update();
            ui.AddChild(table);
        }
    }
}

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
        private Table table;

        public ModConfigMenu()
        {
            ui = new RootElement();

            table = new Table();
            table.LocalPosition = new Vector2((Game1.viewport.Width - 800) / 2, 32);
            table.Size = new Vector2(800, Game1.viewport.Height - 64);
            table.RowHeight = 50;
            foreach (var modConfigEntry in Mod.instance.configs.OrderBy(pair => pair.Key.Name))
            {
                var label = new Label() { String = modConfigEntry.Key.Name };
                label.Callback = (Element e) => changeToModPage(modConfigEntry.Key);
                table.AddRow( new Element[] { label } );
            }
            ui.AddChild(table);
        }

        public override void receiveScrollWheelAction(int direction)
        {
            table.Scrollbar.Scroll(((float)table.RowHeight / (table.RowHeight * table.RowCount)) * direction / -120);
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
        }

        private void changeToModPage( IManifest modManifest )
        {
            Log.trace("Changing to mod config page for mod " + modManifest.UniqueID);
            TitleMenu.subMenu = new SpecificModConfigMenu(modManifest);
        }
    }
}

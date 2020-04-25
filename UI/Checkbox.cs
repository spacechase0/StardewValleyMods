using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu.UI
{
    public class Checkbox : Element
    {
        public Texture2D Texture { get; set; }
        public Rectangle CheckedTextureRect { get; set; }
        public Rectangle UncheckedTextureRect { get; set; }

        public Action<Element> Callback { get; set; }

        public bool Checked { get; set; } = true;

        public Checkbox()
        {
            Texture = Game1.mouseCursors;
            CheckedTextureRect = OptionsCheckbox.sourceRectChecked;
            UncheckedTextureRect = OptionsCheckbox.sourceRectUnchecked;
        }

        public override int Width => CheckedTextureRect.Width * 4;
        public override int Height => CheckedTextureRect.Height * 4;
        public override string ClickedSound => "drumkit6";

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);

            if (Clicked && Callback != null)
            {
                Checked = !Checked;
                Callback.Invoke(this);
            }
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(Texture, Position, Checked ? CheckedTextureRect : UncheckedTextureRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 0);
            Game1.activeClickableMenu?.drawMouse(b);
        }
    }
}

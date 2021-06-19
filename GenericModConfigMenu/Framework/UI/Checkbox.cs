using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace GenericModConfigMenu.Framework.UI
{
    internal class Checkbox : Element
    {
        public Texture2D Texture { get; set; }
        public Rectangle CheckedTextureRect { get; set; }
        public Rectangle UncheckedTextureRect { get; set; }

        public Action<Element> Callback { get; set; }

        public bool Checked { get; set; } = true;

        public Checkbox()
        {
            this.Texture = Game1.mouseCursors;
            this.CheckedTextureRect = OptionsCheckbox.sourceRectChecked;
            this.UncheckedTextureRect = OptionsCheckbox.sourceRectUnchecked;
        }

        public override int Width => this.CheckedTextureRect.Width * 4;
        public override int Height => this.CheckedTextureRect.Height * 4;
        public override string ClickedSound => "drumkit6";

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);

            if (this.Clicked && this.Callback != null)
            {
                this.Checked = !this.Checked;
                this.Callback.Invoke(this);
            }
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(this.Texture, this.Position, this.Checked ? this.CheckedTextureRect : this.UncheckedTextureRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 0);
            Game1.activeClickableMenu?.drawMouse(b);
        }
    }
}

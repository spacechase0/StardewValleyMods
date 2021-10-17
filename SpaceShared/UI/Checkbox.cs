using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace SpaceShared.UI
{
    internal class Checkbox : Element
    {
        /*********
        ** Accessors
        *********/
        public Texture2D Texture { get; set; }
        public Rectangle CheckedTextureRect { get; set; }
        public Rectangle UncheckedTextureRect { get; set; }

        public Action<Element> Callback { get; set; }

        public bool Checked { get; set; } = true;

        /// <inheritdoc />
        public override int Width => this.CheckedTextureRect.Width * 4;

        /// <inheritdoc />
        public override int Height => this.CheckedTextureRect.Height * 4;

        /// <inheritdoc />
        public override string ClickedSound => "drumkit6";


        /*********
        ** Public methods
        *********/
        public Checkbox()
        {
            this.Texture = Game1.mouseCursors;
            this.CheckedTextureRect = OptionsCheckbox.sourceRectChecked;
            this.UncheckedTextureRect = OptionsCheckbox.sourceRectUnchecked;
        }

        /// <inheritdoc />
        public override void Update(bool hidden = false)
        {
            base.Update(hidden);

            if (this.Clicked && this.Callback != null)
            {
                this.Checked = !this.Checked;
                this.Callback.Invoke(this);
            }
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            b.Draw(this.Texture, this.Position, this.Checked ? this.CheckedTextureRect : this.UncheckedTextureRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 0);
            Game1.activeClickableMenu?.drawMouse(b);
        }
    }
}

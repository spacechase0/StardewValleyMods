#if !DEPENDENCY_HAS_SPACESHARED
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

#if IS_SPACECORE
namespace SpaceCore.UI
{
    public
#else
namespace SpaceShared.UI
{
    internal
#endif
        class Checkbox : Element, ISingleTexture
    {
        /*********
        ** Accessors
        *********/
        public Texture2D Texture { get; set; }
        public Rectangle CheckedTextureRect { get; set; }
        public Rectangle UncheckedTextureRect { get; set; }

        public Action<Element>? Callback { get; set; }

        public bool Checked
        {
            get => field;
            set
            {
                field = value;
                ScreenReaderText = value ? "true" : "false";
            }
        }

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

        public override bool LeftClick(Point mousePos, bool pressed)
        {
            base.LeftClick(mousePos, pressed);
            if (pressed)
            {
                Checked = !Checked;
                Callback?.Invoke(this);
            }
            return true;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            b.Draw(this.Texture, this.Position, this.Checked ? this.CheckedTextureRect : this.UncheckedTextureRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 0);
        }
    }
}
#endif

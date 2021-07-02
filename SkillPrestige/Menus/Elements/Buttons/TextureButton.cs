using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace SkillPrestige.Menus.Elements.Buttons
{
    /// <summary>
    /// Represents a button that has nothing drawn on top of it's background texture.
    /// </summary>
    public sealed class TextureButton : Button
    {
        public TextureButton(Rectangle bounds, Texture2D buttonTexture, Rectangle sourceRectangle, ClickCallback onClickCallback, string hoverText = "")
        {
            Bounds = bounds;
            ButtonTexture = buttonTexture;
            HoverText = hoverText;
            SourceRectangle = sourceRectangle;
            ClickableTextureComponent = new ClickableTextureComponent(string.Empty, Bounds, string.Empty, HoverText,
                            ButtonTexture, sourceRectangle, 1f);
            _onClick = onClickCallback;
        }

        public delegate void ClickCallback();

        private readonly ClickCallback _onClick;

        public Rectangle SourceRectangle;

        protected override string HoverText { get; }

        protected override string Text => string.Empty;

        /// <summary>Raised when the player begins hovering over the button.</summary>
        protected override void OnMouseHovered()
        {
            base.OnMouseHovered();

            Game1.playSound("smallSelect");
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="e">The event data.</param>
        /// <param name="isClick">Whether the button press is a click.</param>
        public override void OnButtonPressed(ButtonPressedEventArgs e, bool isClick)
        {
            base.OnButtonPressed(e, isClick);

            if (isClick && IsHovered)
            {
                Game1.playSound("bigSelect");
                _onClick.Invoke();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            ClickableTextureComponent.draw(spriteBatch);
        }

        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            var location = new Vector2(ClickableTextureComponent.bounds.X, ClickableTextureComponent.bounds.Y);
            spriteBatch.Draw(ClickableTextureComponent.texture, location, ClickableTextureComponent.sourceRect, color, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.4f);
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkillPrestige.InputHandling;
using SkillPrestige.Logging;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace SkillPrestige.Menus.Elements.Buttons
{
    /// <summary>
    /// Represnets a button in Stardew Valley.
    /// </summary>
    public abstract class Button : IInputHandler
    {
        /// <summary>
        /// The texture to draw for the button.
        /// </summary>
        protected virtual Texture2D ButtonTexture
        {
            get
            {
                return _buttonTexture ?? DefaultButtonTexture;
            }
            set { _buttonTexture = value; }
        }

        private Texture2D _buttonTexture;

        /// <summary>
        /// The default texture to use for a button background if none is provided.
        /// </summary>
        public static Texture2D DefaultButtonTexture { private get; set; }

        public Rectangle Bounds
        {
            get
            {
                return _bounds;
            }
            set
            {
                _bounds = value;
                ClickableTextureComponent = new ClickableTextureComponent(string.Empty, _bounds, string.Empty, HoverText,
                            ButtonTexture, new Rectangle(0, 0, 0, 0), 1f);
            }
        }
        private Rectangle _bounds;

        protected bool IsHovered { get; private set; }
        protected SpriteFont TitleTextFont { get; set; }
        protected abstract string HoverText { get; }
        protected abstract string Text { get; }

        /// <summary>
        /// The Stardew Valley component used to draw clickable items. 
        /// Certain items are handled better by the original game using the clickable texture component, 
        /// but not all; the features in the component are not all used by this mod.
        /// </summary>
        protected ClickableTextureComponent ClickableTextureComponent;

        // ReSharper disable once UnusedMemberInSuper.Global
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(ButtonTexture, Bounds, Color.White);
            DrawTitleText(spriteBatch);
        }

        public void DrawHoverText(SpriteBatch spriteBatch)
        {
            if (IsHovered) IClickableMenu.drawHoverText(spriteBatch, HoverText, Game1.smallFont);
        }

        protected void DrawTitleText(SpriteBatch spriteBatch, Vector2? locationRelativeToButton = null)
        {
            var textLocation = locationRelativeToButton;
            if (locationRelativeToButton == null)
            {
                var textSize = TitleTextFont.MeasureString(Text);
                var buttonXCenter = Bounds.X + Bounds.Width / 2;
                var buttonYCenter = Bounds.Y + Bounds.Height / 2;
                var textX = buttonXCenter - textSize.X / 2f;
                var textY = buttonYCenter - textSize.Y / 2f + 3f;
                textLocation = new Vector2(textX, textY);
            }
            else
            {
                textLocation += new Vector2(Bounds.X, Bounds.Y);
            }

            spriteBatch.DrawString(TitleTextFont, Text ?? string.Empty, textLocation.Value, Game1.textColor);
        }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="e">The event data.</param>
        public virtual void OnCursorMoved(CursorMovedEventArgs e)
        {
            IsHovered = ContainsPoint(e.NewPosition);
            if (IsHovered && !ContainsPoint(e.OldPosition))
            {
                Logger.LogVerbose($"{Text ?? HoverText} button has focus.");
                OnMouseHovered();
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="e">The event data.</param>
        /// <param name="isClick">Whether the button press is a click.</param>
        public virtual void OnButtonPressed(ButtonPressedEventArgs e, bool isClick) { }

        /// <summary>Raised when the player begins hovering over the button.</summary>
        protected virtual void OnMouseHovered() { }

        /// <summary>Get whether the cursor position is over the button.</summary>
        /// <param name="pos">The cursor position.</param>
        protected bool ContainsPoint(ICursorPosition pos)
        {
            return ClickableTextureComponent.containsPoint((int)pos.ScreenPixels.X, (int)pos.ScreenPixels.Y);
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkillPrestige.InputHandling;
using SkillPrestige.Logging;
using SkillPrestige.Menus.Elements.Buttons;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace SkillPrestige.Menus.Dialogs
{
    /// <summary>
    /// Represents a message dialog box to display information to the user.
    /// </summary>
    internal class MessageDialog : IClickableMenu, IInputHandler
    {
        private bool _buttonInstantiated;
        private int _debounceTimer = 10;
        private TextureButton _okayButton;
        private readonly string _message;

        protected MessageDialog(Rectangle bounds, string message)
            : base(bounds.X, bounds.Y, bounds.Width, bounds.Height, true)
        {
            _message = message;
        }

        private void InstantiateButtons()
        {
            if (_buttonInstantiated) return;
            _buttonInstantiated = true;
            Logger.LogVerbose("Message Dialog - Instantiating Okay button...");
            var buttonSize = Game1.tileSize;
            var okayButtonBounds = new Rectangle(xPositionOnScreen + width + Game1.tileSize / 4, yPositionOnScreen + height - buttonSize, buttonSize, buttonSize);
            _okayButton = new TextureButton(okayButtonBounds, Game1.mouseCursors, new Rectangle(128, 256, 64, 64), Okay);
            Logger.LogVerbose("Message Dialog - Okay button instantiated.");

        }

        private void Okay()
        {
            Logger.LogVerbose("Message Dialog - Okay button called.");
            exitThisMenu(false);
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            if (_debounceTimer > 0)
                _debounceTimer--;

            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);
            upperRightCloseButton?.draw(spriteBatch);
            DrawDecorations(spriteBatch);
            DrawMessage(spriteBatch);
            if (!_buttonInstantiated) InstantiateButtons();
            _okayButton.Draw(spriteBatch);
            _okayButton.DrawHoverText(spriteBatch);
            Mouse.DrawCursor(spriteBatch);
        }

        protected virtual void DrawMessage(SpriteBatch spriteBatch)
        {
            var textPadding = 2 * Game1.pixelZoom;
            var xLocationOfMessage = xPositionOnScreen + spaceToClearSideBorder*2 + textPadding;
            var yLocationOfMessage = yPositionOnScreen + spaceToClearTopBorder + textPadding;
            DrawMessage(spriteBatch, Game1.dialogueFont, new Vector2(xLocationOfMessage, yLocationOfMessage), width - spaceToClearSideBorder * 2);
        }

        protected virtual void DrawMessage(SpriteBatch spriteBatch, SpriteFont font, Vector2 textPosition, int textWidth)
        {
            spriteBatch.DrawString(font, _message.WrapText(font, textWidth), textPosition, Game1.textColor);
        }

        protected virtual void DrawDecorations(SpriteBatch spriteBatch) { }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="e">The event data.</param>
        public void OnCursorMoved(CursorMovedEventArgs e)
        {
            if (_debounceTimer > 0)
                return;

            _okayButton.OnCursorMoved(e);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="e">The event data.</param>
        /// <param name="isClick">Whether the button press is a click.</param>
        public void OnButtonPressed(ButtonPressedEventArgs e, bool isClick)
        {
            if (_debounceTimer > 0)
                return;

            _okayButton.OnButtonPressed(e, isClick);
        }
    }
}

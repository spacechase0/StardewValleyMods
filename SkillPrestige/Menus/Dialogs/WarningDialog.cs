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
    internal class WarningDialog : IClickableMenu, IInputHandler
    {
        public delegate void OkayCallback();

        public delegate void CancelCallback();

        private OkayCallback OnOkay { get; }
        private CancelCallback OnCancel { get; }
        private bool _buttonsInstantiated;
        private int _debounceTimer = 10;
        private TextureButton _okayButton;
        private TextureButton _cancelButton;
        private readonly string _message;


        public WarningDialog(Rectangle bounds, string message, OkayCallback okayCallback, CancelCallback cancelCallback)
            : base(bounds.X, bounds.Y, bounds.Width, bounds.Height, true)
        {
            OnOkay = okayCallback;
            OnCancel = cancelCallback;
            exitFunction = Cancel;
            _message = message;
        }

        private void Cancel()
        {
            Logger.LogInformation("Warning Dialog - Cancel/Close called.");
            OnCancel.Invoke();
        }

        private void InstantiateButtons()
        {
            if (_buttonsInstantiated) return;
            _buttonsInstantiated = true;
            Logger.LogVerbose("Warning Dialog - Instantiating Okay/Cancel buttons...");
            var buttonSize = Game1.tileSize;
            var buttonPadding = Game1.tileSize * 4;
            var okayButtonBounds = new Rectangle(xPositionOnScreen + width - buttonSize - spaceToClearSideBorder * 3, yPositionOnScreen + height - (buttonSize * 1.5).Floor(), buttonSize, buttonSize);
            _okayButton = new TextureButton(okayButtonBounds, Game1.mouseCursors, new Rectangle(128, 256, 64, 64), Okay, "Prestige Skill");
            Logger.LogVerbose("Warning Dialog - Okay button instantiated.");
            var cancelButtonBounds = okayButtonBounds;
            cancelButtonBounds.X -= buttonSize + buttonPadding;
            _cancelButton = new TextureButton(cancelButtonBounds, Game1.mouseCursors, new Rectangle(192, 256, 64, 64), () => exitThisMenu(false), "Cancel");
            Logger.LogVerbose("Warning Dialog - Cancel button instantiated.");

        }

        private void Okay()
        {
            Logger.LogVerbose("Warning Dialog - Okay button called.");
            OnOkay.Invoke();
            exitThisMenu(false);
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            if (_debounceTimer > 0)
                _debounceTimer--;

            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);
            var textPadding = 2 * Game1.pixelZoom;
            Game1.spriteBatch.DrawString(Game1.dialogueFont,
                _message.WrapText(Game1.dialogueFont, width - spaceToClearSideBorder * 2),
                new Vector2(xPositionOnScreen + spaceToClearSideBorder * 2 + textPadding,
                    yPositionOnScreen + spaceToClearTopBorder + textPadding), Game1.textColor);
            upperRightCloseButton?.draw(spriteBatch);
            if (!_buttonsInstantiated) InstantiateButtons();
            _okayButton.Draw(spriteBatch);
            _cancelButton.Draw(spriteBatch);
            _okayButton.DrawHoverText(spriteBatch);
            _cancelButton.DrawHoverText(spriteBatch);
            Mouse.DrawCursor(spriteBatch);
        }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="e">The event data.</param>
        public void OnCursorMoved(CursorMovedEventArgs e)
        {
            if (_debounceTimer > 0)
                return;

            _okayButton.OnCursorMoved(e);
            _cancelButton.OnCursorMoved(e);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="e">The event data.</param>
        /// <param name="isClick">Whether the button press is a click.</param>
        public void OnButtonPressed(ButtonPressedEventArgs e, bool isClick)
        {

            if (_debounceTimer > 0)
                return;

            _okayButton.OnButtonPressed(e, isClick);
            _cancelButton.OnButtonPressed(e, isClick);
        }
    }
}

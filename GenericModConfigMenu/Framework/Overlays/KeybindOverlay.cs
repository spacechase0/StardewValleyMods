using System;
using System.Linq;
using GenericModConfigMenu.Framework.ModOption;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace GenericModConfigMenu.Framework.Overlays
{
    /// <summary>Manages an active keybind overlay.</summary>
    internal class KeybindOverlay<TKeybind> : IKeybindOverlay
    {
        /*********
        ** Fields
        *********/
        /// <summary>The keybind option being bound.</summary>
        private readonly SimpleModOption<TKeybind> Option;

        /// <summary>The label to update with the bound keys when <see cref="Option"/> is rebound.</summary>
        private readonly Label Label;

        /// <summary>The pixel position and size of the keybind UI.</summary>
        private Rectangle Bounds;

        /// <summary>The button which keeps the current keybind.</summary>
        private ClickableTextureComponent OkButton;

        /// <summary>The button which clears the keybind when clicked.</summary>
        private ClickableTextureComponent ClearButton;

        /// <summary>Whether to reset the layout on the next update tick.</summary>
        /// <remarks>This defers resetting the layout until the menu is drawn, so the positions take into account UI scaling.</remarks>
        private bool ShouldResetLayout;
        
        /// <summary>Whether a button has been pressed since the menu has opened.</summary>
        /// <remarks>This prevents checking for keybinds before the menu has opened, since the menu is opened with a key press.</remarks>
        private bool HasPressedButton;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public bool IsFinished { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="option">The keybind option being bound.</param>
        /// <param name="label">The label to update with the bound keys when <see cref="Option"/> is rebound.</param>
        public KeybindOverlay(SimpleModOption<TKeybind> option, Label label)
        {
            this.Option = option;
            this.Label = label;

            this.ShouldResetLayout = true;
            this.HasPressedButton = false;
        }

        /// <inheritdoc />
        public void OnButtonsChanged(ButtonsChangedEventArgs e)
        {
            // check if a key has been pressed since opening the menu
            if (!this.HasPressedButton)
            {
                if (e.Pressed.Any())
                    this.HasPressedButton = true;
                return;
            }

            // get keys
            SButton[] released = e.Released.Where(this.IsValidKey).ToArray();
            SButton[] held = e.Held.Where(this.IsValidKey).ToArray();

            // apply keybind
            if (released.Any())
            {
                this.HandleButtons(released.Concat(held).ToArray());
                this.IsFinished = true;
            }
        }

        /// <inheritdoc />
        public void OnWindowResized()
        {
            this.ShouldResetLayout = true;
        }

        /// <inheritdoc />
        public void OnLeftClick(int x, int y)
        {
            if (this.ShouldResetLayout)
                return;

            if (this.ClearButton.containsPoint(x, y))
            {
                Game1.playSound("coin");
                this.SetValue(Array.Empty<SButton>());
                this.IsFinished = true;
            }
            else if (this.OkButton.containsPoint(x, y) || !this.Bounds.Contains(x, y))
            {
                Game1.playSound("bigDeSelect");
                this.IsFinished = true;
            }
        }

        /// <inheritdoc />
        public void Draw(SpriteBatch spriteBatch)
        {
            // reset layout if needed
            if (this.ShouldResetLayout)
            {
                this.ResetLayout();
                this.ShouldResetLayout = false;
            }

            // background
            spriteBatch.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Color(0, 0, 0, 192));
            IClickableMenu.drawTextureBox(spriteBatch, this.Bounds.X, this.Bounds.Y, this.Bounds.Width, this.Bounds.Height, Color.White);

            // "Rebinding key:" text
            {
                string str = I18n.Config_RebindKey_Title(this.Option.Name());
                int strWidth = (int)Game1.dialogueFont.MeasureString(str).X;
                spriteBatch.DrawString(Game1.dialogueFont, str, new Vector2(this.Bounds.Center.X - (strWidth / 2), this.Bounds.Y + 20), Game1.textColor);
            }

            // instruction text
            {
                string str = typeof(TKeybind) == typeof(KeybindList)
                    ? I18n.Config_RebindKey_ComboInstructions()
                    : I18n.Config_RebindKey_SimpleInstructions();
                int strWidth = (int)Game1.dialogueFont.MeasureString(str).X;
                spriteBatch.DrawString(Game1.dialogueFont, str, new Vector2(this.Bounds.Center.X - (strWidth / 2), this.Bounds.Y + 100), Game1.textColor);
            }

            // buttons
            this.OkButton.draw(spriteBatch);
            this.ClearButton.draw(spriteBatch);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reset the UI layout and positioning.</summary>
        private void ResetLayout()
        {
            Vector2 pos = Utility.getTopLeftPositionForCenteringOnScreen(width: 650, height: 200);
            int x = (int)pos.X;
            int y = (int)pos.Y;
            int width = 650;
            int height = 200;

            this.Bounds = new Rectangle(x, y, width, height);
            this.OkButton = new ClickableTextureComponent("OK", new Rectangle(x + width - Game1.tileSize * 2, y + height, Game1.tileSize, Game1.tileSize), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);
            this.ClearButton = new ClickableTextureComponent("Cancel", new Rectangle(x + width - Game1.tileSize, y + height, Game1.tileSize, Game1.tileSize), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f);
        }

        /// <summary>Get whether a button is valid for binding.</summary>
        /// <param name="button">The button to check.</param>
        private bool IsValidKey(SButton button)
        {
            SButton[] blacklist = new[]
            {
                SButton.ControllerA, // Limitation of how the menu is opened
                // The rest are because you need to be able to navigate to get to the cancel button...
                SButton.LeftThumbstickDown,
                SButton.LeftThumbstickLeft,
                SButton.LeftThumbstickRight,
                SButton.LeftThumbstickUp,
                SButton.RightThumbstickDown,
                SButton.RightThumbstickLeft,
                SButton.RightThumbstickRight,
                SButton.RightThumbstickUp,
            };

            bool pressingButton = this.ClearButton.containsPoint(Game1.getMousePosition().X, Game1.getMousePosition().Y) || this.OkButton.containsPoint(Game1.getMousePosition().X, Game1.getMousePosition().Y);

            return !blacklist.Contains( button ) && !pressingButton;
        }

        /// <summary>Handle the pressed buttons, either by assigning the keybind or cancelling the UI.</summary>
        /// <param name="buttons">The held or released keys.</param>
        private void HandleButtons(SButton[] buttons)
        {
            // handle escape
            if (buttons.Any(p => p == SButton.Escape))
                Game1.playSound("bigDeSelect");

            // apply keybind
            else
            {
                Game1.playSound("coin");
                this.SetValue(buttons);
            }
        }

        /// <summary>Set the keybind value.</summary>
        /// <param name="buttons">The buttons to set.</param>
        private void SetValue(SButton[] buttons)
        {
            switch (this.Option)
            {
                case SimpleModOption<SButton> opt:
                    opt.Value = buttons.Any()
                        ? buttons.First()
                        : SButton.None;
                    this.Label.String = opt.FormatValue();
                    break;

                case SimpleModOption<KeybindList> opt:
                    opt.Value = new KeybindList(new Keybind(buttons.ToArray()));
                    this.Label.String = opt.FormatValue();
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported keybind type {typeof(TKeybind).FullName}.");
            }
        }
    }
}

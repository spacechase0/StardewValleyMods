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
        }

        /// <inheritdoc />
        public bool TryHandle(ButtonsChangedEventArgs e)
        {
            // get keys
            SButton[] released = e.Released.Where(this.IsValidKey).ToArray();
            SButton[] held = e.Held.Where(this.IsValidKey).ToArray();

            // apply keybind
            if (released.Any())
            {
                this.HandleButtons(released.Concat(held).ToArray());
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void Draw(SpriteBatch spriteBatch)
        {
            int boxX = (Game1.uiViewport.Width - 650) / 2, boxY = (Game1.uiViewport.Height - 200) / 2;

            // background
            spriteBatch.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Color(0, 0, 0, 192));
            IClickableMenu.drawTextureBox(spriteBatch, boxX, boxY, 650, 200, Color.White);

            // "Rebinding key:" text
            {
                string str = I18n.Config_RebindKey_Title(this.Option.Name());
                int strWidth = (int)Game1.dialogueFont.MeasureString(str).X;
                spriteBatch.DrawString(Game1.dialogueFont, str, new Vector2((Game1.uiViewport.Width - strWidth) / 2, boxY + 20), Game1.textColor);
            }

            // instruction text
            {
                string str = typeof(TKeybind) == typeof(KeybindList)
                    ? I18n.Config_RebindKey_ComboInstructions()
                    : I18n.Config_RebindKey_SimpleInstructions();
                int strWidth = (int)Game1.dialogueFont.MeasureString(str).X;
                spriteBatch.DrawString(Game1.dialogueFont, str, new Vector2((Game1.uiViewport.Width - strWidth) / 2, boxY + 100), Game1.textColor);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a button is valid for binding.</summary>
        /// <param name="button">The button to check.</param>
        private bool IsValidKey(SButton button)
        {
            return
                button.TryGetKeyboard(out _)
                || button.TryGetController(out _);
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

                switch (this.Option)
                {
                    case SimpleModOption<SButton> opt:
                        opt.Value = buttons.First();
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
}

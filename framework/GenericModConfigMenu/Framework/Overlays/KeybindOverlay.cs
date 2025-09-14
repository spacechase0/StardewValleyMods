using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace GenericModConfigMenu.Framework.Overlays
{
    /// <summary>An overlay which lets the player view and edit a keybind list.</summary>
    internal class KeybindOverlay
    {
        /*********
        ** Fields
        *********/
        /****
        ** Constants
        ****/
        /// <summary>The width of the overlay box, including the <see cref="ContentPadding"/>.</summary>
        private const int BoxWith = 650;

        /// <summary>The pixel spacing around the content within the box.</summary>
        private const int ContentPadding = 30;

        /// <summary>The left pixel indent of the keybind list, added to the <see cref="ContentPadding"/>.</summary>
        private const int KeybindListIndent = 40;

        /// <summary>The button action to save changes.</summary>
        private const string OkAction = "OK";

        /// <summary>The button action to clear the keybind list.</summary>
        private const string ClearAction = "Clear";

        /// <summary>The button action to add a keybind to the list.</summary>
        private const string AddAction = "Add";

        /// <summary>The button action to remove a keybind from the list.</summary>
        private const string RemoveAction = "Remove";

        /****
        ** State
        ****/
        /// <summary>The keybinds to edit and save.</summary>
        private readonly List<Keybind> Keybinds;

        /// <summary>Whether the config only allows a single button, rather than a full keybind list.</summary>
        private readonly bool OnlyAllowSingleButton;

        /// <summary>The translated keybind name.</summary>
        private readonly string Name;

        /// <summary>The callback to invoke with the updated keybinds when the overlay is closed and saved.</summary>
        private readonly Action<Keybind[]> OnSaved;

        /// <summary>The display text to show within the content.</summary>
        private readonly List<ClickableComponent> Labels = [];

        /// <summary>The clickable buttons in the form.</summary>
        private readonly List<ClickableTextureComponent> Buttons = [];

        /// <summary>The pixel position and size of the keybind UI.</summary>
        private Rectangle Bounds;

        /// <summary>Whether to reset the layout on the next update tick.</summary>
        /// <remarks>This defers resetting the layout until the menu is drawn, so the positions take into account UI scaling.</remarks>
        private bool ShouldResetLayout = true;

        /// <summary>Whether the displayed keybind changed.</summary>
        /// <remarks>This is equivalent to <see cref="ShouldResetLayout"/>, but doesn't prevent the user from clicking UI elements (e.g. so clicking 'OK' doesn't start a keybind registration for <c>MouseLeft</c>).</remarks>
        private bool ButtonsChanged;

        /// <summary>The keybind actively being edited by the player, if any.</summary>
        private KeybindEdit KeybindEdit;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the player has finished binding the key, so the overlay can be closed.</summary>
        public bool IsFinished { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="keybinds">The keybinds to edit and save.</param>
        /// <param name="onlyAllowSingleButton">Whether the config only allows a single button, rather than a full keybind list.</param>
        /// <param name="name">The translated keybind name.</param>
        /// <param name="onSaved">The callback to invoke with the updated keybinds when the overlay is closed and saved.</param>
        public KeybindOverlay(Keybind[] keybinds, bool onlyAllowSingleButton, string name, Action<Keybind[]> onSaved)
        {
            this.Keybinds = [.. keybinds];
            this.OnlyAllowSingleButton = onlyAllowSingleButton;
            this.Name = name;
            this.OnSaved = onSaved;

            // go straight to edit view if needed
            if (onlyAllowSingleButton || keybinds.Length == 0)
                this.StartEditingKeybind(0);
        }

        /// <summary>Try to handle the pressed buttons, either by assigning the keybind or cancelling the UI.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnButtonsChanged(ButtonsChangedEventArgs e)
        {
            // exit menu
            if (e.Released.Contains(SButton.Escape))
            {
                Game1.playSound("bigDeSelect");
                this.Exit(save: false);
                return;
            }

            // handle keybind edit
            if (this.KeybindEdit != null)
            {
                // add new keys
                if (this.KeybindEdit.Add(e.Pressed))
                    this.ButtonsChanged = true;

                // finish on key release
                if (this.KeybindEdit.Any() && e.Released.Any(this.KeybindEdit.IsValidKey))
                    this.FinishEditingKeybind();
            }
        }

        /// <summary>Update the overlay when the window is resized.</summary>
        public void OnWindowResized()
        {
            this.ShouldResetLayout = true;
        }

        /// <summary>Handle the player left-clicking the overlay.</summary>
        /// <param name="x">The pixel X position where the player clicked.</param>
        /// <param name="y">The pixel Y position where the player clicked.</param>
        public void OnLeftClick(int x, int y)
        {
            if (this.ShouldResetLayout)
                return;

            // clicked button
            foreach (ClickableTextureComponent button in this.Buttons)
            {
                if (button.containsPoint(x, y))
                {
                    this.PerformButtonAction(button.name);
                    return;
                }
            }

            // clicked out of bounds
            if (!this.Bounds.Contains(x, y))
                this.PerformButtonAction(KeybindOverlay.OkAction);
        }

        /// <summary>Draw the keybind overlay.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // reset layout if needed
            if (this.ShouldResetLayout || this.ButtonsChanged)
                this.ResetLayout();

            // background
            spriteBatch.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Color(0, 0, 0, 192));
            IClickableMenu.drawTextureBox(spriteBatch, this.Bounds.X, this.Bounds.Y, this.Bounds.Width, this.Bounds.Height, Color.White);

            // text labels
            foreach (ClickableComponent label in this.Labels)
                spriteBatch.DrawString(Game1.dialogueFont, label.label, new Vector2(label.bounds.X, label.bounds.Y), Game1.textColor);

            // buttons
            foreach (ClickableTextureComponent button in this.Buttons)
                button.draw(spriteBatch);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reset the UI layout and positioning.</summary>
        private void ResetLayout()
        {
            // reset
            this.ButtonsChanged = false;
            this.ShouldResetLayout = false;
            this.Labels.Clear();
            this.Buttons.Clear();

            // init
            const int width = KeybindOverlay.BoxWith;
            const int padding = KeybindOverlay.ContentPadding;
            const int listIndent = KeybindOverlay.KeybindListIndent;
            int topOffset = padding;

            // add contents with relative positions
            // (Their real positions will be set once we center the final box.)
            {
                // 'Rebinding key: <key>' header
                string newLine = Environment.NewLine;
                topOffset += this.AddCenteredLabel(I18n.Config_RebindKey_Title(this.Name) + newLine, 0, 0, topOffset, width).Height;

                // edit keybind area
                bool isEditing = this.KeybindEdit is not null;
                if (isEditing)
                {
                    string text;
                    if (this.KeybindEdit.Any())
                        text = this.KeybindEdit.ToString();
                    else
                    {
                        text = this.OnlyAllowSingleButton
                            ? I18n.Config_RebindKey_SimpleInstructions()
                            : I18n.Config_RebindKey_ComboInstructions();
                    }

                    topOffset += this.AddCenteredLabel(text + newLine, 0, 0, topOffset, width).Height;
                }
                else if (this.OnlyAllowSingleButton)
                {
                    string text = I18n.Config_RebindKey_SimpleInstructions();

                    topOffset += this.AddCenteredLabel(text + newLine, 0, 0, topOffset, width).Height;
                }

                // keybind list
                if (!this.OnlyAllowSingleButton && !isEditing)
                {
                    // heading
                    string heading = I18n.Config_RebindKey_KeybindList();
                    Vector2 headingSize = Game1.dialogueFont.MeasureString(heading);
                    this.Labels.Add(
                        new ClickableComponent(new Rectangle(padding, topOffset, (int)headingSize.X, (int)headingSize.Y), string.Empty, heading)
                    );
                    topOffset += (int)headingSize.Y;

                    // keybind list
                    for (int i = 0; i < this.Keybinds.Count; i++)
                    {
                        string text = this.Keybinds[i].ToString();
                        Vector2 size = Game1.dialogueFont.MeasureString(text);
                        var bounds = new Rectangle(
                            x: 44 + Game1.tileSize + 10,
                            y: topOffset,
                            width: (int)size.X,
                            height: (int)size.Y
                        );
                        if (bounds.Height < Game1.tileSize)
                            bounds.Y += (Game1.tileSize - bounds.Height) / 2;

                        this.Labels.Add(new ClickableComponent(bounds, string.Empty, text));
                        this.Buttons.Add(
                            new ClickableTextureComponent($"{KeybindOverlay.RemoveAction} {i}", new Rectangle(padding + listIndent, topOffset, 44, 44), null, null, Game1.mouseCursors, new Rectangle(338, 494, 11, 11), Game1.pixelZoom)
                        );

                        topOffset += Math.Max(bounds.Height, 44);
                    }

                    // add button
                    var appendButton = new ClickableTextureComponent(KeybindOverlay.AddAction, new Rectangle(padding + listIndent, topOffset, 40, 44), null, null, Game1.mouseCursors, new Rectangle(402, 361, 10, 11), Game1.pixelZoom);
                    this.Buttons.Add(appendButton);
                    topOffset += appendButton.bounds.Height;
                }
            }

            // set content area
            int height = topOffset + padding;
            Vector2 pos = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
            int x = Math.Max(0, (int)pos.X);
            int y = Math.Max(0, (int)pos.Y);
            this.Bounds = new Rectangle(x, y, width, height);

            // shift components into content area
            foreach (ClickableComponent label in this.Labels)
            {
                label.bounds.X += x;
                label.bounds.Y += y;
            }
            foreach (ClickableTextureComponent button in this.Buttons)
            {
                button.bounds.X += x;
                button.bounds.Y += y;
            }

            // add buttons under content box
            const int mainButtonSize = Game1.tileSize;
            this.Buttons.AddRange([
                new ClickableTextureComponent(KeybindOverlay.OkAction, new Rectangle(x + width - mainButtonSize - mainButtonSize, y + height, mainButtonSize, mainButtonSize), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f),
                new ClickableTextureComponent(KeybindOverlay.ClearAction, new Rectangle(x + width - mainButtonSize, y + height, mainButtonSize, mainButtonSize), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
            ]);
        }

        /// <summary>Add a label horizontally centered within the given content area to the <see cref="Labels"/>.</summary>
        /// <param name="text">The text for which to create a label.</param>
        /// <param name="contentX">The X pixel position on screen at which the content area begins.</param>
        /// <param name="contentY">The Y pixel position on screen at which the content area begins.</param>
        /// <param name="topOffset">The pixel offset from the top of the content area at which to draw the label.</param>
        /// <param name="contentWidth">The content area's width.</param>
        /// <returns>Returns the created component's bounds.</returns>
        private Rectangle AddCenteredLabel(string text, int contentX, int contentY, int topOffset, int contentWidth)
        {
            const int padding = KeybindOverlay.ContentPadding;

            text = Game1.parseText(text, Game1.dialogueFont, contentWidth - padding - padding); // wrap long text
            Vector2 size = Game1.dialogueFont.MeasureString(text);
            Rectangle bounds = new(
                x: (int)(contentX + (contentWidth / 2f) - (size.X / 2f)),
                y: contentY + topOffset,
                width: (int)size.X,
                height: (int)size.Y
            );

            ClickableComponent label = new ClickableComponent(bounds, "", text);
            this.Labels.Add(label);
            return label.bounds;
        }

        /// <summary>Perform a button action.</summary>
        /// <param name="action">The button action to perform.</param>
        private void PerformButtonAction(string action)
        {
            switch (action)
            {
                case KeybindOverlay.ClearAction:
                    Game1.playSound("coin");
                    this.Keybinds.Clear();
                    this.Exit();
                    break;

                case KeybindOverlay.OkAction:
                    Game1.playSound("bigDeSelect");
                    this.Exit();
                    break;

                case KeybindOverlay.AddAction:
                    this.StartEditingKeybind(this.Keybinds.Count);
                    break;

                default:
                    if (action.StartsWith(KeybindOverlay.RemoveAction))
                    {
                        int keyBindIndex = ArgUtility.GetInt(action.Split(' '), 1);

                        this.Keybinds.RemoveAt(keyBindIndex);
                        this.ShouldResetLayout = true;
                    }
                    break;
            }
        }

        /// <summary>Begin editing a keybind.</summary>
        /// <param name="index">The index to edit.</param>
        private void StartEditingKeybind(int index)
        {
            this.KeybindEdit = new KeybindEdit(index, this.OnlyAllowSingleButton);
            this.ShouldResetLayout = true;
        }

        /// <summary>Finish editing the current keybind.</summary>
        private void FinishEditingKeybind()
        {
            KeybindEdit edit = this.KeybindEdit;

            // save keybind
            if (edit != null)
            {
                Keybind keybind = edit.ToKeybind();

                // add to list
                if (this.OnlyAllowSingleButton)
                {
                    this.Keybinds.Clear();
                    this.Keybinds.Add(keybind);
                }
                else if (edit.Index >= this.Keybinds.Count)
                    this.Keybinds.Add(keybind);
                else
                    this.Keybinds[edit.Index] = keybind;

                // ignore duplicate keybinds
                if (this.Keybinds.Count > 1)
                {
                    HashSet<string> seen = [];
                    this.Keybinds.RemoveWhere(bind =>
                        !seen.Add(string.Join(" + ", bind.Buttons.OrderBy(button => button)))
                    );
                }
            }

            // reset
            this.KeybindEdit = null;
            this.ShouldResetLayout = true;

            // exit if single button
            if (this.OnlyAllowSingleButton)
                this.Exit();
        }

        /// <summary>Save changes if applicable, and mark the overlay ready to close.</summary>
        /// <param name="save">Whether to save changes.</param>
        private void Exit(bool save = true)
        {
            if (save)
                this.OnSaved(this.Keybinds.ToArray());

            this.IsFinished = true;
            this.KeybindEdit = null;
        }
    }
}

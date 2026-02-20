using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;

#if IS_SPACECORE
namespace SpaceCore.UI
{
    public
#else
namespace SpaceShared.UI
{
    internal
#endif
         class TableNavigator
    {
        /*********
        ** Fields
        *********/
        private readonly Table Table;
        private int FocusedRow = -1;
        private List<int> InteractiveRows;
        private bool NeedsRebuild = true;

        /// <summary>Previous gamepad state for edge detection.</summary>
        private GamePadState PrevPad;

        // Stick navigation hold-to-repeat
        private int StickDir;
        private int StickTick;
        private bool StickInitial;
        private const float StickThreshold = 0.3f;
        private const int InitialDelay = 18; // ~300ms at 60fps
        private const int RepeatDelay = 9;   // ~150ms at 60fps

        // Dropdown navigation state
        private Dropdown OpenDropdown;
        private int DropdownOriginalChoice;

        // Bottom button navigation (Cancel, Reset, Save, Save&Close)
        private int FocusedButton = -1; // -1 = focus is in table, 0+ = index into BottomButtons
        private bool FocusOnButtons => this.FocusedButton >= 0;


        /*********
        ** Accessors
        *********/
        /// <summary>When false, snap navigation is completely disabled regardless of gamepad setting.</summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>Called when B is pressed outside a dropdown. Menu should go back/close.</summary>
        public Action OnBack { get; set; }

        /// <summary>Bottom action buttons (Cancel, Reset, Save, Save&Close). Set by the menu.</summary>
        public Label[] BottomButtons { get; set; }

        /// <summary>Get or set the currently focused row index (for save/restore across menu transitions).</summary>
        public int CurrentFocusedRow
        {
            get => this.FocusedRow;
            set => this.FocusedRow = value;
        }


        /*********
        ** Public methods
        *********/
        public TableNavigator(Table table)
        {
            this.Table = table;
            // Initialize PrevPad to current state so a freshly created navigator
            // doesn't see phantom button edges (e.g. B held from previous menu).
            this.PrevPad = Game1.input.GetGamePadState();
        }

        /// <summary>Mark the interactive rows list as stale (call after Table rows change).</summary>
        public void InvalidateRows()
        {
            this.NeedsRebuild = true;
        }

        /// <summary>Process gamepad input. Call BEFORE Ui.Update() in menu.update().</summary>
        public void HandleInput()
        {
            if (!Enabled || !Game1.options.gamepadControls)
                return;

            if (this.NeedsRebuild)
                this.RebuildInteractiveRows();

            if (this.InteractiveRows.Count == 0 && (this.BottomButtons == null || this.BottomButtons.Length == 0))
                return;

            var pad = Game1.input.GetGamePadState();

            // Initialize focus on first interactive row if not set
            if (!this.FocusOnButtons && (this.FocusedRow < 0 || !this.InteractiveRows.Contains(this.FocusedRow)))
            {
                if (this.InteractiveRows.Count > 0)
                    this.FocusedRow = this.InteractiveRows[0];
            }

            // Track dropdown state from the Dropdown class's static field
            if (Dropdown.ActiveDropdown != null && this.OpenDropdown == null)
            {
                this.OpenDropdown = Dropdown.ActiveDropdown;
                this.DropdownOriginalChoice = this.OpenDropdown.ActiveChoice;
            }
            else if (Dropdown.ActiveDropdown == null && this.OpenDropdown != null)
            {
                this.OpenDropdown = null;
            }

            this.HandleUpDown(pad);
            this.HandleLeftRight(pad);
            this.HandleB(pad);

            this.PrevPad = pad;
        }

        /// <summary>Snap cursor and ensure scroll position. Call AFTER Ui.Update() so element positions are fresh.</summary>
        public void SnapAndScroll()
        {
            if (!Enabled || !Game1.options.gamepadControls)
                return;

            if (this.FocusOnButtons)
            {
                this.SnapCursorToButton();
            }
            else
            {
                this.EnsureFocusedVisible();
                if (this.OpenDropdown != null)
                    this.SnapCursorToDropdownItem();
                else
                    this.SnapCursorToFocused();
            }
        }


        /*********
        ** Private methods
        *********/
        private void RebuildInteractiveRows()
        {
            this.InteractiveRows = new List<int>();
            var rows = this.Table.RowData;
            for (int i = 0; i < rows.Count; i++)
            {
                if (IsRowInteractive(rows[i]))
                    this.InteractiveRows.Add(i);
            }
            this.NeedsRebuild = false;
        }

        private static bool IsRowInteractive(Element[] row)
        {
            foreach (var el in row)
            {
                if (el is Checkbox) return true;
                if (el is Slider) return true;
                if (el is Dropdown) return true;
                if (el is Label label && label.Callback != null) return true;
            }
            return false;
        }

        /// <summary>Get the element to snap the cursor to in a row.</summary>
        private static Element GetSnapTarget(Element[] row)
        {
            // Prefer non-label interactive elements (checkbox, slider, dropdown)
            foreach (var el in row)
            {
                if (el is Checkbox || el is Slider || el is Dropdown)
                    return el;
            }
            // Fall back to label with callback (mod name, page link, button)
            foreach (var el in row)
            {
                if (el is Label label && label.Callback != null)
                    return el;
            }
            return row.Length > 0 ? row[0] : null;
        }

        /// <summary>Find the position of the current focused row in the interactive rows list.</summary>
        private int FindCurrentPosition()
        {
            return this.InteractiveRows.IndexOf(this.FocusedRow);
        }

        private void HandleUpDown(GamePadState pad)
        {
            // D-pad edge detection
            bool dpadUp = pad.DPad.Up == ButtonState.Pressed && this.PrevPad.DPad.Up == ButtonState.Released;
            bool dpadDown = pad.DPad.Down == ButtonState.Pressed && this.PrevPad.DPad.Down == ButtonState.Released;

            // Left stick or right stick discrete navigation with hold-to-repeat
            // (Right stick also drives row navigation to prevent bounce from direct scroll handlers)
            float lsY = Math.Abs(pad.ThumbSticks.Left.Y) >= Math.Abs(pad.ThumbSticks.Right.Y)
                ? pad.ThumbSticks.Left.Y
                : pad.ThumbSticks.Right.Y;
            int newStickDir = 0;
            if (lsY > StickThreshold) newStickDir = 1;       // Up
            else if (lsY < -StickThreshold) newStickDir = 2; // Down

            bool stickUp = false, stickDown = false;
            if (newStickDir == 0)
            {
                this.StickDir = 0;
                this.StickInitial = true;
            }
            else if (newStickDir != this.StickDir)
            {
                this.StickDir = newStickDir;
                this.StickTick = Game1.ticks;
                this.StickInitial = true;
                stickUp = newStickDir == 1;
                stickDown = newStickDir == 2;
            }
            else
            {
                int elapsed = Game1.ticks - this.StickTick;
                int delay = this.StickInitial ? InitialDelay : RepeatDelay;
                if (elapsed >= delay)
                {
                    this.StickTick = Game1.ticks;
                    this.StickInitial = false;
                    stickUp = newStickDir == 1;
                    stickDown = newStickDir == 2;
                }
            }

            bool goUp = dpadUp || stickUp;
            bool goDown = dpadDown || stickDown;
            if (!goUp && !goDown)
                return;

            // Dropdown navigation
            if (this.OpenDropdown != null)
            {
                int count = this.OpenDropdown.Choices.Length;
                if (count <= 0)
                    return;

                if (goUp)
                    this.OpenDropdown.ActiveChoice = (this.OpenDropdown.ActiveChoice - 1 + count) % count;
                else
                    this.OpenDropdown.ActiveChoice = (this.OpenDropdown.ActiveChoice + 1) % count;

                // Scroll the visible window to keep the selection visible
                int maxVis = this.OpenDropdown.MaxValuesAtOnce;
                if (this.OpenDropdown.ActiveChoice < this.OpenDropdown.ActivePosition)
                    this.OpenDropdown.ActivePosition = this.OpenDropdown.ActiveChoice;
                else if (this.OpenDropdown.ActiveChoice >= this.OpenDropdown.ActivePosition + maxVis)
                    this.OpenDropdown.ActivePosition = this.OpenDropdown.ActiveChoice - maxVis + 1;

                // Prevent Dropdown.Update() from overwriting our selection via mouse hit-test
                this.OpenDropdown.GamepadNavigated = true;

                this.OpenDropdown.Callback?.Invoke(this.OpenDropdown);
                Game1.playSound("shiny4");
                return;
            }

            bool hasButtons = this.BottomButtons != null && this.BottomButtons.Length > 0;

            // Navigation when focus is on bottom buttons
            if (this.FocusOnButtons)
            {
                if (goUp)
                {
                    // Move back to last table row
                    this.FocusedButton = -1;
                    if (this.InteractiveRows.Count > 0)
                        this.FocusedRow = this.InteractiveRows[this.InteractiveRows.Count - 1];
                    Game1.playSound("shiny4");
                }
                // Down on buttons does nothing (or could wrap to top)
                return;
            }

            // Navigation within table rows
            int pos = this.FindCurrentPosition();
            if (goUp)
            {
                if (pos <= 0)
                {
                    // At top of table — wrap to bottom buttons if available, else wrap to last row
                    if (hasButtons)
                    {
                        this.FocusedButton = 0;
                        Game1.playSound("shiny4");
                        return;
                    }
                    else
                    {
                        this.FocusedRow = this.InteractiveRows[this.InteractiveRows.Count - 1];
                    }
                }
                else
                {
                    this.FocusedRow = this.InteractiveRows[pos - 1];
                }
            }
            else // goDown
            {
                if (pos < 0 || pos >= this.InteractiveRows.Count - 1)
                {
                    // At bottom of table — move to bottom buttons if available, else wrap to first row
                    if (hasButtons)
                    {
                        this.FocusedButton = 0;
                        Game1.playSound("shiny4");
                        return;
                    }
                    else
                    {
                        this.FocusedRow = this.InteractiveRows[0];
                    }
                }
                else
                {
                    this.FocusedRow = this.InteractiveRows[pos + 1];
                }
            }

            Game1.playSound("shiny4");
        }

        private void HandleLeftRight(GamePadState pad)
        {
            // D-pad edge detection
            bool dpadLeft = pad.DPad.Left == ButtonState.Pressed && this.PrevPad.DPad.Left == ButtonState.Released;
            bool dpadRight = pad.DPad.Right == ButtonState.Pressed && this.PrevPad.DPad.Right == ButtonState.Released;

            // Left stick horizontal (edge-only)
            float lsX = pad.ThumbSticks.Left.X;
            bool stickLeft = false, stickRight = false;
            if (Math.Abs(lsX) > StickThreshold && Math.Abs(lsX) > Math.Abs(pad.ThumbSticks.Left.Y))
            {
                if (Math.Abs(this.PrevPad.ThumbSticks.Left.X) <= StickThreshold)
                {
                    stickLeft = lsX < 0;
                    stickRight = lsX > 0;
                }
            }

            bool goLeft = dpadLeft || stickLeft;
            bool goRight = dpadRight || stickRight;
            if (!goLeft && !goRight)
                return;

            // Navigate between bottom buttons with Left/Right
            if (this.FocusOnButtons && this.BottomButtons != null)
            {
                int count = this.BottomButtons.Length;
                if (goRight)
                    this.FocusedButton = Math.Min(this.FocusedButton + 1, count - 1);
                else
                    this.FocusedButton = Math.Max(this.FocusedButton - 1, 0);
                Game1.playSound("shiny4");
                return;
            }

            if (this.OpenDropdown != null)
                return; // Left/Right does nothing in dropdown mode

            if (this.FocusedRow < 0 || this.FocusedRow >= this.Table.RowData.Count)
                return;

            var target = GetSnapTarget(this.Table.RowData[this.FocusedRow]);
            if (target == null)
                return;

            // Slider: adjust value
            if (target is Slider<int> intSlider)
            {
                int step = intSlider.Interval;
                int newVal = goRight
                    ? Math.Min(intSlider.Value + step, intSlider.Maximum)
                    : Math.Max(intSlider.Value - step, intSlider.Minimum);
                if (newVal != intSlider.Value)
                {
                    intSlider.Value = newVal;
                    intSlider.Callback?.Invoke(intSlider);
                    Game1.playSound("smallSelect");
                }
                return;
            }

            if (target is Slider<float> floatSlider)
            {
                float step = floatSlider.Interval;
                float newVal = goRight
                    ? Math.Min(floatSlider.Value + step, floatSlider.Maximum)
                    : Math.Max(floatSlider.Value - step, floatSlider.Minimum);
                if (Math.Abs(newVal - floatSlider.Value) > 0.0001f)
                {
                    floatSlider.Value = newVal;
                    floatSlider.Callback?.Invoke(floatSlider);
                    Game1.playSound("smallSelect");
                }
                return;
            }
        }

        private void HandleB(GamePadState pad)
        {
            bool bPressed = pad.Buttons.B == ButtonState.Pressed && this.PrevPad.Buttons.B == ButtonState.Released;
            if (!bPressed)
                return;

            if (this.OpenDropdown != null)
            {
                // Cancel dropdown — revert to original choice
                this.OpenDropdown.ActiveChoice = this.DropdownOriginalChoice;
                this.OpenDropdown.Callback?.Invoke(this.OpenDropdown);
                this.OpenDropdown.Dropped = false;
                if (this.OpenDropdown.Parent?.RenderLast == this.OpenDropdown)
                    this.OpenDropdown.Parent.RenderLast = null;
                Dropdown.ActiveDropdown = null;
                this.OpenDropdown = null;
                Game1.playSound("bigDeSelect");
                return;
            }

            // If on buttons, move back to table
            if (this.FocusOnButtons)
            {
                this.FocusedButton = -1;
                Game1.playSound("bigDeSelect");
                return;
            }

            this.OnBack?.Invoke();
        }

        private void SnapCursorToFocused()
        {
            if (this.FocusedRow < 0 || this.FocusedRow >= this.Table.RowData.Count)
                return;

            var target = GetSnapTarget(this.Table.RowData[this.FocusedRow]);
            if (target == null)
                return;

            var bounds = target.Bounds;
            Game1.setMousePosition(bounds.Center.X, bounds.Center.Y);
        }

        private void SnapCursorToButton()
        {
            if (this.BottomButtons == null || this.FocusedButton < 0 || this.FocusedButton >= this.BottomButtons.Length)
                return;

            var btn = this.BottomButtons[this.FocusedButton];
            if (btn.IsHidden())
            {
                // Skip hidden buttons (e.g., Reset hidden on sub-pages)
                // Try to move to next visible button
                for (int i = 0; i < this.BottomButtons.Length; i++)
                {
                    if (!this.BottomButtons[i].IsHidden())
                    {
                        this.FocusedButton = i;
                        btn = this.BottomButtons[i];
                        break;
                    }
                }
                if (btn.IsHidden())
                    return;
            }

            var bounds = btn.Bounds;
            Game1.setMousePosition(bounds.Center.X, bounds.Center.Y);
        }

        private void SnapCursorToDropdownItem()
        {
            if (this.OpenDropdown == null)
                return;

            int selected = this.OpenDropdown.ActiveChoice;
            int activePos = this.OpenDropdown.ActivePosition;
            int itemHeight = this.OpenDropdown.Height;

            int tall = Math.Min(this.OpenDropdown.MaxValuesAtOnce, this.OpenDropdown.Choices.Length - activePos) * itemHeight;
            int drawY = Math.Min((int)this.OpenDropdown.Position.Y, Game1.uiViewport.Height - tall);

            int itemIndex = selected - activePos;
            if (itemIndex >= 0 && itemIndex < this.OpenDropdown.MaxValuesAtOnce)
            {
                int itemY = drawY + itemIndex * itemHeight + itemHeight / 2;
                int itemX = (int)this.OpenDropdown.Position.X + this.OpenDropdown.Width / 2;
                Game1.setMousePosition(itemX, itemY);
            }
        }

        private void EnsureFocusedVisible()
        {
            if (this.FocusOnButtons || this.FocusedRow < 0)
                return;

            var rows = this.Table.RowData;
            if (this.FocusedRow >= rows.Count)
                return;

            var target = GetSnapTarget(rows[this.FocusedRow]);
            if (target == null)
                return;

            // Element positions are now fresh (called after Ui.Update/Table.Update)
            float tableTop = this.Table.Position.Y;
            float tableBottom = tableTop + this.Table.Height;
            float elementTop = target.Position.Y;
            float elementBottom = elementTop + target.Height;

            if (elementTop < tableTop)
            {
                // Element above visible area — scroll up by the right amount
                int rowsNeeded = (int)Math.Ceiling((tableTop - elementTop) / this.Table.RowHeight);
                this.Table.Scrollbar.ScrollBy(-rowsNeeded);
            }
            else if (elementBottom > tableBottom)
            {
                // Element below visible area — scroll down by the right amount
                int rowsNeeded = (int)Math.Ceiling((elementBottom - tableBottom) / this.Table.RowHeight);
                this.Table.Scrollbar.ScrollBy(rowsNeeded);
            }
        }
    }
}

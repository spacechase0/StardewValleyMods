using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley.Extensions;

namespace GenericModConfigMenu.Framework.Overlays
{
    /// <summary>Tracks a keybind currently being edited by the player.</summary>
    internal class KeybindEdit
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The index of the keybind being edited.</summary>
        public int Index { get; }

        /// <summary>The list of buttons held down for the current keybind, in the order they were pressed.</summary>
        public HashSet<SButton> PressedButtons { get; } = [];

        /// <summary>Whether the config only allows a single button, rather than a full keybind list.</summary>
        public bool OnlyAllowSingleButton { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="index">The index of the keybind being edited.</param>
        /// <param name="onlyAllowSingleButton">Whether the config only allows a single button, rather than a full keybind list.</param>
        public KeybindEdit(int index, bool onlyAllowSingleButton)
        {
            this.Index = index;
            this.OnlyAllowSingleButton = onlyAllowSingleButton;
        }

        /// <summary>Get whether any keys were added to the keybind.</summary>
        public bool Any()
        {
            return this.PressedButtons.Count > 0;
        }

        /// <summary>Add keys to the keybind being edited.</summary>
        /// <param name="keys">The keys to add.</param>
        /// <returns>Returns whether any keys were added.</returns>
        public bool Add(IEnumerable<SButton> keys)
        {
            // ignore invalid keys
            keys = keys.Where(this.IsValidKey);

            // add
            if (this.OnlyAllowSingleButton)
            {
                SButton button = keys.LastOrDefault(SButton.None);

                if (button is SButton.None || (this.PressedButtons.Count == 1 && this.PressedButtons.Contains(button)))
                    return false;

                this.PressedButtons.Clear();
                this.PressedButtons.Add(button);
                return true;
            }
            else
                return this.PressedButtons.AddRange(keys) > 0;
        }

        /// <summary>Get whether a button is valid for keybinds.</summary>
        /// <param name="button">The button to check.</param>
        public bool IsValidKey(SButton button)
        {
            switch (button)
            {
                // limitation of how the menu is opened
                case SButton.ControllerA:
                    return false;

                // you need to be able to navigate to get to the cancel button...
                case SButton.LeftThumbstickDown:
                case SButton.LeftThumbstickLeft:
                case SButton.LeftThumbstickRight:
                case SButton.LeftThumbstickUp:
                case SButton.RightThumbstickDown:
                case SButton.RightThumbstickLeft:
                case SButton.RightThumbstickRight:
                case SButton.RightThumbstickUp:
                    return false;

                default:
                    return true;
            }
        }

        /// <summary>Create the keybind for the edited values.</summary>
        public Keybind ToKeybind()
        {
            return new Keybind(this.PressedButtons.ToArray());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(" + ", this.PressedButtons);
        }
    }
}

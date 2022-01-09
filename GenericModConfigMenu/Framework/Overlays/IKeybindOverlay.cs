using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

namespace GenericModConfigMenu.Framework.Overlays
{
    /// <summary>Manages an active keybind overlay.</summary>
    internal interface IKeybindOverlay
    {
        /// <summary>Try to handle the pressed buttons, either by assigning the keybind or cancelling the UI.</summary>
        /// <param name="e">The event arguments.</param>
        /// <returns>Returns whether the buttons were handled.</returns>
        bool TryHandle(ButtonsChangedEventArgs e);

        /// <summary>Draw the key binding overlay.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        void Draw(SpriteBatch spriteBatch);
    }
}

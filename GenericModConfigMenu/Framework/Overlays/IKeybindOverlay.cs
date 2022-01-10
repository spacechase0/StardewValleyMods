using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

namespace GenericModConfigMenu.Framework.Overlays
{
    /// <summary>Manages an active keybind overlay.</summary>
    internal interface IKeybindOverlay
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the player has finished binding the key, so the overlay can be closed.</summary>
        bool IsFinished { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Try to handle the pressed buttons, either by assigning the keybind or cancelling the UI.</summary>
        /// <param name="e">The event arguments.</param>
        void OnButtonsChanged(ButtonsChangedEventArgs e);

        /// <summary>Update the overlay when the window is resized.</summary>
        void OnWindowResized();

        /// <summary>Handle the player left-clicking the overlay.</summary>
        /// <param name="x">The pixel X position where the player clicked.</param>
        /// <param name="y">The pixel Y position where the player clicked.</param>
        void OnLeftClick(int x, int y);

        /// <summary>Draw the key binding overlay.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        void Draw(SpriteBatch spriteBatch);
    }
}

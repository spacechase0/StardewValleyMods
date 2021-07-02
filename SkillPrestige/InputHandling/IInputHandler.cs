using StardewModdingAPI.Events;

namespace SkillPrestige.InputHandling
{
    /// <summary>A component which can handle user input.</summary>
    public interface IInputHandler
    {
        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="e">The event data.</param>
        void OnCursorMoved(CursorMovedEventArgs e);

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="e">The event data.</param>
        /// <param name="isClick">Whether the button press is a click.</param>
        void OnButtonPressed(ButtonPressedEventArgs e, bool isClick);
    }
}

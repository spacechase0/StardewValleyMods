using StardewValley;
using StardewValley.Events;
using xTile.Dimensions;

namespace SpaceCore.Events
{
    public class EventArgsAction : CancelableEventArgs
    {
        internal EventArgsAction( bool touch, string action, Location pos )
        {
            TouchAction = touch;
            Action = action.Split(' ')[0];
            ActionString = action;
            Position = pos;
        }

        public bool TouchAction { get; }
        public string Action { get; }
        public string ActionString { get; }
        public Location Position { get; } // Not valid for TouchActions
    }
}

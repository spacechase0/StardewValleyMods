using SpaceShared;
using StardewValley;

namespace SpaceCore.Events
{
    public class EventArgsBeforeWarp : CancelableEventArgs
    {
        public LocationRequest WarpTargetLocation;
        public int WarpTargetX;
        public int WarpTargetY;
        public int WarpTargetFacing;

        public EventArgsBeforeWarp(LocationRequest req, int targetX, int targetY, int targetFacing)
        {
            this.WarpTargetLocation = req;
            this.WarpTargetX = targetX;
            this.WarpTargetY = targetY;
            this.WarpTargetFacing = targetFacing;
        }
    }
}

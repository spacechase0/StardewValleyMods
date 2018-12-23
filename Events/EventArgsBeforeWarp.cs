using StardewValley;

namespace SpaceCore.Events
{
    public class EventArgsBeforeWarp : CancelableEventArgs
    {
        public LocationRequest WarpTargetLocation;
        public int WarpTargetX;
        public int WarpTargetY;
        public int WarpTargetFacing;

        public EventArgsBeforeWarp( LocationRequest req, int targetX, int targetY, int targetFacing )
        {
            WarpTargetLocation = req;
            WarpTargetX = targetX;
            WarpTargetY = targetY;
            WarpTargetFacing = targetFacing;
        }
    }
}

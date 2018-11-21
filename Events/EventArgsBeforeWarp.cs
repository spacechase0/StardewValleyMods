using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

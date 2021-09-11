using System;
using System.Collections.Generic;
using StardewValley;

namespace SurfingFestival.Framework
{
    internal class RacerPlacementComparer : Comparer<string>
    {
        public override int Compare(string x, string y)
        {
            int xLaps = Mod.RacerState[x].LapsDone;
            int yLaps = Mod.RacerState[y].LapsDone;
            if (xLaps != yLaps)
                return xLaps - yLaps;

            int xPlace = this.DirectionToProgress(Mod.RacerState[x].Facing);
            int yPlace = this.DirectionToProgress(Mod.RacerState[y].Facing);
            if (xPlace != yPlace)
                return xPlace - yPlace;

            int xCoord = (int)this.GetProgressCoordinate(x);
            int yCoord = (int)this.GetProgressCoordinate(y);

            // x @ 5, y @ 10
            // right: 5 - 10 = -5, y is greater (same for down)
            // left: -5 - -10 = -5 + 10 = 5, x is greater (same for up)
            return xCoord - yCoord;
        }

        private int DirectionToProgress(int dir)
        {
            return dir switch
            {
                Game1.up => 3,
                Game1.down => 1,
                Game1.left => 2,
                Game1.right => 0,
                _ => throw new ArgumentException("Bad facing direction")
            };
        }

        private float GetProgressCoordinate(string racerName)
        {
            return Mod.RacerState[racerName].Facing switch
            {
                Game1.up => -Game1.CurrentEvent.getCharacterByName(racerName).Position.Y,
                Game1.down => Game1.CurrentEvent.getCharacterByName(racerName).Position.Y,
                Game1.left => -Game1.CurrentEvent.getCharacterByName(racerName).Position.X,
                Game1.right => Game1.CurrentEvent.getCharacterByName(racerName).Position.X,
                _ => throw new ArgumentException("Bad facing direction")
            };
        }
    };
}

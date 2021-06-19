using Microsoft.Xna.Framework;
using StardewValley;

namespace SleepyEye.Framework
{
    /// <summary>Metadata about where and when the player camped.</summary>
    internal class CampData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The name of the game location.</summary>
        public string Location { get; set; }

        /// <summary>The player's map pixel position.</summary>
        public Vector2 Position { get; set; }

        /// <summary>The mine level, if applicable.</summary>
        public int? MineLevel { get; set; }

        /// <summary>The <see cref="WorldDate.TotalDays"/> value when the player slept.</summary>
        public int DaysPlayed { get; set; }
    }
}

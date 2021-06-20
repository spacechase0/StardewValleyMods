using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace ExperienceBars.Framework
{
    internal class Configuration
    {
        /// <summary>The button which shows or hides the experience bars display.</summary>
        public SButton ToggleBars { get; set; } = SButton.X;

        /// <summary>The pixel position at which to draw the experience bars, relative to the top-left corner of the screen.</summary>
        public Point Position { get; set; } = new(10, 10);
    }
}

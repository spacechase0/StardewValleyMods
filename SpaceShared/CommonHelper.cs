using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;

namespace SpaceShared
{
    /// <summary>Provides common utility methods for interacting with the game code.</summary>
    internal static class CommonHelper
    {
        /// <summary>Get all game locations.</summary>
        /// <param name="includeTempLevels">Whether to include temporary mine/dungeon locations.</param>
        public static IEnumerable<GameLocation> GetLocations(bool includeTempLevels = false)
        {
            //
            // Copied from CommonHelper in Pathoschild's repo: https://github.com/Pathoschild/StardewMods
            //

            var locations = Game1.locations
                .Concat(
                    from location in Game1.locations
                    from building in location.buildings
                    where building.indoors.Value != null
                    select building.indoors.Value
                );

            if (includeTempLevels)
                locations = locations.Concat(MineShaft.activeMines).Concat(VolcanoDungeon.activeLevels);

            return locations;
        }

        /// <summary>Get the top-left pixel coordinate for a rectangle so that it renders flush against the given anchor position, with an optional offset.</summary>
        /// <param name="offsetX">The X pixel offset from the anchor position, with positive numbers moving inwards from the screen edge.</param>
        /// <param name="offsetY">The Y pixel offset from the anchor position, with positive numbers moving inwards from the screen edge.</param>
        /// <param name="width">The rectangle width to position.</param>
        /// <param name="height">The rectangle height to position.</param>
        /// <param name="anchor">The screen position from which to offset the rectangle.</param>
        /// <param name="uiScale">Whether to apply UI scaling, or <c>null</c> to use the game's current UI scaling mode.</param>
        /// <param name="zoom">The zoom factor to apply to the <paramref name="width"/> and <paramref name="height"/> values.</param>
        public static Vector2 GetPositionFromAnchor(int offsetX, int offsetY, int width, int height, PositionAnchor anchor, bool? uiScale = null, int zoom = Game1.pixelZoom)
        {
            var screen = uiScale ?? Game1.uiMode
                ? Game1.uiViewport
                : Game1.viewport;

            if (anchor is PositionAnchor.BottomRight or PositionAnchor.TopRight)
                offsetX = screen.Width - (width * zoom) - offsetX;
            if (anchor is PositionAnchor.BottomLeft or PositionAnchor.BottomRight)
                offsetY = screen.Height - (height * zoom) - offsetY;

            return new Vector2(offsetX, offsetY);
        }
    }
}

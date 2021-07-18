using System.Collections.Generic;
using System.Linq;
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
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors.Value != null
                    select building.indoors.Value
                );

            if (includeTempLevels)
                locations = locations.Concat(MineShaft.activeMines).Concat(VolcanoDungeon.activeLevels);

            return locations;
        }
    }
}

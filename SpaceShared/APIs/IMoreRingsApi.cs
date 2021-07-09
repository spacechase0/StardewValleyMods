using System.Collections.Generic;
using StardewValley;
using StardewValley.Objects;

namespace SpaceShared.APIs
{
    public interface IMoreRingsApi
    {
        /// <summary>
        /// Count how many of the specified ring type the given player has equipped. This includes the vanilla left & right rings.
        /// </summary>
        /// <returns>How many of the specified ring the given player has equipped.</returns>
        /// <param name="f">The farmer/farmhand whose inventory is being checked. For the local player, use Game1.player.</param>
        /// <param name="which">The parentSheetIndex of the ring.</param>
        int CountEquippedRings(Farmer f, int which);

        /// <summary>
        /// Returns a list of all rings the player has equipped. This includes the vanilla left & right rings.
        /// </summary>
        /// <returns>A list of all equipped rings.</returns>
        /// <param name="f">The farmer/farmhand whose inventory is being checked. For the local player, use Game1.player.</param>
        IEnumerable<Ring> GetAllRings(Farmer f);
    }
}

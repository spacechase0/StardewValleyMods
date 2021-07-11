using System;
using StardewModdingAPI;

namespace Magic
{
    /// <summary>The API for the Magic mod.</summary>
    public interface IApi
    {
        /*********
        ** Accessors
        *********/
        /// <summary>An event raised when the player casts the analyze spell.</summary>
        event EventHandler OnAnalyzeCast;


        /*********
        ** Methods
        *********/
        /// <summary>Cause the player to forget all learned spell and reset earned spell points to zero. This should only be used in very specialized cases like Skill Prestige.</summary>
        /// <param name="manifest">The mod calling the API.</param>
        void ResetProgress(IManifest manifest);

        /// <summary>Reduce the player's spell points by the given amount (or add spell points if the <paramref name="count"/> is negative).</summary>
        /// <param name="manifest">The mod calling the API.</param>
        /// <param name="count">The number of spell points.</param>
        void UseSpellPoints(IManifest manifest, int count);
    }
}

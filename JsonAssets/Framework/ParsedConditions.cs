using System.Collections.Generic;
using System.Linq;
using SpaceShared.APIs;
using StardewValley;

namespace JsonAssets.Framework
{
    /// <summary>A set of parsed in-game requirements using the event precondition or Expanded Preconditions Utility format.</summary>
    public class ParsedConditions : IParsedConditions
    {
        /*********
        ** Fields
        *********/
        /// <summary>The Expanded Preconditions Utility API, if that mod is loaded.</summary>
        private readonly IExpandedPreconditionsUtilityApi ExpandedPreconditionsUtility;

        /// <summary>The raw vanilla or Expanded Preconditions Utility condition string.</summary>
        private readonly string RawConditions;


        /*********
        ** Accessors
        *********/
        /// <summary>A cached instance with empty conditions that always return true.</summary>
        public static ParsedConditions AlwaysTrue { get; } = new(null, null);

        /// <inheritdoc />
        public bool HasConditions { get; }

        /// <inheritdoc />
        public bool NeedsExpandedPreconditionsUtility { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="rawConditions">The raw vanilla or Expanded Preconditions Utility conditions.</param>
        /// <param name="expandedPreconditionsUtility">The Expanded Preconditions Utility API, if that mod is loaded.</param>
        public ParsedConditions(IList<string> rawConditions, IExpandedPreconditionsUtilityApi expandedPreconditionsUtility)
        {
            this.RawConditions = string.Join("/", rawConditions ?? Enumerable.Empty<string>());
            this.HasConditions = !string.IsNullOrWhiteSpace(this.RawConditions);
            this.NeedsExpandedPreconditionsUtility = !this.IsVanillaOnly(this.RawConditions);
            this.ExpandedPreconditionsUtility = expandedPreconditionsUtility;
        }

        /// <inheritdoc />
        public bool CurrentlyMatch()
        {
            // not conditional
            if (!this.HasConditions)
                return true;

            // EPU format
            if (this.NeedsExpandedPreconditionsUtility)
            {
                // If EPU isn't installed, all EPU conditions automatically fail.
                // Json Assets will show a separate error/warning about this.
                if (this.ExpandedPreconditionsUtility == null)
                    return false;

                // check conditions
                return this.ExpandedPreconditionsUtility.CheckConditions(this.RawConditions);
            }

            // vanilla format
            return this.CurrentlyMatchVanilla();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether the conditions match using the vanilla game logic.</summary>
        private bool CurrentlyMatchVanilla()
        {
            string eventId = (int.MinValue + 1720).ToString();
            bool wasSeen = false;

            try
            {
                wasSeen = Game1.player.eventsSeen.Remove(eventId);

                GameLocation location = Game1.currentLocation ?? Game1.getFarm();
                return location.checkEventPrecondition($"{eventId}/{this.RawConditions}") == eventId;
            }
            finally
            {
                if (wasSeen)
                    Game1.player.eventsSeen.Add(eventId);
            }
        }

        /// <summary>Get whether a condition string consists only of vanilla requirements that don't require Expanded Preconditions Utility.</summary>
        /// <param name="conditions">The condition string to validate.</param>
        private bool IsVanillaOnly(string conditions)
        {
            if (!string.IsNullOrWhiteSpace(conditions))
            {
                // We can distinguish between vanilla and EPU conditions based on two factors:
                //   1. EPU adds '!' to invert conditions;
                //   2. EPU uses readable flags like 'HasCookingRecipe', compared to the game's 1-2 character flags like 'x' or 'Hn'.
                foreach (string condition in conditions.Split('/'))
                {
                    string flag = condition.Trim().Split(' ')[0];
                    if (flag.StartsWith("!") || flag.Length > 3)
                        return false;
                }
            }

            return true;
        }
    }
}

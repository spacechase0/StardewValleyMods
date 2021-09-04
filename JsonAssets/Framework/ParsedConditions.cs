using System.Collections.Generic;
using System.Linq;
using SpaceShared.APIs;

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

        /// <summary>Whether there are any conditions.</summary>
        public bool HasConditions { get; }

        /// <summary>Whether Expanded Preconditions Utility is needed to handle the conditions.</summary>
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
            this.NeedsExpandedPreconditionsUtility = this.HasConditions;
            this.ExpandedPreconditionsUtility = expandedPreconditionsUtility;
        }

        /// <summary>Get the current result of the conditions.</summary>
        public bool CurrentlyMatch()
        {
            // not conditional
            if (!this.HasConditions)
                return true;

            // If EPU isn't installed, all EPU conditions automatically fail.
            // Json Assets will show a separate error/warning about this.
            if (this.ExpandedPreconditionsUtility == null)
                return false;

            // check conditions
            return this.ExpandedPreconditionsUtility.CheckConditions(this.RawConditions);
        }
    }
}

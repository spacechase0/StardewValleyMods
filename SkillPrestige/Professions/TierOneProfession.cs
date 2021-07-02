using System.Collections.Generic;

namespace SkillPrestige.Professions
{
    /// <summary>
    /// Represents a Tier 1 profession in Stardew Valley (a profession available at level 5).
    /// </summary>
    public class TierOneProfession : Profession
    {
        public override int LevelAvailableAt => 5;

        /// <summary>
        /// The tier two (available at level 10) professions that are available when this profession is chosen.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IEnumerable<TierTwoProfession> TierTwoProfessions { get; set; }
    }
}

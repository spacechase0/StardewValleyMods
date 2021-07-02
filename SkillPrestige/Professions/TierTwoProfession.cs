namespace SkillPrestige.Professions
{
    /// <summary>
    /// Represents a Tier 2 profession in Stardew Valley (a profession available at level 10).
    /// </summary>
    public class TierTwoProfession : Profession
    {
        public override int LevelAvailableAt => 10;

        /// <summary>
        /// The tier 1 profession that is required for this profession to be chosen.
        /// </summary>
        public TierOneProfession TierOneProfession { get; set; }
    }
}

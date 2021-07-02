using System.Collections.Generic;

namespace SkillPrestige.Professions
{
    public partial class Profession
    {
        public static IEnumerable<Profession> ForagingProfessions => new List<Profession>
        {
            Forester,
            Gatherer,
            Lumberjack,
            Tapper,
            Botanist,
            Tracker
        };

        protected static TierOneProfession Forester { get; set; }
        protected static TierOneProfession Gatherer { get; set; }
        protected static TierTwoProfession Lumberjack { get; set; }
        protected static TierTwoProfession Tapper { get; set; }
        protected static TierTwoProfession Botanist { get; set; }
        protected static TierTwoProfession Tracker { get; set; }
    }
}

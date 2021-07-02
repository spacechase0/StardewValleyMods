using System.Collections.Generic;

namespace SkillPrestige.Professions
{
    public partial class Profession
    {
        public static IEnumerable<Profession> FishingProfessions => new List<Profession>
        {
            Fisher,
            Trapper,
            Angler,
            Pirate,
            Mariner,
            Luremaster
        };

        protected static TierOneProfession Fisher { get; set; }
        protected static TierOneProfession Trapper { get; set; }
        protected static TierTwoProfession Angler { get; set; }
        protected static TierTwoProfession Pirate { get; set; }
        protected static TierTwoProfession Mariner { get; set; }
        protected static TierTwoProfession Luremaster { get; set; }
    }
}

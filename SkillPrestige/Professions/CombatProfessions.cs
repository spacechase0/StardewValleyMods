using System.Collections.Generic;

namespace SkillPrestige.Professions
{
    public partial class Profession
    {
        public static IEnumerable<Profession> CombatProfessions => new List<Profession>
        {
            Fighter,
            Scout,
            Brute,
            Defender,
            Acrobat,
            Desperado
        };

        protected static TierOneProfession Fighter { get; set; }
        protected static TierOneProfession Scout { get; set; }
        protected static TierTwoProfession Brute { get; set; }
        protected static TierTwoProfession Defender { get; set; }
        protected static TierTwoProfession Acrobat { get; set; }
        protected static TierTwoProfession Desperado { get; set; }
    }
}

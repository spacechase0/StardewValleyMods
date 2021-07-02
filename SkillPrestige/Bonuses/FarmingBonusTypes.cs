using System.Collections.Generic;

namespace SkillPrestige.Bonuses
{
    public partial class BonusType
    {
        public static IEnumerable<BonusType> FarmingBonusTypes => new List<BonusType>
        {
            FarmingToolProficiency,
            BetterCrops,
            EfficientAnimals,
            RegrowthOpportunity
        };

        protected static BonusType FarmingToolProficiency { get; set; }
        protected static BonusType BetterCrops { get; set; }
        protected static BonusType EfficientAnimals { get; set; }
        protected static BonusType RegrowthOpportunity { get; set; }
    }
}
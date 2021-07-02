using System.Linq;

namespace SkillPrestige.Bonuses
{
    /// <summary>
    /// Represents a bonus in this mod, which are post-all-professions-prestiged effects the player can purchase.
    /// </summary>
    public class Bonus
    {
        public string BonusTypeCode { get; set; }

        public BonusType Type
        {
            get { return BonusType.AllBonusTypes.Single(x => x.Code == BonusTypeCode); }
        }

        public int Level { get; set; }

        public void ApplyEffect()
        {
            Type.ApplyEffect.Invoke(Level);
        }
    }
}

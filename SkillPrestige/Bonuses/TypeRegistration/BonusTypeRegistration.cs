namespace SkillPrestige.Bonuses.TypeRegistration
{
    public abstract class BonusTypeRegistration : BonusType, IBonusTypeRegistration
    {
        /// <summary>
        /// This call will 'register' available professions with the bonus type class.
        /// </summary>
        public abstract void RegisterBonusTypes();
    }
}

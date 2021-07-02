namespace SkillPrestige.Bonuses.TypeRegistration
{
    public interface IBonusTypeRegistration
    {
        /// <summary>
        /// This call will 'register' available bonus types with the bonus type class.
        /// </summary>
        void RegisterBonusTypes();
    }
}
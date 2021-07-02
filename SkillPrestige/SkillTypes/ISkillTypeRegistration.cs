namespace SkillPrestige.SkillTypes
{
    /// <summary>
    /// Allows for a registration of a skill type with the base skill type class.
    /// </summary>
    public interface ISkillTypeRegistration
    {
        /// <summary>
        /// Implementations of this method should initialize static implementations of skill types in the SkillType class.
        /// </summary>
        void RegisterSkillTypes();
    }
}
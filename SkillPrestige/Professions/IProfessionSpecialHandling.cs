namespace SkillPrestige.Professions
{
    /// <summary>
    /// Represents special handling for professions where Stardew Valley applies the profession's effects in a custom manner.
    /// </summary>
    public interface IProfessionSpecialHandling
    {
        /// <summary>Apply effects for the profession.</summary>
        void ApplyEffect();

        /// <summary>Remove effects for the profession.</summary>
        void RemoveEffect();
    }
}

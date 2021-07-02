using SkillPrestige.Logging;

namespace SkillPrestige.SkillTypes
{
    /// <summary>
    /// Class registers all of the skill types that are available in Stardew Valley by default.
    /// </summary>
    // ReSharper disable once UnusedMember.Global - created through reflection.
    public sealed class DefaultSkillTypeRegistration : SkillType, ISkillTypeRegistration
    {
        public void RegisterSkillTypes()
        {
            Logger.LogInformation("Registering default skill types...");
            Farming = new SkillType("Farming", 0);
            Fishing = new SkillType("Fishing", 1);
            Foraging = new SkillType("Foraging", 2);
            Mining = new SkillType("Mining", 3);
            Combat = new SkillType("Combat", 4);
            Logger.LogInformation("Default skill types registered.");
        }

    }
}


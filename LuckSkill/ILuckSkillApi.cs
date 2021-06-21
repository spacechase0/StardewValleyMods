using System.Collections.Generic;

namespace LuckSkill
{
    /// <summary>The mod-provided API for Luck Skill.</summary>
    public interface ILuckSkillApi
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The ID for the Fortunate profession.</summary>
        int FortunateProfessionId { get; }

        /// <summary>The ID for the Popular Helper profession.</summary>
        int PopularHelperProfessionId { get; }

        /// <summary>The ID for the Lucky profession.</summary>
        int LuckyProfessionId { get; }

        /// <summary>The ID for the Un-unlucky profession.</summary>
        int UnUnluckyProfessionId { get; }

        /// <summary>The ID for the Shooting Star profession.</summary>
        int ShootingStarProfessionId { get; }

        /// <summary>The ID for the Spirit Child profession.</summary>
        int SpiritChildProfessionId { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the available Luck professions by ID.</summary>
        IDictionary<int, IProfession> GetProfessions();
    }
}

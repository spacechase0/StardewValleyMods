using StardewValley;

namespace LuckSkill
{
    /// <summary>A luck skill profession.</summary>
    public interface IProfession
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique profession ID in <see cref="Farmer.professions"/>.</summary>
        int Id { get; }

        /// <summary>The default English display name.</summary>
        string DefaultName { get; }

        /// <summary>The translated display name.</summary>
        string Name { get; }

        /// <summary>The default description text.</summary>
        string DefaultDescription { get; }

        /// <summary>The translated description text.</summary>
        string Description { get; }
    }
}

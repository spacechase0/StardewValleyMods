namespace Magic.Framework
{
    /// <summary>Defines constants for the magic system.</summary>
    internal class MagicConstants
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The number of spell bar which players are expected to have.</summary>
        public static int SpellBarCount { get; } = 2;

        /// <summary>The ID of the event in which the player learns magic from the Wizard.</summary>
        public static string LearnedMagicEventId { get; } = "90001";

        /// <summary>The number of mana points gained per magic level.</summary>
        public static int ManaPointsPerLevel { get; } = 100;
    }
}

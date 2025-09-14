using SpaceShared;

namespace AnotherHungerMod.Framework
{
    /// <summary>The mod configuration model.</summary>
    internal class Configuration
    {
        /// <summary>The X pixel position of the fullness UI.</summary>
        public int FullnessUiX { get; set; } = 10;

        /// <summary>The Y pixel position of the fullness UI.</summary>
        public int FullnessUiY { get; set; } = 350;

        /// <summary>The alignment for the fullness UI on the game screen.</summary>
        public PositionAnchor FullnessUiAlignment { get; set; } = PositionAnchor.TopLeft;

        /// <summary>The maximum amount of fullness you can have.</summary>
        public int MaxFullness { get; set; } = 100;

        /// <summary>The amount of fullness to drain per in-game minute.</summary>
        public float DrainPerMinute { get; set; } = 0.08f;

        /// <summary>A multiplier for the amount of fullness you get, based on the food's edibility.</summary>
        public float EdibilityMultiplier { get; set; } = 1;

        /// <summary>The minimum fullness needed for positive buffs to apply.</summary>
        public int PositiveBuffThreshold { get; set; } = 80;

        /// <summary>The maximum fullness before negative buffs apply.</summary>
        public int NegativeBuffThreshold { get; set; } = 25;

        /// <summary>The amount of starvation damage taken every in-game minute when you have no fullness.</summary>
        public float StarvationDamagePerMinute { get; set; } = 1;

        /// <summary>The relationship points penalty for not feeding your spouse.</summary>
        public int RelationshipHitForNotFeedingSpouse { get; set; } = 50;

        /// <summary>When the time changes by a large amount (e.g. setting the time), the maximum number of minutes to count towards fullness drain or starvation damage.</summary>
        public int MaxTransitionMinutes { get; set; } = 30;
    }
}

namespace AnotherHungerMod.Framework
{
    /// <summary>The mod configuration model.</summary>
    internal class Configuration
    {
        public int FullnessUiX = 10;
        public int FullnessUiY = 350;

        public int MaxFullness = 100;
        public float EdibilityMultiplier = 1;
        public float DrainPerMinute = 0.08f;
        public int PositiveBuffThreshold = 80;
        public int NegativeBuffThreshold = 25;

        public float StarvationDamagePerMinute = 1;

        public int RelationshipHitForNotFeedingSpouse = 50;

        /// <summary>When the time changes by a large amount (e.g. setting the time), the maximum number of minutes to count towards fullness drain or starvation damage.</summary>
        public int MaxTransitionMinutes = 30;
    }
}

using System.Diagnostics.CodeAnalysis;

namespace MoreRings.Framework
{
    /// <summary>The mod configuration model.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named to simplify readability for players editing the config file.")]
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The percentage chance of a higher crop quality with the Quality+ ring equipped.</summary>
        public float QualityRing_ChancePerRing { get; set; } = 0.125f;

        /// <summary>The multiplier applied to the fishing bar size with the Ring of Wide Nets equipped.</summary>
        public float RingOfWideNets_BarSizeMultiplier { get; set; } = 1.5f;

        /// <summary>The health regen rate per second with the Ring of Regeneration equipped.</summary>
        public float RingOfRegeneration_RegenPerSecond { get; set; } = 0.25f;

        /// <summary>The stamina regen rate per second with the Refreshing Ring equipped.</summary>
        public float RefreshingRing_RegenPerSecond { get; set; } = 0.25f;

        /// <summary>The distance in tiles at which the player can use tools with the Ring of Far Reaching equipped.</summary>
        public int RingOfFarReaching_TileDistance { get; set; } = 100;
    }
}

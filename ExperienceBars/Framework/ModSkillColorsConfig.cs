using Microsoft.Xna.Framework;
using StardewValley;

namespace ExperienceBars.Framework
{
    /// <summary>The colors to use for each skill.</summary>
    internal class ModSkillColorsConfig
    {
        /// <summary>The color to use for the <see cref="Farmer.combatSkill"/> skill.</summary>
        public Color Combat { get; set; } = new(178, 255, 211);

        /// <summary>The color to use for the <see cref="Farmer.farmingSkill"/> skill.</summary>
        public Color Farming { get; set; } = new(115, 255, 56);

        /// <summary>The color to use for the <see cref="Farmer.fishingSkill"/> skill.</summary>
        public Color Fishing { get; set; } = new(117, 225, 255);

        /// <summary>The color to use for the <see cref="Farmer.foragingSkill"/> skill.</summary>
        public Color Foraging { get; set; } = new(205, 127, 50);

        /// <summary>The color to use for the <see cref="Farmer.luckLevel"/> skill.</summary>
        public Color Luck { get; set; } = new(255, 255, 84);

        /// <summary>The color to use for the <see cref="Farmer.miningSkill"/> skill.</summary>
        public Color Mining { get; set; } = new(247, 31, 0);
    }
}

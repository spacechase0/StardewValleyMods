using System;
using System.Collections.Generic;
using System.Linq;
using SkillPrestige.Mods;

namespace SkillPrestige
{
    /// <summary>
    /// Represents a set of prestiges that are tied to skills.
    /// </summary>
    [Serializable]
    public class PrestigeSet
    {
        public IEnumerable<Prestige> Prestiges { get; set; }

        /// <summary>
        /// the default prestige set that contains prestiges for each of the skills in the unmodded version Stardew Valley.
        /// </summary>
        private static List<Prestige> DefaultPrestiges =>
            new List<Prestige>
            {
                new Prestige
                {
                    SkillType = SkillTypes.SkillType.Farming
                },
                new Prestige
                {
                    SkillType = SkillTypes.SkillType.Mining
                },
                new Prestige
                {
                    SkillType = SkillTypes.SkillType.Fishing
                },
                new Prestige
                {
                    SkillType = SkillTypes.SkillType.Foraging
                },
                new Prestige
                {
                    SkillType = SkillTypes.SkillType.Combat
                }
            };

        /// <summary>
        /// Returns all prestige set loaded and registered into this mod, default and mod.
        /// </summary>
        public static PrestigeSet CompleteEmptyPrestigeSet
        {
            get
            {
                var prestiges = DefaultPrestiges;
                var addedPrestiges = ModHandler.GetAddedEmptyPrestiges().ToList();
                if (addedPrestiges.Any()) prestiges.AddRange(addedPrestiges);
                return new PrestigeSet
                {
                    Prestiges = prestiges
                };

            }
        }
    }
}

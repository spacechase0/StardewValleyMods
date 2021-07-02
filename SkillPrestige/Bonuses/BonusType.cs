using System;
using System.Collections.Generic;
using System.Linq;
using SkillPrestige.Logging;
using SkillPrestige.SkillTypes;
using SkillPrestige.Bonuses.TypeRegistration;

namespace SkillPrestige.Bonuses
{
    /// <summary>
    /// Represents a bonus type in this mod, which are post-all-professions-prestiged effects the player can purchase.
    /// </summary>
    public partial class BonusType
    {
        /// <summary>
        /// Static constructor to, when the BonusType class is first called, use reflection to register all static BonusType variables declared in partial methods.
        /// </summary>
        static BonusType()
        {
            Logger.LogInformation("Registering bonus types...");
            //gets all non abstract classes that implement IBonusTypeRegistration.
            var concreteBonusTypeRegistrations = AppDomain.CurrentDomain.GetNonSystemAssemblies().SelectMany(x => x.GetTypesSafely())
                .Where(x => typeof(IBonusTypeRegistration).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract).ToList();
            Logger.LogVerbose($"{concreteBonusTypeRegistrations.Count} concrete bonus type registrations found.");
            foreach (var registration in concreteBonusTypeRegistrations)
            {
                ((IBonusTypeRegistration)Activator.CreateInstance(registration)).RegisterBonusTypes();
            }
            Logger.LogInformation("Bonus Types registered.");
        }

        /// <summary>
        /// Code used to uniquely identify a bonus type.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The maximum level available for a bonus. null means no limit.
        /// </summary>
        public int? MaxLevel { get; set; }

        /// <summary>
        /// The skill the bonus pertains to.
        /// </summary>
        public SkillType SkillType { get; set; }

        /// <summary>
        /// The display name of the bonus.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The effect descriptions to diplsay for the bonus.
        /// </summary>
        public IEnumerable<string> EffectDescriptions { get; set; }

        /// <summary>
        /// The action to apply in order to invoke the effect.
        /// </summary>
        public Action<int> ApplyEffect { get; set; }

        public static IEnumerable<BonusType> AllBonusTypes
        {
            get { return Skill.AllSkills.SelectMany(x => x.AvailableBonusTypes); }
        }
    }
}
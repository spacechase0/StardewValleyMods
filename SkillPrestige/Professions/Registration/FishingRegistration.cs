using System.Collections.Generic;
using SkillPrestige.Logging;

namespace SkillPrestige.Professions.Registration
{
    // ReSharper disable once UnusedMember.Global - created through reflection.
    public sealed class FishingRegistration : ProfessionRegistration
    {
        public override void RegisterProfessions()
        {
            Logger.LogInformation("Registering Fishing professions...");
            Fisher = new TierOneProfession
            {
                Id = 6

            };
            Trapper = new TierOneProfession
            {
                Id = 7

            };
            Angler = new TierTwoProfession
            {
                Id = 8,
                TierOneProfession = Fisher
            };
            Pirate = new TierTwoProfession
            {
                Id = 9,
                TierOneProfession = Fisher
            };
            Mariner = new TierTwoProfession
            {
                Id = 10,
                TierOneProfession = Trapper
            };
            Luremaster = new TierTwoProfession
            {
                Id = 11,
                TierOneProfession = Trapper
            };
            Fisher.TierTwoProfessions = new List<TierTwoProfession>
            {
                Angler,
                Pirate
            };
            Trapper.TierTwoProfessions = new List<TierTwoProfession>
            {
                Mariner,
                Luremaster
            };
            Logger.LogInformation("Fishing professions registered.");
        }
    }
}

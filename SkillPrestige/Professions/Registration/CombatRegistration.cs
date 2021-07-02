using System.Collections.Generic;
using SkillPrestige.Logging;

namespace SkillPrestige.Professions.Registration
{
    // ReSharper disable once UnusedMember.Global - created through reflection.
    public sealed class CombatRegistration : ProfessionRegistration
    {
        public override void RegisterProfessions()
        {
            Logger.LogInformation("Registering Combat professions...");
            Fighter = new TierOneProfession
            {
                Id = 24,
                SpecialHandling = new FighterSpecialHandling(),
            };
            Scout = new TierOneProfession
            {
                Id = 25,
            };
            Brute = new TierTwoProfession
            {
                Id = 26,
                TierOneProfession = Fighter
            };
            Defender = new TierTwoProfession
            {
                Id = 27,
                SpecialHandling = new DefenderSpecialHandling(),
                TierOneProfession = Fighter
            };
            Acrobat = new TierTwoProfession
            {
                Id = 28,
                TierOneProfession = Scout
            };
            Desperado = new TierTwoProfession
            {
                Id = 29,
                TierOneProfession = Scout
            };
            Scout.TierTwoProfessions = new List<TierTwoProfession>
            {
                Acrobat,
                Desperado
            };
            Fighter.TierTwoProfessions = new List<TierTwoProfession>
            {
                Brute,
                Defender
            };
            Logger.LogInformation("Combat professions registered.");
        }
    }
}

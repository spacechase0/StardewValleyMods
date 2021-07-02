using System;
using System.Linq;
using SkillPrestige.Logging;
using StardewValley;

namespace SkillPrestige.Commands
{
    /// <summary>
    /// A command that sets experience levels for a player.
    /// </summary>
    /// // ReSharper disable once UnusedMember.Global - referenced via reflection
    internal class SetExperienceCommand : SkillPrestigeCommand
    {

        public SetExperienceCommand() : base("player_setexperience", GetDescription()) { }

        private static string GetDescription()
        {
            var skillNames = string.Join(", ", Skill.AllSkills.Select(x => x.Type.Name));
            return "Sets the player's specified skill to the specified level of experience.\n\n"
                        + "Usage: player_setexperience <skill> <level>\n"
                        + $"- skill: the name of the skill (one of {skillNames}).\n"
                        + "- level: the target experience level.";
        }

        protected override bool TestingCommand => true;

        protected override void Apply(string[] args)
        {
            if (args.Length <= 1)
            {
                SkillPrestigeMod.LogMonitor.Log("<skill> and <value> must be specified");
                return;
            }
            var skillArgument = args[0];
            if (!Skill.AllSkills.Select(x => x.Type.Name).Contains(skillArgument, StringComparer.InvariantCultureIgnoreCase))
            {
                SkillPrestigeMod.LogMonitor.Log("<skill> is invalid");
                return;
            }
            if (!int.TryParse(args[1], out int experienceArgument))
            {
                SkillPrestigeMod.LogMonitor.Log("experience must be an integer.");
                return;
            }
            if (Game1.player == null)
            {
                SkillPrestigeMod.LogMonitor.Log("A game file must be loaded in order to run this command.");
                return;
            }
            Logger.LogInformation("Setting experience level...");
            Logger.LogVerbose($"experience argument: {experienceArgument}");
            experienceArgument = experienceArgument.Clamp(0, 100000);
            Logger.LogVerbose($"experience used: {experienceArgument}");
            var skill = Skill.AllSkills.Single(x => x.Type.Name.Equals(skillArgument, StringComparison.InvariantCultureIgnoreCase));
           
            var playerSkillExperience = Game1.player.experiencePoints[skill.Type.Ordinal];
            Logger.LogVerbose($"Current experience level for {skill.Type.Name} skill: {playerSkillExperience}");
            Logger.LogVerbose($"Setting {skill.Type.Name} skill to {experienceArgument} experience.");
            ExperienceHandler.DisableExperienceGains = true;
            skill.SetSkillExperience(experienceArgument);
            ExperienceHandler.DisableExperienceGains = false;
            var skillLevel = GetLevel(experienceArgument);
            Logger.LogVerbose($"Setting skill level for {experienceArgument} experience: {skillLevel}");
            skill.SetSkillLevel(skillLevel);
            
        }

        private static int GetLevel(int experience)
        {
            if (experience < 100) return 0;
            if (experience < 380) return 1;
            if (experience < 770) return 2;
            if (experience < 1300) return 3;
            if (experience < 2150) return 4;
            if (experience < 3300) return 5;
            if (experience < 4800) return 6;
            if (experience < 6900) return 7;
            if (experience < 10000) return 8;
            return experience < 15000 ? 9 : 10;
        }
    }
}

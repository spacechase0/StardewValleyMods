using System;
using System.Linq;
using SkillPrestige.Logging;
using StardewValley;

namespace SkillPrestige.Commands
{
    /// <summary>
    /// A command that clears all professions from a player's game.
    /// </summary>
    // ReSharper disable once UnusedMember.Global - referenced via reflection
    internal class AddProfessionCommand : SkillPrestigeCommand
    {

        public AddProfessionCommand() : base("player_addprofession", GetDescription()) { }

        protected override bool TestingCommand => false;

        protected override void Apply(string[] args)
        {
            if (args.Length < 1)
            {
                SkillPrestigeMod.LogMonitor.Log("<profession> must be specified");
                return;
            }
            var professionArgument = args[0];
            if (!Skill.AllSkills.SelectMany(x => x.Professions).Select(x => x.DisplayName).Contains(professionArgument, StringComparer.InvariantCultureIgnoreCase))
            {
                SkillPrestigeMod.LogMonitor.Log("<profession> is invalid");
                return;
            }
            if (Game1.player == null)
            {
                SkillPrestigeMod.LogMonitor.Log("A game file must be loaded in order to run this command.");
                return;
            }
            var profession = Skill.AllSkills.SelectMany(x => x.Professions).Single(x => x.DisplayName.Equals(professionArgument, StringComparison.InvariantCultureIgnoreCase));
            if (Game1.player.professions.Contains(profession.Id))
            {
                SkillPrestigeMod.LogMonitor.Log("profession already added.");
            }
            Logger.LogInformation($"Adding profession {professionArgument}...");
            Game1.player.professions.Add(profession.Id);
            profession.SpecialHandling?.ApplyEffect();
            Logger.LogInformation($"Profession {professionArgument} added.");
        }

        private static string GetDescription()
        {
            var professionNames = string.Join(", ", Skill.AllSkills.SelectMany(x => x.Professions).Select(x => x.DisplayName));
            return
                "Adds the specified profession to the player.\n\n"
                + "Usage: player_addprofession <profession>\n"
                + $"- profession: the name of the profession to add (one of {professionNames}).";
        }
    }
}

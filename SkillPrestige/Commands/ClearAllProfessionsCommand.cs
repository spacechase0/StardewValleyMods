using System;
using System.Linq;
using SkillPrestige.Logging;
using StardewValley;

namespace SkillPrestige.Commands
{
    /// <summary>
    /// A command that clears all professions from a player's game.
    /// </summary>
    /// // ReSharper disable once UnusedMember.Global - referenced via reflection
    internal class ClearAllProfessionsCommand : SkillPrestigeCommand
    {

        public ClearAllProfessionsCommand() : base("player_clearallprofessions", "Removes all professions for the current game file.\n\nUsage: player_clearallprofessions\n") { }

        protected override bool TestingCommand => false;

        protected override void Apply(string[] args)
        {
            if (Game1.player == null)
            {
                SkillPrestigeMod.LogMonitor.Log("A game file must be loaded in order to run this command.");
                return;
            }
            SkillPrestigeMod.LogMonitor.Log("This command will remove all of your character's professions. " + Environment.NewLine +
                       "If you have read this and wish to continue confirm with 'y' or 'yes'");
            var response = Console.ReadLine();
            if (response == null ||
                !response.Equals("y", StringComparison.InvariantCultureIgnoreCase) &&
                !response.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                Logger.LogVerbose("Cancelled clear all professions..");
                return;
            }
            Logger.LogVerbose("Clearing all professions...");

            var specialHandlingsForSkillsRemoved =
                Skill.AllSkills.SelectMany(x => x.Professions)
                    .Where(x => Game1.player.professions.Contains(x.Id) && x.SpecialHandling != null)
                    .Select(x => x.SpecialHandling);

            Game1.player.professions.Clear();
            foreach (var specialHandling in specialHandlingsForSkillsRemoved)
            {
                specialHandling.RemoveEffect();
            }
            Logger.LogVerbose("Professions cleared.");
        }
    }
}

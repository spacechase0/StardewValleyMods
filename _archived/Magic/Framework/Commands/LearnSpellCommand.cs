using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Magic.Framework.Spells;
using SpaceShared;
using SpaceShared.ConsoleCommands;
using StardewModdingAPI;
using StardewValley;

namespace Magic.Framework.Commands
{
    /// <summary>A command which causes the player to learn a spell.</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = DiagnosticMessages.IsUsedViaReflection)]
    internal class LearnSpellCommand : ConsoleCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public LearnSpellCommand()
            : base("player_learnspell", LearnSpellCommand.BuildDescription()) { }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, string[] args)
        {
            SpellBook spellBook = Game1.player.GetSpellBook();

            if (args.Length == 1 && args[0] == "all")
            {
                foreach (string spellName in SpellManager.GetAll())
                {
                    var curSpell = SpellManager.Get(spellName);
                    spellBook.LearnSpell(curSpell, curSpell.GetMaxCastingLevel(), true);
                }

                return;
            }

            if (args.Length != 2 || (args.Length > 1 && (args[0] == "" || args[1] == "")))
            {
                Log.Info("Usage: player_learnspell <spell> <level>");
                return;
            }

            Spell spell = SpellManager.Get(args[0]);
            if (spell == null)
            {
                Log.Error($"Spell '{args[0]}' does not exist.");
                return;
            }

            if (!int.TryParse(args[1], out int level))
            {
                Log.Error($"That spell only casts up to level {spell.GetMaxCastingLevel()}.");
                return;
            }

            spellBook.LearnSpell(spell, level, true);
        }


        /*********
        ** Private methods
        *********/
        private static string BuildDescription()
        {
            string[] spellIds = SpellManager.GetAll().OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
            return
                "Immediately learn a spell or spell level without using any spell points.\n\n"
                + "Usage:\n"
                + "    player_learnspell <spell> <level>\n"
                + $"    - spell: the spell to learn. This must be an internal ID like '{string.Join("', '", spellIds)}'.\n"
                + $"    - level: the spell level to learn, usually a number between 1 and 3.";
        }
    }
}

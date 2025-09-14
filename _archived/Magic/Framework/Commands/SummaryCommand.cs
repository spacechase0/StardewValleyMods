using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Magic.Framework.Spells;
using SpaceCore;
using SpaceShared;
using SpaceShared.ConsoleCommands;
using StardewModdingAPI;
using StardewValley;

namespace Magic.Framework.Commands
{
    /// <summary>A command which causes the player to learn a spell.</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = DiagnosticMessages.IsUsedViaReflection)]
    internal class SummaryCommand : ConsoleCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SummaryCommand()
            : base("magic_summary", "Displays metadata about the current player's magic for troubleshooting.\n\nUsage:\n    magic_summary") { }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                monitor.Log("You must load a save to use this command.", LogLevel.Warn);
                return;
            }

            StringBuilder report = new();
            var player = Game1.player;
            var spellBook = player.GetSpellBook();

            // general data
            report.AppendLine();
            report.AppendLine($"Magic info for {this.GetPlayerContext(player)}:");
            report.AppendLine($"   Learned magic: {Magic.LearnedMagic}");
            report.AppendLine($"   Current mana:  {player.GetCurrentMana()} / {player.GetMaxMana()}");
            report.AppendLine($"   Unused points: {spellBook.FreePoints}");
            report.AppendLine();

            // professions
            report.AppendLine("Professions:");
            {
                void PrintProfession(Skills.Skill.Profession profession, bool indent)
                {
                    bool hasProfession = Game1.player.HasCustomProfession(profession);
                    report.AppendLine($"   {(indent ? "   " : "")}[{(hasProfession ? "X" : " ")}] {profession.GetName()} ({profession.GetDescription()})");
                }

                foreach (var group in Magic.Skill.ProfessionsForLevels.Where(p => p.Requires != null))
                {
                    PrintProfession(group.Requires, indent: false);
                    PrintProfession(group.First, indent: true);
                    PrintProfession(group.Second, indent: true);
                }
            }
            report.AppendLine();

            // known spells
            report.AppendLine("Known spells:");
            foreach (SpellInfo spell in this.GetKnownSpells(spellBook).OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
            {
                report.AppendLine(spell.KnownLevel != null
                    ? $"   [X] {spell.Name} (level {spell.KnownLevel} of {spell.MaxLevel})"
                    : $"   [ ] {spell.Name}"
                );
            }
            report.AppendLine();

            // prepared spells
            report.AppendLine("Prepared spells:");
            if (spellBook.Prepared.Any())
            {
                for (int barIndex = 0; barIndex < spellBook.Prepared.Count; barIndex++)
                {
                    var bar = spellBook.Prepared[barIndex];

                    report.AppendLine($"   Spellbar #{barIndex + 1}:");
                    if (bar.Spells.Any(p => p != null))
                    {
                        for (int spellIndex = 0; spellIndex < bar.Spells.Count; spellIndex++)
                        {
                            var knownSpell = bar.Spells[spellIndex];

                            if (knownSpell == null)
                                report.AppendLine($"      {spellIndex + 1}. empty");
                            else
                            {
                                var spell = SpellManager.Get(knownSpell.SpellId);
                                report.AppendLine($"      {spellIndex + 1}. {spell.GetTranslatedName()} (level {knownSpell.Level})");
                            }
                        }
                    }
                    else
                        report.AppendLine("      No spells prepared.");
                    report.AppendLine();
                }
            }
            else
                report.AppendLine("   No prepared spells.");
            report.AppendLine();

            // log result
            monitor.Log(report.ToString(), LogLevel.Info);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a summary of the player context.</summary>
        /// <param name="player">The current player instance.</param>
        private string GetPlayerContext(Farmer player)
        {
            string result = $"{player.Name} (";
            if (!Context.IsMultiplayer)
                result += "single-player";
            else
                result += $"{(Context.IsMainPlayer ? "Main player" : "Farmhand")} with {Game1.getOnlineFarmers().Count} players connected";

            result += ")";

            return result;
        }

        /// <summary>Get the names of every valid spell and the level learned by the player.</summary>
        /// <param name="spellBook">The player's current spell book.</param>
        private IEnumerable<SpellInfo> GetKnownSpells(SpellBook spellBook)
        {
            foreach (string spellId in SpellManager.GetAll())
            {
                Spell spell = SpellManager.Get(spellId);

                if (!spellBook.KnownSpells.TryGetValue(spellId, out PreparedSpell knownSpell))
                    knownSpell = null;

                yield return new(spell, knownSpell?.Level);
            }
        }

        /// <summary>Display metadata for a spell.</summary>
        private class SpellInfo
        {
            /*********
            ** Accessors
            *********/
            /// <summary>The general spell info.</summary>
            public Spell Spell { get; }

            /// <summary>The spell's translated name.</summary>
            public string Name { get; }

            /// <summary>The display level known by the player, or <c>null</c> if the player doesn't know it.</summary>
            public int? KnownLevel { get; }

            /// <summary>The spell's maximum display level.</summary>
            public int MaxLevel { get; }


            /*********
            ** Public methods
            *********/
            /// <summary>Construct an instance.</summary>
            /// <param name="spell"></param>
            /// <param name="knownLevel"></param>
            public SpellInfo(Spell spell, int? knownLevel)
            {
                this.Spell = spell;
                this.KnownLevel = knownLevel + 1; // zero-indexed
                this.Name = spell.GetTranslatedName();
                this.MaxLevel = spell.GetMaxCastingLevel(); // one-indexed
            }
        }
    }
}

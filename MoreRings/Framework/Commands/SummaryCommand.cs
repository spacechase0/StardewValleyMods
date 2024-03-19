using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using SpaceShared;
using SpaceShared.ConsoleCommands;
using StardewModdingAPI;
using StardewValley;

namespace MoreRings.Framework.Commands
{
    /// <summary>A command which prints a summary of the More Rings state.</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = DiagnosticMessages.IsUsedViaReflection)]
    internal class SummaryCommand : ConsoleCommand
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SummaryCommand()
            : base("more_rings_summary", "Displays metadata about the current player's rings for troubleshooting.\n\nUsage:\n    more_rings_summary") { }

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
            var mod = Mod.Instance;

            // collect rings
            var rings = new Dictionary<string, string>
            {
                [mod.RingFishingLargeBar] = I18n.RingOfWideNets_Name(),
                [mod.RingCombatRegen] = I18n.RingOfRegeneration_Name(),
                [mod.RingDiamondBooze] = I18n.RingOfDiamondBooze_Name(),
                [mod.RingRefresh] = I18n.RefreshingRing_Name(),
                [mod.RingQuality] = I18n.QualityRing_Name(),
                [mod.RingMageHand] = I18n.RingOfFarReaching_Name(),
                [mod.RingTrueSight] = I18n.RingOfTrueSight_Name()
            };

            // general data
            report.AppendLine();
            report.AppendLine($"Showing More Rings info for {this.GetPlayerContext(player)}.");
            report.AppendLine();

            report.AppendLine("Mod integrations:");
            report.AppendLine($"   [{(Mod.Instance.HasWearMoreRings ? "X": " ")}] Wear More Rings");
            report.AppendLine();

            report.AppendLine("Equipped rings:");
            foreach (var entry in rings.OrderBy(p => p.Value, StringComparer.OrdinalIgnoreCase))
            {
                string name = entry.Value;
                int equipped = mod.CountRingsEquipped(entry.Key);

                report.AppendLine($"   [{(equipped > 0 ? "X" : " ")}] {name}{(equipped > 0 ? $" ({equipped} equipped)" : "")}");
            }

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
    }
}

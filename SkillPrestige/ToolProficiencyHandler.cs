using System.Collections.Generic;
using System.Linq;
using SkillPrestige.Logging;
using StardewValley;

namespace SkillPrestige
{
    /// <summary>
    /// Makes any necessary adjustments to the game's strategy for determining tool proficency.
    /// </summary>
    public static class ToolProficiencyHandler
    {
        public static IDictionary<ToolType, int> AddedToolProficencies { get; } = new Dictionary<ToolType, int>();

        private static bool AwaitingToolRelease { get; set; }
        private static bool ToolJustReleased { get; set; }
        private static bool ToolJustHeld { get; set; }
        private static ToolType? CurrentTool { get; set; }
        private static float StaminaBeforeToolUse { get; set; }

        public static void HandleToolProficiency()
        {
            CheckCurrentTool();
            if (!CurrentTool.HasValue) return;
            CheckForToolUsage();
            if (ToolJustHeld) AdjustTemporaryToolProfiency();
            if (ToolJustReleased) MakeFinalToolProciencyAdjustments();
        }

        private static void CheckCurrentTool()
        {
            var tool = Game1.player.CurrentTool;
            if (tool == null)
            {
                CurrentTool = null;
                return;
            }
            foreach (var lookup in ToolTypeLookup.ToolTypes)
            {
                if (!lookup.Value.IsInstanceOfType(tool)) continue;
                CurrentTool = lookup.Key;
                return;
            }
            CurrentTool = null;
        }

        private static void CheckForToolUsage()
        {

            if (Game1.player.UsingTool)
            {
                if (!AwaitingToolRelease)
                {
                    AwaitingToolRelease = true;
                    ToolJustHeld = true;
                    ToolJustReleased = false;
                }
                else
                {
                    ToolJustHeld = false;
                    ToolJustReleased = false;
                }
            }
            else
            {
                if (AwaitingToolRelease)
                {
                    AwaitingToolRelease = false;
                    ToolJustReleased = true;
                }
                else
                {
                    ToolJustReleased = false;
                }
            }
        }

        private static void AdjustTemporaryToolProfiency()
        {
            var addedProficiency = AddedToolProficencies.SingleOrDefault(x => x.Key == CurrentTool).Value;
            if (addedProficiency <= 0) return;
            Logger.LogVerbose(
                $"Adding {addedProficiency} temporary levels to {ToolTypeLookup.ToolTypes.Single(x => x.Key == CurrentTool).Value} skill.");
            StaminaBeforeToolUse = Game1.player.Stamina;
            switch (CurrentTool)
            {
                case ToolType.Hoe:
                case ToolType.WateringCan:
                    Game1.player.addedFarmingLevel.Value += addedProficiency;
                    break;
                case ToolType.FishingRod:
                    Game1.player.addedFishingLevel.Value += addedProficiency;
                    break;
                case ToolType.Axe:
                    Game1.player.addedForagingLevel.Value += addedProficiency;
                    break;
                case ToolType.Pickaxe:
                    Game1.player.addedMiningLevel.Value += addedProficiency;
                    break;
                case null:
                    Logger.LogVerbose("Tool is not recognized for profieciency.");
                    break;
                default:
                    Logger.LogWarning("Unknown tool type proficiency detected, no proficiency adjusted.");
                    break;
            }
        }

        private static void MakeFinalToolProciencyAdjustments()
        {
            var addedProficiency = AddedToolProficencies.SingleOrDefault(x => x.Key == CurrentTool).Value;
            if (addedProficiency <= 0) return;
            Logger.LogVerbose(
                $"Removing {addedProficiency} temporary levels from {ToolTypeLookup.ToolTypes.Single(x => x.Key == CurrentTool).Value} skill.");
            switch (CurrentTool)
            {
                case ToolType.Hoe:
                case ToolType.WateringCan:
                    Game1.player.addedFarmingLevel.Value -= addedProficiency;
                    break;
                case ToolType.FishingRod:
                    Game1.player.addedFishingLevel.Value -= addedProficiency;
                    break;
                case ToolType.Axe:
                    Game1.player.addedForagingLevel.Value -= addedProficiency;
                    break;
                case ToolType.Pickaxe:
                    Game1.player.addedMiningLevel.Value -= addedProficiency;
                    break;
                case null:
                    Logger.LogVerbose("Tool is not recognized for proficiency.");
                    break;
                default:
                    Logger.LogWarning("Unknown tool type proficiency detected, no proficiency adjusted.");
                    break;
            }
            if (!(Game1.player.Stamina >= StaminaBeforeToolUse)) return;
            Logger.LogVerbose("Added proficiency would have resulted in 0 or negative energy consumption, consuming a single energy point by default.");
            Game1.player.Stamina = StaminaBeforeToolUse--;
            StaminaBeforeToolUse = 0;
        }

    }
}
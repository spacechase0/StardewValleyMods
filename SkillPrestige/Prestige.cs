using System;
using System.Collections.Generic;
using System.Linq;
using SkillPrestige.Bonuses;
using SkillPrestige.Logging;
using SkillPrestige.Professions;
using SkillPrestige.SkillTypes;
using StardewValley;

namespace SkillPrestige
{
    /// <summary>
    /// Represents a prestige for a skill.
    /// </summary>
    [Serializable]
    public class Prestige
    {
        public Prestige()
        {
            PrestigeProfessionsSelected = new List<int>();
        }

        /// <summary>
        /// The skill the prestige is for.
        /// </summary>
        public SkillType SkillType { get; set; }

        /// <summary>
        /// The total available prestige points, one is gained per skill reset.
        /// </summary>
        public int PrestigePoints { get; set; }

        /// <summary>
        /// Professions that have been chosen to be permanent using skill points.
        /// </summary>
        public IList<int> PrestigeProfessionsSelected { get; set; }

        /// <summary>
        /// Bonuses that have been purchased for this prestige.
        /// </summary>
        public IList<Bonus> Bonuses { get; set; }

        /// <summary>
        /// Purchases a profession to be part of the prestige set.
        /// </summary>
        /// <param name="professionId"></param>
        public static void AddPrestigeProfession(int professionId)
        {
            var skill = Skill.AllSkills.Single(x => x.Professions.Select(y => y.Id).Contains(professionId));
            var prestige = PrestigeSaveData.CurrentlyLoadedPrestigeSet.Prestiges.Single(x => x.SkillType == skill.Type);
            var originalPrestigePointsForSkill = prestige.PrestigePoints;
            if (skill.Professions.Where(x => x.LevelAvailableAt == 5).Select(x => x.Id).Contains(professionId))
            {
                prestige.PrestigePoints -= PerSaveOptions.Instance.CostOfTierOnePrestige;
                Logger.LogInformation($"Spent prestige point on {skill.Type.Name} skill.");
            }

            else if (skill.Professions.Where(x => x.LevelAvailableAt == 10).Select(x => x.Id).Contains(professionId))
            {
                prestige.PrestigePoints -= PerSaveOptions.Instance.CostOfTierTwoPrestige;
                Logger.LogInformation($"Spent 2 prestige points on {skill.Type.Name} skill.");
            }
            else
                Logger.LogError($"No skill found for selected profession: {professionId}");
            if (prestige.PrestigePoints < 0)
            {
                prestige.PrestigePoints = originalPrestigePointsForSkill;
                Logger.LogCritical($"Prestige amount for {skill.Type.Name} skill would have gone negative, unable to grant profession {professionId}. Prestige values reset.");
            }
            else
            {
                prestige.PrestigeProfessionsSelected.Add(professionId);
                Logger.LogInformation("Profession permanently added.");
                Profession.AddMissingProfessions();
            }
        }

        /// <summary>
        /// Prestiges a skill, resetting it to level 0, removing all recipes and effects of the skill at higher levels
        /// and grants one prestige point in that skill to the player.
        /// </summary>
        /// <param name="skill">the skill you wish to prestige.</param>
        public static void PrestigeSkill(Skill skill)
        {
            try
            {
                if (PerSaveOptions.Instance.PainlessPrestigeMode)
                {
                    Logger.LogInformation($"Prestiging skill {skill.Type.Name} via Painless Mode.");
                    skill.SetSkillExperience(Game1.player.experiencePoints[skill.Type.Ordinal] - PerSaveOptions.Instance.ExperienceNeededPerPainlessPrestige);
                    Logger.LogInformation($"Removed {PerSaveOptions.Instance.ExperienceNeededPerPainlessPrestige} experience points from {skill.Type.Name} skill.");
                }
                else
                {
                    Logger.LogInformation($"Prestiging skill {skill.Type.Name}.");
                    skill.OnPrestige?.Invoke();
                    skill.SetSkillExperience(0);
                    skill.SetSkillLevel(0);
                    Logger.LogInformation($"Skill {skill.Type.Name} experience and level reset.");
                    if (PerSaveOptions.Instance.ResetRecipesOnPrestige)
                    {
                        RemovePlayerCraftingRecipesForSkill(skill.Type);
                        RemovePlayerCookingRecipesForSkill(skill.Type);
                    }
                    Profession.RemoveProfessions(skill);
                    PlayerManager.CorrectStats(skill);
                    Profession.AddMissingProfessions();
                }
                PrestigeSaveData.CurrentlyLoadedPrestigeSet.Prestiges.Single(x => x.SkillType == skill.Type).PrestigePoints += PerSaveOptions.Instance.PointsPerPrestige;
                Logger.LogInformation($"{PerSaveOptions.Instance.PointsPerPrestige} Prestige point(s) added to {skill.Type.Name} skill.");

            }
            catch (Exception exception)
            {
                Logger.LogError(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        /// <summary>
        /// Removes all crafting recipes granted by levelling  a skill.
        /// </summary>
        /// <param name="skillType">the skill type to remove all crafting recipes from.</param>
        private static void RemovePlayerCraftingRecipesForSkill(SkillType skillType)
        {
            Logger.LogInformation($"Removing {skillType.Name} crafting recipes");
            foreach (
                var recipe in
                CraftingRecipe.craftingRecipes.Where(
                    x =>
                        x.Value.Split('/')[4].Contains(skillType.Name) &&
                        Game1.player.craftingRecipes.ContainsKey(x.Key)))
            {
                Logger.LogVerbose($"Removing {skillType.Name} crafting recipe {recipe.Value}");
                Game1.player.craftingRecipes.Remove(recipe.Key);
            }
            Logger.LogInformation($"{skillType.Name} crafting recipes removed.");

        }

        /// <summary>
        /// Removes all cooking recipes granted by levelling  a skill.
        /// </summary>
        /// <param name="skillType">the skill type to remove all cooking recipes from.</param>
        private static void RemovePlayerCookingRecipesForSkill(SkillType skillType)
        {
            if (skillType.Name.IsOneOf("Cooking", string.Empty))
            {
                Logger.LogInformation($"Wiping skill cooking recipes for skill: {skillType.Name} could remove more than intended. Exiting skill cooking recipe wipe.");
                return;
            }
            Logger.LogInformation($"Removing {skillType.Name} cooking recipes.");
            foreach (
                var recipe in
                CraftingRecipe.cookingRecipes.Where(
                    x =>
                        x.Value.Split('/')[3].Contains(skillType.Name) &&
                        Game1.player.cookingRecipes.ContainsKey(x.Key)))
            {
                Logger.LogVerbose($"Removing {skillType.Name} cooking recipe {recipe.Value}");
                Game1.player.cookingRecipes.Remove(recipe.Key);
            }
            Logger.LogInformation($"{skillType.Name} cooking recipes removed.");
        }
    }
}

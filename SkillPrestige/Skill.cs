using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkillPrestige.Bonuses;
using SkillPrestige.Menus;
using SkillPrestige.Mods;
using SkillPrestige.Professions;
using SkillPrestige.SkillTypes;
using StardewValley;
using StardewValley.Menus;

namespace SkillPrestige
{
    /// <summary>
    /// Represents a skill in Stardew Valley.
    /// </summary>
    public class Skill
    {
        public Skill()
        {
            SetSkillExperience = x =>
            {
                Game1.player.experiencePoints[Type.Ordinal] = 0;
                Game1.player.gainExperience(Type.Ordinal, x);
            };
            LevelUpManager = new LevelUpManager
            {
                IsMenu = menu => menu.GetType() == typeof(LevelUpMenu),
                GetLevel = () => (int)(Game1.activeClickableMenu as LevelUpMenu).GetInstanceField("currentLevel"),
                GetSkill = () => AllSkills.Single(y => y.Type.Ordinal == (int?)(Game1.activeClickableMenu as LevelUpMenu)?.GetInstanceField("currentSkill")),
                CreateNewLevelUpMenu = (skill, level) => new LevelUpMenuDecorator<LevelUpMenu>(skill, level, new LevelUpMenu(skill.Type.Ordinal, level),
                "professionsToChoose", "leftProfessionDescription", "rightProfessionDescription", LevelUpMenu.getProfessionDescription)
            };
        }

        public SkillType Type { get; set; }

        public IEnumerable<Profession> Professions { get; set; }

        /// <summary>
        /// The location of the texture on the buffsIcons sprite sheet.
        /// </summary>
        public Rectangle SourceRectangleForSkillIcon { get; set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public Texture2D SkillIconTexture { get; set; } = Game1.buffsIcons;

        /// <summary>
        /// The 1-based index of where the skill appears on the screen. (e.g. Farming is 1, Fishing is 4, Luck, if added, is 6.)
        /// If your mod is creating a skill, you will need to detect where in the list your mod's skill(s) will be.
        /// </summary>
        public int SkillScreenPosition { get; set; }

        public IEnumerable<int> GetAllProfessionIds()
        {
            return Professions.Select(x => x.Id);
        }

        /// <summary>
        /// An action to set the skill's level. For the unmodded game (and the luck mod) 
        /// it is setting the skill level on the farmer object in the base game. (e.g. Game1.player.farmingLevel) 
        /// If you are implementing this class for your mod it should be whatever would be needed to set the skill level to a given integer.
        /// </summary>
        public Action<int> SetSkillLevel;

        /// <summary>
        /// A function to return the skill's level. For the unmodded game (and the luck mod)
        /// it is returning the skill level on the farmer object in the base game. (e.g. Game1.player.farmingLevel) 
        /// If you are implementing this class for your mod it should be whatever would be needed to retrieve the player's current skill level.
        /// </summary>
        public Func<int> GetSkillLevel;

        /// <summary>
        /// An action to set the skill's experience. For the unmodded game (and the luck mod) 
        /// it is setting the experience on the farmer object in the base game based upon a given array index. (e.g. Game1.player.experiencePoints[0]) 
        /// If you are implementing this class for your mod it should be whatever would be needed to set the skill experience level to a given integer.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global used by other mods.
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public Action<int> SetSkillExperience { get; set; }

        /// <summary>
        /// An action triggered when prestiging is done. This allows extra handling if something else needs to be reset.
        /// </summary>
        public Action OnPrestige { get; set; }

        /// <summary>
        /// The management class for any level up menu.
        /// </summary>
        public LevelUpManager LevelUpManager { get; set; }

        /// <summary>
        /// The types of bonuses available with this skill.
        /// </summary>
        public IEnumerable<BonusType> AvailableBonusTypes { get; set; }

        /// <summary>
        /// The default skills available in the unmodded game.
        /// </summary>
        public static IEnumerable<Skill> DefaultSkills => new List<Skill>
        {
            new Skill
            {
                Type = SkillType.Farming,
                SkillScreenPosition = 1,
                SourceRectangleForSkillIcon = new Rectangle(0, 0, 16, 16),
                Professions = Profession.FarmingProfessions,
                SetSkillLevel = x => Game1.player.farmingLevel.Value = x,
                GetSkillLevel = () => Game1.player.farmingLevel.Value,
                AvailableBonusTypes = BonusType.FarmingBonusTypes
            },
            new Skill
            {
                Type = SkillType.Fishing,
                SkillScreenPosition = 4,
                SourceRectangleForSkillIcon = new Rectangle(16, 0, 16, 16),
                Professions = Profession.FishingProfessions,
                SetSkillLevel = x => Game1.player.fishingLevel.Value = x,
                GetSkillLevel = () => Game1.player.fishingLevel.Value
            },
            new Skill
            {
                Type = SkillType.Foraging,
                SkillScreenPosition = 3,
                SourceRectangleForSkillIcon = new Rectangle(80, 0, 16, 16),
                Professions = Profession.ForagingProfessions,
                SetSkillLevel = x => Game1.player.foragingLevel.Value = x,
                GetSkillLevel = () => Game1.player.foragingLevel.Value
            },
            new Skill
            {
                Type = SkillType.Mining,
                SkillScreenPosition = 2,
                SourceRectangleForSkillIcon = new Rectangle(32, 0, 16, 16),
                Professions = Profession.MiningProfessions,
                SetSkillLevel = x => Game1.player.miningLevel.Value = x,
                GetSkillLevel = () => Game1.player.miningLevel.Value
            },
            new Skill
            {
                Type = SkillType.Combat,
                SkillScreenPosition = 5,
                SourceRectangleForSkillIcon = new Rectangle(128, 16, 16, 16),
                Professions = Profession.CombatProfessions,
                SetSkillLevel = x => Game1.player.combatLevel.Value = x,
                GetSkillLevel = () => Game1.player.combatLevel.Value
            }
        };

        /// <summary>
        /// Returns all skills loaded and registered into this mod, default and mod.
        /// </summary>
        public static IEnumerable<Skill> AllSkills
        {
            get
            {
                var skills = new List<Skill>(DefaultSkills);
                var addedSkills = ModHandler.GetAddedSkills().ToList();
                if (addedSkills.Any()) skills.AddRange(addedSkills);
                return skills;
            }
        }
    }
}

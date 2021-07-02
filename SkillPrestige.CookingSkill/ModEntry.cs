using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkillPrestige.Menus;
using SkillPrestige.Mods;
using SkillPrestige.Professions;
using SkillPrestige.SkillTypes;
using SpaceCore;
using SpaceCore.Interface;
using StardewModdingAPI;
using StardewValley;

namespace SkillPrestige.CookingSkill
{
    /// <summary>The mod entry class.</summary>
    public class ModEntry : Mod, ISkillMod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The cooking skill icon.</summary>
        private Texture2D IconTexture;

        /// <summary>The cooking skill type.</summary>
        private SkillType CookingSkillType;

        /// <summary>Whether the Luck Skill mod is loaded.</summary>
        private bool IsLuckSkillModLoaded;


        /*********
        ** Accessors
        *********/
        /// <summary>The name to display for the mod in the log.</summary>
        public string DisplayName { get; } = "Cooking Skill";

        /// <summary>Whether the mod is found in SMAPI.</summary>
        public bool IsFound { get; private set; }

        /// <summary>The skills added by this mod.</summary>
        public IEnumerable<Skill> AdditionalSkills => this.GetAddedSkills();

        /// <summary>The prestiges added by this mod.</summary>
        public IEnumerable<Prestige> AdditonalPrestiges => this.GetAddedPrestiges();


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.IconTexture = helper.Content.Load<Texture2D>("icon.png");
            this.CookingSkillType = new SkillType("Cooking", 6);
            this.IsFound = helper.ModRegistry.IsLoaded("spacechase0.LuckSkill");
            this.IsLuckSkillModLoaded = helper.ModRegistry.IsLoaded("alphablackwolf.LuckSkillPrestigeAdapter");

            ModHandler.RegisterMod(this);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the skills added by this mod.</summary>
        private IEnumerable<Skill> GetAddedSkills()
        {
            if (!this.IsFound)
                yield break;

            yield return new Skill
            {
                Type = this.CookingSkillType,
                SkillScreenPosition = this.IsLuckSkillModLoaded ? 7 : 6, // fix potential conflict with order due to luck skill mod
                SourceRectangleForSkillIcon = new Rectangle(0, 0, 16, 16),
                SkillIconTexture = this.IconTexture,
                Professions = this.GetAddedProfessions(),
                SetSkillLevel = x => { }, // no set necessary, as the level isn't stored independently from the experience
                GetSkillLevel = this.GetCookingLevel,
                SetSkillExperience = SetCookingExperience,
                LevelUpManager = new LevelUpManager
                {
                    IsMenu = menu => menu is SkillLevelUpMenu && Helper.Reflection.GetField<string>(menu, "currentSkill").GetValue() == "spacechase0.Cooking",
                    GetLevel = () => Game1.player.GetCustomSkillLevel(SpaceCore.Skills.GetSkill("spacechase0.Cooking")),
                    GetSkill = () => Skill.AllSkills.Single(x => x.Type == this.CookingSkillType),
                    CreateNewLevelUpMenu = (skill, level) => new LevelUpMenuDecorator<SkillLevelUpMenu>(skill, level, new SkillLevelUpMenu("spacechase0.Cooking", level),
                        "professionsToChoose", "leftProfessionDescription", "rightProfessionDescription", SkillLevelUpMenu.getProfessionDescription)
                }
            };
        }

        /// <summary>Get the prestiges added by this mod.</summary>
        private IEnumerable<Prestige> GetAddedPrestiges()
        {
            if (!this.IsFound)
                yield break;

            yield return new Prestige
            {
                SkillType = this.CookingSkillType
            };
        }

        /// <summary>Get the professions added by this mod.</summary>
        private IEnumerable<Profession> GetAddedProfessions()
        {
            var gourmet = new TierOneProfession
            {
                Id = 50,
                DisplayName = "Gourmet",
                EffectText = new[] { "+20% sell price" }
            };
            var satisfying = new TierOneProfession
            {
                Id = 51,
                DisplayName = "Satisfying",
                EffectText = new[] { "+25% buff duration once eaten" }
            };
            var efficient = new TierTwoProfession
            {
                Id = 52,
                DisplayName = "Efficient",
                EffectText = new[] { "15% chance to not consume ingredients" },
                TierOneProfession = gourmet
            };
            var professionalChef = new TierTwoProfession
            {
                Id = 53,
                DisplayName = "Prof. Chef",
                EffectText = new[] { "Home-cooked meals are always at least silver" },
                TierOneProfession = gourmet
            };
            var intenseFlavors = new TierTwoProfession
            {
                Id = 54,
                DisplayName = "Intense Flavors",
                EffectText = new[]
                {
                    "Food buffs are one level stronger",
                    "(+20% for max energy or magnetism)"
                },
                TierOneProfession = satisfying
            };
            var secretSpices = new TierTwoProfession
            {
                Id = 55,
                DisplayName = "Secret Spices",
                EffectText = new[] { "Provides a few random buffs when eating unbuffed food." },
                TierOneProfession = satisfying
            };
            gourmet.TierTwoProfessions = new List<TierTwoProfession>
            {
                efficient,
                professionalChef
            };
            satisfying.TierTwoProfessions = new List<TierTwoProfession>
            {
                intenseFlavors,
                secretSpices
            };
            return new Profession[]
            {
                gourmet,
                satisfying,
                efficient,
                professionalChef,
                intenseFlavors,
                secretSpices
            };
        }

        /// <summary>Get the current cooking skill level.</summary>
        private int GetCookingLevel()
        {
            //this.FixExpLength();
            return Game1.player.GetCustomSkillLevel("spacechase0.Cooking");
        }

        /// <summary>Set the current cooking skill XP.</summary>
        /// <param name="amount">The amount to set.</param>
        private static void SetCookingExperience(int amount)
        {
            int addedExperience = amount - Game1.player.GetCustomSkillExperience("spacechase0.Cooking");
            Game1.player.AddCustomSkillExperience("spacechase0.Cooking", addedExperience);
        }
    }
}

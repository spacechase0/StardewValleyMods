using System.Collections.Generic;
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

namespace SkillPrestige.LoveOfCooking
{
    // ReSharper disable once UnusedType.Global
    internal class ModEntry : Mod, ISkillMod
    {
        private const string SpaceCoreSkillId = "blueberry.LoveOfCooking.CookingSkill";

        private const string TargetModId = "blueberry.LoveOfCooking";

        private SkillType CookingSkillType;

        private Texture2D IconTexture;

        public string DisplayName { get; } = "LoveOfCooking Skill";

        public bool IsFound { get; private set; }

        public IEnumerable<Skill> AdditionalSkills => this.GetAddedSkills();

        public IEnumerable<Prestige> AdditonalPrestiges => this.GetAddedPrestiges();


        public override void Entry(IModHelper helper)
        {
            this.IconTexture = helper.Content.Load<Texture2D>("icon.png");
            this.CookingSkillType = new SkillType("Cooking", 6);
            this.IsFound = helper.ModRegistry.IsLoaded(ModEntry.TargetModId);
            ModHandler.RegisterMod(this);
        }

        private IEnumerable<Skill> GetAddedSkills()
        {
            if (!this.IsFound)
                yield break;

            yield return new Skill
            {
                Type = this.CookingSkillType,
                SkillScreenPosition = 8,
                SourceRectangleForSkillIcon = new Rectangle(0, 0, 16, 16),
                SkillIconTexture = this.IconTexture,
                Professions = this.GetAddedProfessions(),
                GetSkillLevel = this.GetLevel,
                SetSkillLevel = level =>
                {
                }, // no set necessary, as the level isn't stored independently from the experience
                SetSkillExperience = ModEntry.SetExperience,
                LevelUpManager = new LevelUpManager
                {
                    IsMenu = menu =>
                        menu is SkillLevelUpMenu &&
                        this.Helper.Reflection.GetField<string>(menu, "currentSkill").GetValue() ==
                        ModEntry.SpaceCoreSkillId,
                    GetLevel = () => Game1.player.GetCustomSkillLevel(Skills.GetSkill(ModEntry.SpaceCoreSkillId)),
                    CreateNewLevelUpMenu = (skill, level) => new LevelUpMenuDecorator<SkillLevelUpMenu>(
                        skill,
                        level,
                        new SkillLevelUpMenu(ModEntry.SpaceCoreSkillId, level),
                        "professionsToChoose",
                        "leftProfessionDescription",
                        "rightProfessionDescription",
                        SkillLevelUpMenu.getProfessionDescription
                    )
                }
            };
        }

        private IEnumerable<Prestige> GetAddedPrestiges()
        {
            if (!this.IsFound)
                yield break;

            yield return new Prestige
            {
                SkillType = this.CookingSkillType
            };
        }

        private IEnumerable<Profession> GetAddedProfessions()
        {
            var sousChef = new TierOneProfession
            {
                Id = 50,
                DisplayName = "Sous Chef",
                EffectText = new[] {"Cooking oil further improves the quality of recipes."}
            };
            var quickEater = new TierOneProfession
            {
                Id = 51,
                DisplayName = "Quick Eater",
                EffectText = new[] {"Food restores your energy more quickly."}
            };
            var headChef = new TierTwoProfession
            {
                Id = 52,
                DisplayName = "Head Chef",
                EffectText = new[] {"Cooked foods given as gifts befriend quicker."},
                TierOneProfession = sousChef
            };
            var fiveStarCook = new TierTwoProfession
            {
                Id = 53,
                DisplayName = "Five Star Cook",
                EffectText = new[] {"Cooked foods worth 30% more."},
                TierOneProfession = sousChef
            };
            var gourmet = new TierTwoProfession
            {
                Id = 54,
                DisplayName = "Gourmet",
                EffectText = new[] {"Chance to craft an extra portion when cooking."},
                TierOneProfession = quickEater
            };
            var glutton = new TierTwoProfession
            {
                Id = 55,
                DisplayName = "Glutton",
                EffectText = new[] {"Buff duration from food and drinks increased when at full energy."},
                TierOneProfession = quickEater
            };
            sousChef.TierTwoProfessions = new List<TierTwoProfession>
            {
                headChef,
                fiveStarCook
            };
            quickEater.TierTwoProfessions = new List<TierTwoProfession>
            {
                gourmet,
                glutton
            };
            return new Profession[]
            {
                sousChef,
                quickEater,
                headChef,
                fiveStarCook,
                gourmet,
                glutton
            };
        }

        private int GetLevel()
        {
            //this.FixExpLength();
            return Game1.player.GetCustomSkillLevel(SpaceCoreSkillId);
        }

        private int GetExperience()
        {
            return Game1.player.GetCustomSkillExperience(SpaceCoreSkillId);
        }

        private static void SetExperience(int amount)
        {
            int addedExperience = amount - Game1.player.GetCustomSkillExperience(SpaceCoreSkillId);
            Game1.player.AddCustomSkillExperience(SpaceCoreSkillId, addedExperience);
        }
    }
}

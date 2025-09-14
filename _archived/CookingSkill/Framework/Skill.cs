using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace CookingSkill.Framework
{
    internal class Skill : SpaceCore.Skills.Skill
    {
        public static GenericProfession ProfessionSellPrice;
        public static GenericProfession ProfessionBuffTime;
        public static GenericProfession ProfessionConservation;
        public static GenericProfession ProfessionSilver;
        public static GenericProfession ProfessionBuffLevel;
        public static GenericProfession ProfessionBuffPlain;

        public Skill()
            : base("spacechase0.Cooking")
        {
            this.Icon = Mod.Instance.Helper.ModContent.Load<Texture2D>("assets/iconA.png");
            this.SkillsPageIcon = Mod.Instance.Helper.ModContent.Load<Texture2D>("assets/iconB.png");

            this.ExperienceCurve = new[] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 };

            this.ExperienceBarColor = new Microsoft.Xna.Framework.Color(196, 76, 255);

            // Level 5
            Skill.ProfessionSellPrice = new GenericProfession(skill: this, id: "SellPrice", name: I18n.Gourmet_Name, description: I18n.Gourmet_Desc);
            this.Professions.Add(Skill.ProfessionSellPrice);

            Skill.ProfessionBuffTime = new GenericProfession(skill: this, id: "BuffTime", name: I18n.Satisfying_Name, description: I18n.Satisfying_Desc);
            this.Professions.Add(Skill.ProfessionBuffTime);

            this.ProfessionsForLevels.Add(new ProfessionPair(5, Skill.ProfessionSellPrice, Skill.ProfessionBuffTime));

            // Level 10 - track A
            Skill.ProfessionConservation = new GenericProfession(skill: this, id: "Conservation", name: I18n.Efficient_Name, description: I18n.Efficient_Desc);
            this.Professions.Add(Skill.ProfessionConservation);

            Skill.ProfessionSilver = new GenericProfession(skill: this, id: "Silver", name: I18n.ProfessionalChef_Name, description: I18n.ProfessionalChef_Desc);
            this.Professions.Add(Skill.ProfessionSilver);

            this.ProfessionsForLevels.Add(new ProfessionPair(10, Skill.ProfessionConservation, Skill.ProfessionSilver, Skill.ProfessionSellPrice));

            // Level 10 - track B
            Skill.ProfessionBuffLevel = new GenericProfession(skill: this, id: "BuffLevel", name: I18n.IntenseFlavors_Name, description: I18n.IntenseFlavors_Desc);
            this.Professions.Add(Skill.ProfessionBuffLevel);

            Skill.ProfessionBuffPlain = new GenericProfession(skill: this, id: "BuffPlain", name: I18n.SecretSpices_Name, description: I18n.SecretSpices_Desc);
            this.Professions.Add(Skill.ProfessionBuffPlain);

            this.ProfessionsForLevels.Add(new ProfessionPair(10, Skill.ProfessionBuffLevel, Skill.ProfessionBuffPlain, Skill.ProfessionBuffTime));
        }

        public override string GetName()
        {
            return I18n.Skill_Name();
        }

        public override List<string> GetExtraLevelUpInfo(int level)
        {
            return new()
            {
                I18n.Skill_LevelUpPerk(bonus: 3)
            };
        }

        public override string GetSkillPageHoverText(int level)
        {
            return I18n.Skill_LevelUpPerk(bonus: 3 * level);
        }
    }
}

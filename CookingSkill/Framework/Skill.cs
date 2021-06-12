using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace CookingSkill.Framework
{
    internal class Skill : SpaceCore.Skills.Skill
    {
        public class GenericProfession : Profession
        {
            public GenericProfession(Skill skill, string theId)
                : base(skill, theId) { }

            internal string Name { get; set; }
            internal string Description { get; set; }

            public override string GetName()
            {
                return this.Name;
            }

            public override string GetDescription()
            {
                return this.Description;
            }
        }

        public static GenericProfession ProfessionSellPrice;
        public static GenericProfession ProfessionBuffTime;
        public static GenericProfession ProfessionConservation;
        public static GenericProfession ProfessionSilver;
        public static GenericProfession ProfessionBuffLevel;
        public static GenericProfession ProfessionBuffPlain;

        public Skill()
            : base("spacechase0.Cooking")
        {
            this.Icon = Mod.Instance.Helper.Content.Load<Texture2D>("assets/iconA.png");
            this.SkillsPageIcon = Mod.Instance.Helper.Content.Load<Texture2D>("assets/iconB.png");

            this.ExperienceCurve = new[] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 };

            this.ExperienceBarColor = new Microsoft.Xna.Framework.Color(196, 76, 255);

            // Level 5
            Skill.ProfessionSellPrice = new GenericProfession(this, "SellPrice");
            Skill.ProfessionSellPrice.Icon = null; // TODO
            Skill.ProfessionSellPrice.Name = "Gourmet";
            Skill.ProfessionSellPrice.Description = "+20% sell price";
            this.Professions.Add(Skill.ProfessionSellPrice);

            Skill.ProfessionBuffTime = new GenericProfession(this, "BuffTime");
            Skill.ProfessionBuffTime.Icon = null; // TODO
            Skill.ProfessionBuffTime.Name = "Satisfying";
            Skill.ProfessionBuffTime.Description = "+25% buff duration once eaten";
            this.Professions.Add(Skill.ProfessionBuffTime);

            this.ProfessionsForLevels.Add(new ProfessionPair(5, Skill.ProfessionSellPrice, Skill.ProfessionBuffTime));

            // Level 10 - track A
            Skill.ProfessionConservation = new GenericProfession(this, "Conservation");
            Skill.ProfessionConservation.Icon = null; // TODO
            Skill.ProfessionConservation.Name = "Efficient";
            Skill.ProfessionConservation.Description = "15% chance to not consume ingredients";
            this.Professions.Add(Skill.ProfessionConservation);

            Skill.ProfessionSilver = new GenericProfession(this, "Silver");
            Skill.ProfessionSilver.Icon = null; // TODO
            Skill.ProfessionSilver.Name = "Professional Chef";
            Skill.ProfessionSilver.Description = "Home-cooked meals are always at least silver";
            this.Professions.Add(Skill.ProfessionSilver);

            this.ProfessionsForLevels.Add(new ProfessionPair(10, Skill.ProfessionConservation, Skill.ProfessionSilver, Skill.ProfessionSellPrice));

            // Level 10 - track B
            Skill.ProfessionBuffLevel = new GenericProfession(this, "BuffLevel");
            Skill.ProfessionBuffLevel.Icon = null; // TODO
            Skill.ProfessionBuffLevel.Name = "Intense Flavors";
            Skill.ProfessionBuffLevel.Description = "Food buffs are one level stronger once eaten\n(+20% for max energy or magnetism)";
            this.Professions.Add(Skill.ProfessionBuffLevel);

            Skill.ProfessionBuffPlain = new GenericProfession(this, "BuffPlain");
            Skill.ProfessionBuffPlain.Icon = null; // TODO
            Skill.ProfessionBuffPlain.Name = "Secret Spices";
            Skill.ProfessionBuffPlain.Description = "Provides a few random buffs when eating unbuffed food";
            this.Professions.Add(Skill.ProfessionBuffPlain);

            this.ProfessionsForLevels.Add(new ProfessionPair(10, Skill.ProfessionBuffLevel, Skill.ProfessionBuffPlain, Skill.ProfessionBuffTime));
        }

        public override string GetName()
        {
            return "Cooking";
        }

        public override List<string> GetExtraLevelUpInfo(int level)
        {
            List<string> list = new List<string>();
            list.Add("+3% edibility in home-cooked foods");
            return list;
        }

        public override string GetSkillPageHoverText(int level)
        {
            return "+" + (3 * level) + "% edibility in home-cooked foods";
        }
    }
}

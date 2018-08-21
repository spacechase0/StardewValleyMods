using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookingSkill
{
    public class Skill : SpaceCore.Skills.Skill
    {
        public class GenericProfession : SpaceCore.Skills.Skill.Profession
        {
            public GenericProfession(Skill skill, string theId)
            :   base( skill, theId )
            {
            }
            
            internal string Name { get; set; }
            internal string Description { get; set; }

            public override string GetName()
            {
                return Name;
            }

            public override string GetDescription()
            {
                return Description;
            }
        }

        public static GenericProfession ProfessionSellPrice = null;
        public static GenericProfession ProfessionBuffTime = null;
        public static GenericProfession ProfessionConservation = null;
        public static GenericProfession ProfessionSilver = null;
        public static GenericProfession ProfessionBuffLevel = null;
        public static GenericProfession ProfessionBuffPlain = null;

        public Skill()
        :   base( "spacechase0.Cooking" )
        {
            Icon = Mod.instance.Helper.Content.Load<Texture2D>("iconA.png");
            SkillsPageIcon = Mod.instance.Helper.Content.Load<Texture2D>("iconB.png");

            ExperienceCurve = new int[] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 }; ;

            ExperienceBarColor = new Microsoft.Xna.Framework.Color(196, 76, 255);

            // Level 5
            ProfessionSellPrice = new GenericProfession(this, "SellPrice");
            ProfessionSellPrice.Icon = null; // TODO
            ProfessionSellPrice.Name = "Gourmet";
            ProfessionSellPrice.Description = "+20% sell price";
            Professions.Add(ProfessionSellPrice);

            ProfessionBuffTime = new GenericProfession(this, "BuffTime");
            ProfessionBuffTime.Icon = null; // TODO
            ProfessionBuffTime.Name = "Satisfying";
            ProfessionBuffTime.Description = "+25% buff duration once eaten";
            Professions.Add(ProfessionBuffTime);

            ProfessionsForLevels.Add(new ProfessionPair(5, ProfessionSellPrice, ProfessionBuffTime));
            
            // Level 10 - track A
            ProfessionConservation = new GenericProfession(this, "Conservation");
            ProfessionConservation.Icon = null; // TODO
            ProfessionConservation.Name = "Efficient";
            ProfessionConservation.Description = "15% chance to not consume ingredients";
            Professions.Add(ProfessionConservation);

            ProfessionSilver = new GenericProfession(this, "Silver");
            ProfessionSilver.Icon = null; // TODO
            ProfessionSilver.Name = "Professional Chef";
            ProfessionSilver.Description = "Home-cooked meals are always at least silver";
            Professions.Add(ProfessionSilver);

            ProfessionsForLevels.Add(new ProfessionPair(10, ProfessionConservation, ProfessionSilver, ProfessionSellPrice));

            // Level 10 - track B
            ProfessionBuffLevel = new GenericProfession(this, "BuffLevel");
            ProfessionBuffLevel.Icon = null; // TODO
            ProfessionBuffLevel.Name = "Intense Flavors";
            ProfessionBuffLevel.Description = "Food buffs are one level stronger once eaten\n(+20% for max energy or magnetism)";
            Professions.Add(ProfessionBuffLevel);

            ProfessionBuffPlain = new GenericProfession(this, "BuffPlain");
            ProfessionBuffPlain.Icon = null; // TODO
            ProfessionBuffPlain.Name = "Secret Spices";
            ProfessionBuffPlain.Description = "Provides a few random buffs when eating unbuffed food";
            Professions.Add(ProfessionBuffPlain);

            ProfessionsForLevels.Add(new ProfessionPair(10, ProfessionBuffLevel, ProfessionBuffPlain, ProfessionBuffTime));
        }

        public override string GetName()
        {
            return "Cooking";
        }

        public override List<string> GetExtraLevelUpInfo( int level )
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

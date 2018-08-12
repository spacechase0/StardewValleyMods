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
        public static SpaceCore.Skills.Skill.Profession ProfessionSellPrice = null;
        public static SpaceCore.Skills.Skill.Profession ProfessionBuffTime = null;
        public static SpaceCore.Skills.Skill.Profession ProfessionConservation = null;
        public static SpaceCore.Skills.Skill.Profession ProfessionSilver = null;
        public static SpaceCore.Skills.Skill.Profession ProfessionBuffLevel = null;
        public static SpaceCore.Skills.Skill.Profession ProfessionBuffPlain = null;

        public Skill()
        :   base( "spacechase0.Cooking" )
        {
            Name = "Cooking";
            Icon = Mod.instance.Helper.Content.Load<Texture2D>("iconA.png");

            ExperienceCurve = new int[] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 }; ;

            ExperienceBarColor = new Microsoft.Xna.Framework.Color(196, 76, 255);

            // Level 5
            ProfessionSellPrice = new Profession(this, "SellPrice");
            ProfessionSellPrice.Icon = null; // TODO
            ProfessionSellPrice.Name = "Gourmet";
            ProfessionSellPrice.Description = "+20% sell price";
            Professions.Add(ProfessionSellPrice);

            ProfessionBuffTime = new Profession(this, "BuffTime");
            ProfessionBuffTime.Icon = null; // TODO
            ProfessionBuffTime.Name = "Satisfying";
            ProfessionBuffTime.Description = "+25% buff duration once eaten";
            Professions.Add(ProfessionBuffTime);

            ProfessionsForLevels.Add(new ProfessionPair(5, ProfessionSellPrice, ProfessionBuffTime));
            
            // Level 10 - track A
            ProfessionConservation = new Profession(this, "Conservation");
            ProfessionConservation.Icon = null; // TODO
            ProfessionConservation.Name = "Efficient";
            ProfessionConservation.Description = "15% chance to not consume ingredients";
            Professions.Add(ProfessionConservation);

            ProfessionSilver = new Profession(this, "Silver");
            ProfessionSilver.Icon = null; // TODO
            ProfessionSilver.Name = "Professional Chef";
            ProfessionSilver.Description = "Home-cooked meals are always at least silver";
            Professions.Add(ProfessionSilver);

            ProfessionsForLevels.Add(new ProfessionPair(10, ProfessionConservation, ProfessionSilver, ProfessionSellPrice));

            // Level 10 - track B
            ProfessionBuffLevel = new Profession(this, "BuffLevel");
            ProfessionBuffLevel.Icon = null; // TODO
            ProfessionBuffLevel.Name = "Intense Flavors";
            ProfessionBuffLevel.Description = "Food buffs are one level stronger once eaten\n(+20% for max energy or magnetism)";
            Professions.Add(ProfessionBuffLevel);

            ProfessionBuffPlain = new Profession(this, "BuffPlain");
            ProfessionBuffPlain.Icon = null; // TODO
            ProfessionBuffPlain.Name = "Secret Spices";
            ProfessionBuffPlain.Description = "Provides a few random buffs when eating unbuffed food";
            Professions.Add(ProfessionBuffPlain);

            ProfessionsForLevels.Add(new ProfessionPair(10, ProfessionBuffLevel, ProfessionBuffPlain, ProfessionBuffTime));
        }

        public override List<string> GetExtraLevelUpInfo( int level )
        {
            List<string> list = new List<string>();
            list.Add("+3% edibility in home-cooked foods");
            return list;
        }
    }
}

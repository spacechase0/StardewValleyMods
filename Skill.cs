using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SpaceCore.Skills;

namespace Magic
{
    public class Skill : SpaceCore.Skills.Skill
    {
        public class GenericProfession : SpaceCore.Skills.Skill.Profession
        {
            public GenericProfession(Skill skill, string theId)
            : base(skill, theId)
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

        public class UpgradePointProfession : GenericProfession
        {
            public UpgradePointProfession(Skill skill, string theId)
            : base(skill, theId)
            {
            }

            public override void DoImmediateProfessionPerk()
            {
                Game1.player.useSpellPoints(-2);
            }
        }

        public class ManaCapProfession : GenericProfession
        {
            public ManaCapProfession(Skill skill, string theId)
            : base(skill, theId)
            {
            }

            public override void DoImmediateProfessionPerk()
            {
                Game1.player.setMaxMana(Game1.player.getMaxMana() + 500);
            }
        }

        public static GenericProfession ProfessionUpgradePoint1 = null;
        public static GenericProfession ProfessionUpgradePoint2 = null;
        public static GenericProfession Profession_12 = null;
        public static GenericProfession ProfessionManaRegen1 = null;
        public static GenericProfession ProfessionManaRegen2 = null;
        public static GenericProfession ProfessionManaCap = null;

        public Skill()
        : base("spacechase0.Magic")
        {
            Icon = Mod.instance.Helper.Content.Load<Texture2D>("res/interface/magicexpicon.png");
            SkillsPageIcon = null; // TODO: Make an icon for this

            ExperienceCurve = new int[] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 };

            ExperienceBarColor = new Microsoft.Xna.Framework.Color(0, 66, 255);

            // Level 5
            ProfessionUpgradePoint1 = new GenericProfession(this, "UpgradePoints1");
            ProfessionUpgradePoint1.Icon = null; // TODO
            ProfessionUpgradePoint1.Name = "Potential";
            ProfessionUpgradePoint1.Description = "+2 spell upgrade points";
            Professions.Add(ProfessionUpgradePoint1);

            ProfessionManaRegen1 = new GenericProfession(this, "ManaRegen1");
            ProfessionManaRegen1.Icon = null; // TODO
            ProfessionManaRegen1.Name = "Mana Regen I";
            ProfessionManaRegen1.Description = "+0.5 mana regen per level";
            Professions.Add(ProfessionManaRegen1);

            ProfessionsForLevels.Add(new ProfessionPair(5, ProfessionUpgradePoint1, ProfessionManaRegen1));

            // Level 10 - track A
            ProfessionUpgradePoint2 = new GenericProfession(this, "UpgradePoints2");
            ProfessionUpgradePoint2.Icon = null; // TODO
            ProfessionUpgradePoint2.Name = "Prodigy";
            ProfessionUpgradePoint2.Description = "+2 spell upgrade points";
            Professions.Add(ProfessionUpgradePoint2);

            Profession_12 = new GenericProfession(this, "TODO_12");
            Profession_12.Icon = null; // TODO
            Profession_12.Name = "<TODO>";
            Profession_12.Description = "<TODO>";
            Professions.Add(Profession_12);

            ProfessionsForLevels.Add(new ProfessionPair(10, ProfessionUpgradePoint2, Profession_12, ProfessionUpgradePoint1));

            // Level 10 - track B
            ProfessionManaRegen2 = new GenericProfession(this, "ManaRegen2");
            ProfessionManaRegen2.Icon = null; // TODO
            ProfessionManaRegen2.Name = "Mana Regen II";
            ProfessionManaRegen2.Description = "+1 mana regen per level";
            Professions.Add(ProfessionManaRegen2);

            ProfessionManaCap = new ManaCapProfession(this, "ManaCap");
            ProfessionManaCap.Icon = null; // TODO
            ProfessionManaCap.Name = "Mana Reserve";
            ProfessionManaCap.Description = "+500 max mana";
            Professions.Add(ProfessionManaCap);

            ProfessionsForLevels.Add(new ProfessionPair(10, ProfessionManaRegen2, ProfessionManaCap, ProfessionManaRegen1));
        }

        public override string GetName()
        {
            return "Magic";
        }

        public override List<string> GetExtraLevelUpInfo(int level)
        {
            List<string> list = new List<string>();
            list.Add("+1 mana regen");
            return list;
        }

        public override string GetSkillPageHoverText(int level)
        {
            return "+" + level + "mana regen";
        }

        public override void DoLevelPerk(int level)
        {
            Game1.player.setMaxMana(Game1.player.getMaxMana() + 100);
            Game1.player.useSpellPoints(-1);
        }
    }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Magic
{
    public class Skill : SpaceCore.Skills.Skill
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

        public class UpgradePointProfession : GenericProfession
        {
            public UpgradePointProfession(Skill skill, string theId)
                : base(skill, theId) { }

            public override void DoImmediateProfessionPerk()
            {
                Game1.player.useSpellPoints(-2);
            }
        }

        public class ManaCapProfession : GenericProfession
        {
            public ManaCapProfession(Skill skill, string theId)
                : base(skill, theId) { }

            public override void DoImmediateProfessionPerk()
            {
                Game1.player.setMaxMana(Game1.player.getMaxMana() + 500);
            }
        }

        public static GenericProfession ProfessionUpgradePoint1;
        public static GenericProfession ProfessionUpgradePoint2;
        public static GenericProfession ProfessionFifthSpellSlot;
        public static GenericProfession ProfessionManaRegen1;
        public static GenericProfession ProfessionManaRegen2;
        public static GenericProfession ProfessionManaCap;

        public Skill()
            : base("spacechase0.Magic")
        {
            this.Icon = Mod.instance.Helper.Content.Load<Texture2D>("assets/interface/magicexpicon.png");
            this.SkillsPageIcon = null; // TODO: Make an icon for this

            this.ExperienceCurve = new[] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 };

            this.ExperienceBarColor = new Microsoft.Xna.Framework.Color(0, 66, 255);

            // Level 5
            Skill.ProfessionUpgradePoint1 = new UpgradePointProfession(this, "UpgradePoints1");
            Skill.ProfessionUpgradePoint1.Icon = null; // TODO
            Skill.ProfessionUpgradePoint1.Name = "Potential";
            Skill.ProfessionUpgradePoint1.Description = "+2 spell upgrade points";
            this.Professions.Add(Skill.ProfessionUpgradePoint1);

            Skill.ProfessionManaRegen1 = new GenericProfession(this, "ManaRegen1");
            Skill.ProfessionManaRegen1.Icon = null; // TODO
            Skill.ProfessionManaRegen1.Name = "Mana Regen I";
            Skill.ProfessionManaRegen1.Description = "+0.5 mana regen per level";
            this.Professions.Add(Skill.ProfessionManaRegen1);

            this.ProfessionsForLevels.Add(new ProfessionPair(5, Skill.ProfessionUpgradePoint1, Skill.ProfessionManaRegen1));

            // Level 10 - track A
            Skill.ProfessionUpgradePoint2 = new UpgradePointProfession(this, "UpgradePoints2");
            Skill.ProfessionUpgradePoint2.Icon = null; // TODO
            Skill.ProfessionUpgradePoint2.Name = "Prodigy";
            Skill.ProfessionUpgradePoint2.Description = "+2 spell upgrade points";
            this.Professions.Add(Skill.ProfessionUpgradePoint2);

            Skill.ProfessionFifthSpellSlot = new GenericProfession(this, "FifthSpellSlot");
            Skill.ProfessionFifthSpellSlot.Icon = null; // TODO
            Skill.ProfessionFifthSpellSlot.Name = "Memory";
            Skill.ProfessionFifthSpellSlot.Description = "Adds a fifth spell per spell set.";
            this.Professions.Add(Skill.ProfessionFifthSpellSlot);

            this.ProfessionsForLevels.Add(new ProfessionPair(10, Skill.ProfessionUpgradePoint2, Skill.ProfessionFifthSpellSlot, Skill.ProfessionUpgradePoint1));

            // Level 10 - track B
            Skill.ProfessionManaRegen2 = new GenericProfession(this, "ManaRegen2");
            Skill.ProfessionManaRegen2.Icon = null; // TODO
            Skill.ProfessionManaRegen2.Name = "Mana Regen II";
            Skill.ProfessionManaRegen2.Description = "+1 mana regen per level";
            this.Professions.Add(Skill.ProfessionManaRegen2);

            Skill.ProfessionManaCap = new ManaCapProfession(this, "ManaCap");
            Skill.ProfessionManaCap.Icon = null; // TODO
            Skill.ProfessionManaCap.Name = "Mana Reserve";
            Skill.ProfessionManaCap.Description = "+500 max mana";
            this.Professions.Add(Skill.ProfessionManaCap);

            this.ProfessionsForLevels.Add(new ProfessionPair(10, Skill.ProfessionManaRegen2, Skill.ProfessionManaCap, Skill.ProfessionManaRegen1));
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
            return "+" + level + " mana regen";
        }

        public override void DoLevelPerk(int level)
        {
            Game1.player.setMaxMana(Game1.player.getMaxMana() + 100);
            Game1.player.useSpellPoints(-1);
        }
    }
}

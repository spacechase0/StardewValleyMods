using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Magic.Framework.Skills
{
    internal class Skill : SpaceCore.Skills.Skill
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique ID for the magic skill.</summary>
        public static readonly string MagicSkillId = "spacechase0.Magic";

        /// <summary>The level 5 'potential' profession.</summary>
        public static GenericProfession PotentialProfession;

        /// <summary>The level 10 'prodigy' profession.</summary>
        public static GenericProfession ProdigyProfession;

        /// <summary>The level 10 'memory' profession.</summary>
        public static GenericProfession MemoryProfession;

        /// <summary>The level 5 'Mana Regen I' profession.</summary>
        public static GenericProfession ManaRegen1Profession;

        /// <summary>The level 10 'Mana Regen II' profession.</summary>
        public static GenericProfession ManaRegen2Profession;

        /// <summary>The level 10 'Mana Reserve' profession.</summary>
        public static GenericProfession ManaReserveProfession;


        /*********
        ** Public methods
        *********/
        public Skill()
            : base(Skill.MagicSkillId)
        {
            this.Icon = Mod.Instance.Helper.ModContent.Load<Texture2D>("assets/interface/magicexpicon.png");
            this.SkillsPageIcon = null; // TODO: Make an icon for this

            this.ExperienceCurve = new[] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 };

            this.ExperienceBarColor = new Microsoft.Xna.Framework.Color(0, 66, 255);

            // Level 5
            Skill.PotentialProfession = new UpgradePointProfession(this, "UpgradePoints1")
            {
                Icon = null, // TODO
                Name = "Potential",
                Description = "+2 spell upgrade points"
            };
            this.Professions.Add(Skill.PotentialProfession);

            Skill.ManaRegen1Profession = new GenericProfession(this, "ManaRegen1")
            {
                Icon = null, // TODO
                Name = "Mana Regen I",
                Description = "+0.5 mana regen per level"
            };
            this.Professions.Add(Skill.ManaRegen1Profession);

            this.ProfessionsForLevels.Add(new ProfessionPair(5, Skill.PotentialProfession, Skill.ManaRegen1Profession));

            // Level 10 - track A
            Skill.ProdigyProfession = new UpgradePointProfession(this, "UpgradePoints2")
            {
                Icon = null, // TODO
                Name = "Prodigy",
                Description = "+2 spell upgrade points"
            };
            this.Professions.Add(Skill.ProdigyProfession);

            Skill.MemoryProfession = new GenericProfession(this, "FifthSpellSlot")
            {
                Icon = null, // TODO
                Name = "Memory",
                Description = "Adds a fifth spell per spell set."
            };
            this.Professions.Add(Skill.MemoryProfession);

            this.ProfessionsForLevels.Add(new ProfessionPair(10, Skill.ProdigyProfession, Skill.MemoryProfession, Skill.PotentialProfession));

            // Level 10 - track B
            Skill.ManaRegen2Profession = new GenericProfession(this, "ManaRegen2")
            {
                Icon = null, // TODO
                Name = "Mana Regen II",
                Description = "+1 mana regen per level"
            };
            this.Professions.Add(Skill.ManaRegen2Profession);

            Skill.ManaReserveProfession = new ManaCapProfession(this, "ManaCap")
            {
                Icon = null, // TODO
                Name = "Mana Reserve",
                Description = "+500 max mana"
            };
            this.Professions.Add(Skill.ManaReserveProfession);

            this.ProfessionsForLevels.Add(new ProfessionPair(10, Skill.ManaRegen2Profession, Skill.ManaReserveProfession, Skill.ManaRegen1Profession));
        }

        public override string GetName()
        {
            return "Magic";
        }

        public override List<string> GetExtraLevelUpInfo(int level)
        {
            return new()
            {
                "+1 mana regen"
            };
        }

        public override string GetSkillPageHoverText(int level)
        {
            return "+" + level + " mana regen";
        }

        public override void DoLevelPerk(int level)
        {
            // fix magic info if invalid
            Magic.FixMagicIfNeeded(Game1.player, overrideMagicLevel: level - 1);

            // add level perk
            int curMana = Game1.player.GetMaxMana();
            if (level > 1 || curMana < MagicConstants.ManaPointsPerLevel) // skip increasing mana for first level, since we did it on learning the skill
                Game1.player.SetMaxMana(curMana + MagicConstants.ManaPointsPerLevel);

            Game1.player.GetSpellBook().UseSpellPoints(-1);
        }
    }
}

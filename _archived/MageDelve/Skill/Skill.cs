using System.Collections.Generic;
using MageDelve.Mana;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MageDelve.Skill
{
    public class ArcanaSkill : SpaceCore.Skills.Skill
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique ID for the magic skill.</summary>
        public static readonly string ArcanaSkillId = "spacechase0.MageDelve.Arcana";

        /// <summary>The level 5 'potential' profession.</summary>
        public static GenericProfession MoreEssencesProfession;

        /// <summary>The level 10 'prodigy' profession.</summary>
        public static GenericProfession CheaperRecipesProfession;

        /// <summary>The level 10 'memory' profession.</summary>
        public static GenericProfession TrashCanProfession;

        /// <summary>The level 5 'Mana Regen I' profession.</summary>
        public static GenericProfession ManaRegenProfession;

        /// <summary>The level 10 'Mana Regen II' profession.</summary>
        public static GenericProfession MagicCombatProfession;

        /// <summary>The level 10 'Mana Reserve' profession.</summary>
        public static GenericProfession ManaCapProfession;


        /*********
        ** Public methods
        *********/
        public ArcanaSkill()
            : base(ArcanaSkillId)
        {
            this.Icon = Mod.instance.Helper.ModContent.Load<Texture2D>("assets/buff-icon.png");
            this.SkillsPageIcon = null; // TODO: Make an icon for this

            this.ExperienceCurve = new[] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 };

            this.ExperienceBarColor = new Microsoft.Xna.Framework.Color(0, 66, 255);

            // Level 5
            ArcanaSkill.MoreEssencesProfession = new GenericProfession(this, "alchemy-essences")
            {
                Icon = Mod.instance.Helper.ModContent.Load<Texture2D>("assets/profession_0.png")
            };
            this.Professions.Add(ArcanaSkill.MoreEssencesProfession);

            ArcanaSkill.ManaRegenProfession = new GenericProfession(this, "mana-regen")
            {
                Icon = Mod.instance.Helper.ModContent.Load<Texture2D>("assets/profession_1.png")
            };
            this.Professions.Add(ArcanaSkill.ManaRegenProfession);

            this.ProfessionsForLevels.Add(new ProfessionPair(5, ArcanaSkill.MoreEssencesProfession, ArcanaSkill.ManaRegenProfession));

            // Level 10 - track A
            ArcanaSkill.CheaperRecipesProfession = new GenericProfession(this, "alchemy-cheaper-recipes")
            {
                Icon = Mod.instance.Helper.ModContent.Load<Texture2D>("assets/profession_00.png")
            };
            this.Professions.Add(ArcanaSkill.CheaperRecipesProfession);

            ArcanaSkill.TrashCanProfession = new GenericProfession(this, "alchemy-trash-can")
            {
                Icon = Mod.instance.Helper.ModContent.Load<Texture2D>("assets/profession_01.png")
            };
            this.Professions.Add(ArcanaSkill.TrashCanProfession);

            this.ProfessionsForLevels.Add(new ProfessionPair(10, ArcanaSkill.CheaperRecipesProfession, ArcanaSkill.TrashCanProfession, ArcanaSkill.MoreEssencesProfession));

            // Level 10 - track B
            ArcanaSkill.MagicCombatProfession = new GenericProfession(this, "magic-combat-buff")
            {
                Icon = Mod.instance.Helper.ModContent.Load<Texture2D>("assets/profession_10.png")
            };
            this.Professions.Add(ArcanaSkill.MagicCombatProfession);

            ArcanaSkill.ManaCapProfession = new ManaCapProfession(this, "mana-cap", 100)
            {
                Icon = Mod.instance.Helper.ModContent.Load<Texture2D>("assets/profession_11.png")
            };
            this.Professions.Add(ArcanaSkill.ManaCapProfession);

            this.ProfessionsForLevels.Add(new ProfessionPair(10, ArcanaSkill.MagicCombatProfession, ArcanaSkill.ManaCapProfession, ArcanaSkill.ManaRegenProfession));
        }

        public override string GetName()
        {
            return I18n.Skill_Name();
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
            // add level perk
            int curMaxMana = (int)Game1.player.GetMaxMana();
            Game1.player.SetMaxMana(curMaxMana + 100);
        }
    }
}

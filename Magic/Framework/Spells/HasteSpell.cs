using Magic.Framework.Schools;
using SpaceCore;
using StardewValley;

namespace Magic.Framework.Spells
{
    internal class HasteSpell : Spell
    {
        /*********
        ** Public methods
        *********/
        public HasteSpell()
            : base(SchoolId.Life, "haste") { }

        public override bool CanCast(Farmer player, int level)
        {
            if (player.buffs.IsApplied("spell:life:haste"))
                return false;

            return base.CanCast(player, level);
        }

        public override int GetManaCost(Farmer player, int level)
        {
            return 10;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            if (player.buffs.IsApplied("spell:life:haste"))
                return null;

            var effects = new StardewValley.Buffs.BuffEffects();
            effects.speed.Value = level + 1;

            player.buffs.Apply(new Buff(
                buff_id: "spell:life:haste",
                display_source: "Haste (spell)",
                duration: (60 + level * 120) / 10 * 7000,
                buff_effects: effects
            ));

            //Game1.buffsDisplay.addOtherBuff(new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, level + 1, 0, 0, 60 + level * 120, "spell:life:haste", "Haste (spell)"));
            player.AddCustomSkillExperience(Magic.Skill, 5);
            return null;
        }
    }
}

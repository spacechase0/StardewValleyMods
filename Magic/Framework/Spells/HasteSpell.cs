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
            if (player == Game1.player)
            {
                foreach (var buff in Game1.buffsDisplay.otherBuffs)
                {
                    if (buff.source == "spell:life:haste")
                        return false;
                }
            }

            return base.CanCast(player, level);
        }

        public override int GetManaCost(Farmer player, int level)
        {
            return 10;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            if (player != Game1.player)
                return null;

            foreach (var buff in Game1.buffsDisplay.otherBuffs)
            {
                if (buff.source == "spell:life:haste")
                    return null;
            }

            Game1.buffsDisplay.addOtherBuff(new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, level + 1, 0, 0, 60 + level * 120, "spell:life:haste", "Haste (spell)"));
            player.AddCustomSkillExperience(Magic.Skill, 5);
            return null;
        }
    }
}

using Magic.Schools;
using SpaceCore;
using StardewValley;

namespace Magic.Spells
{
    public class HasteSpell : Spell
    {
        public HasteSpell() : base(SchoolId.Life, "haste")
        {
        }

        public override bool canCast(Farmer player, int level)
        {
            if (player == Game1.player)
            {
                foreach (var buff in Game1.buffsDisplay.otherBuffs)
                {
                    if (buff.source == "spell:life:haste")
                        return false;
                }
            }

            return base.canCast(player, level);
        }

        public override int getManaCost(Farmer player, int level)
        {
            return 10;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            if (player != Game1.player)
                return null;

            foreach ( var buff in Game1.buffsDisplay.otherBuffs )
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

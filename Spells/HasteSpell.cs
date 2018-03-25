using Magic.Schools;
using StardewValley;

namespace Magic.Spells
{
    public class HasteSpell : Spell
    {
        public HasteSpell() : base(SchoolId.Life, "haste")
        {
        }

        public override bool canCast(StardewValley.Farmer player, int level)
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

        public override int getManaCost(StardewValley.Farmer player, int level)
        {
            return 10;
        }

        public override void onCast(StardewValley.Farmer player, int level, int targetX, int targetY)
        {
            if (player != Game1.player)
                return;

            foreach ( var buff in Game1.buffsDisplay.otherBuffs )
            {
                if (buff.source == "spell:life:haste")
                    return;
            }

            Game1.buffsDisplay.addOtherBuff(new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, level + 1, 0, 0, 60 + level * 120, "spell:air:haste", "Haste (spell)"));
            player.addMagicExp(10);
        }
    }
}

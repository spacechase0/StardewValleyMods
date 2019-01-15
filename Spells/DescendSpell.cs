using Magic.Schools;
using StardewValley;
using StardewValley.Locations;

namespace Magic.Spells
{
    public class DescendSpell : Spell
    {
        public DescendSpell() : base(SchoolId.Elemental, "descend")
        {
        }

        public override bool canCast(Farmer player, int level)
        {
            return base.canCast(player, level) && player.currentLocation is MineShaft;
        }

        public override int getManaCost(Farmer player, int level)
        {
            return 15;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            if (player != Game1.player)
                return null;

            var ms = player.currentLocation as MineShaft;
            if (ms == null)
                return null;

            int target = ms.mineLevel + 1 + 2 * level;
            if ( ms.mineLevel <= 120 && target >= 120 )
            {
                // We don't want the player to go through the bottom of the
                // original mine and into the skull cavern.
                target = 120;
            }

            Game1.enterMine(target);

            player.addMagicExp(5);
            return null;
        }
    }
}

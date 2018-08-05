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

        public override bool canCast(StardewValley.Farmer player, int level)
        {
            return base.canCast(player, level) && player.currentLocation is MineShaft;
        }

        public override int getManaCost(StardewValley.Farmer player, int level)
        {
            return 15;
        }

        public override void onCast(StardewValley.Farmer player, int level, int targetX, int targetY)
        {
            if (player != Game1.player)
                return;

            var ms = player.currentLocation as MineShaft;
            if (ms == null)
                return;

            int target = ms.mineLevel + 1 + 2 * level;
            if ( ms.mineLevel <= 120 && target >= 120 )
            {
                // We don't want the player to go through the bottom of the
                // original mine and into the skull cavern.
                target = 120;
            }

            Game1.enterMine(target);

            player.addMagicExp(5);
        }
    }
}

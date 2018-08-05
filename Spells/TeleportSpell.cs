using Magic.Game.Interface;
using Magic.Schools;
using Magic.Spells;
using StardewValley;

namespace Magic
{
    public class TeleportSpell : Spell
    {
        public TeleportSpell()
        :   base( SchoolId.Elemental, "teleport" )
        {
        }

        public override int getMaxCastingLevel()
        {
            return 1;
        }

        public override bool canCast(StardewValley.Farmer player, int level)
        {
            return base.canCast(player, level) && player.currentLocation.IsOutdoors && player.mount == null && player.hasItemInInventory(Mod.ja.GetObjectId("Travel Core"), 1);
        }

        public override int getManaCost(StardewValley.Farmer player, int level)
        {
            return 0;
        }

        public override void onCast(StardewValley.Farmer player, int level, int targetX, int targetY)
        {
            Game1.activeClickableMenu = new TeleportMenu();
        }
    }
}
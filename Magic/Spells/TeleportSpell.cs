using Magic.Game.Interface;
using Magic.Schools;
using Magic.Spells;
using StardewValley;

namespace Magic
{
    public class TeleportSpell : Spell
    {
        public TeleportSpell()
            : base(SchoolId.Elemental, "teleport") { }

        public override int GetMaxCastingLevel()
        {
            return 1;
        }

        public override bool CanCast(Farmer player, int level)
        {
            return base.CanCast(player, level) && player.currentLocation.IsOutdoors && player.mount == null && player.hasItemInInventory(Mod.Ja.GetObjectId("Travel Core"), 1);
        }

        public override int GetManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            Game1.activeClickableMenu = new TeleportMenu();
            return null;
        }
    }
}

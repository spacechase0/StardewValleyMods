using Magic.Framework.Game.Interface;
using Magic.Framework.Schools;
using StardewValley;

namespace Magic.Framework.Spells
{
    internal class TeleportSpell : Spell
    {
        /*********
        ** Public methods
        *********/
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
            if (player.IsLocalPlayer)
                Game1.activeClickableMenu = new TeleportMenu();

            return null;
        }
    }
}

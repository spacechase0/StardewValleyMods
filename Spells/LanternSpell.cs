using Magic.Schools;
using StardewValley;
using System.Linq;

namespace Magic.Spells
{
    class LanternSpell : Spell
    {
        public LanternSpell() : base( SchoolId.Nature, "lantern" )
        {
        }

        public override int getManaCost(Farmer player, int level)
        {
            return level;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            if (player != Game1.player)
                return null;

            int power = 4;
            if (level == 1)
                power = 8;
            else if (level == 2)
                power = 16;
            player.currentLocation.sharedLights.Add(new LightSource(1, Game1.player.position, power));
            player.addMagicExp(level);

            return null;
        }
    }
}

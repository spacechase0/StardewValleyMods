using StardewModdingAPI.Events;
using Magic.Schools;
using StardewValley;
using System;

namespace Magic.Spells
{
    class LanternSpell : Spell
    {
        public LanternSpell() : base( SchoolId.Nature, "lantern" )
        {
        }

        public override int getManaCost(StardewValley.Farmer player, int level)
        {
            return level;
        }

        public override void onCast(StardewValley.Farmer player, int level, int targetX, int targetY)
        {
            if (player != Game1.player)
                return;

            int power = 4;
            if (level == 1)
                power = 8;
            else if (level == 2)
                power = 16;
            player.currentLocation.sharedLights.Add(new LightSource(1, Game1.player.position, power));
            player.addMagicExp(level);
        }
    }
}

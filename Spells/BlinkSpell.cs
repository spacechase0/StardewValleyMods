using Microsoft.Xna.Framework;
using Magic.Schools;
using StardewValley;
using SFarmer = StardewValley.Farmer;

namespace Magic.Spells
{
    public class BlinkSpell : Spell
    {
        public BlinkSpell() : base(SchoolId.Toil, "blink")
        {
        }

        public override int getManaCost(SFarmer player, int level)
        {
            return 5;
        }

        public override int getMaxCastingLevel()
        {
            return 1;
        }

        public override void onCast(SFarmer player, int level, int targetX, int targetY)
        {
            Log.debug(player.Name + " casted Blink.");
            player.position.X = targetX - player.GetBoundingBox().Width / 2;
            player.position.Y = targetY - player.GetBoundingBox().Height / 2;
            Game1.playSound("powerup");
            player.addMagicExp(5);
        }
    }
}

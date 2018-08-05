using Microsoft.Xna.Framework;
using Magic.Schools;
using StardewValley;
using SFarmer = StardewValley.Farmer;

namespace Magic.Spells
{
    public class HealSpell : Spell
    {
        public HealSpell() : base(SchoolId.Life, "heal")
        {
        }

        public override int getManaCost(SFarmer player, int level)
        {
            return 5 + 5 * level;
        }

        public override bool canCast(SFarmer player, int level)
        {
            return base.canCast(player, level) && player.health != player.maxHealth;
        }

        public override void onCast(SFarmer player, int level, int targetX, int targetY)
        {
            Log.debug(player.Name + " casted Heal.");
            int health = 10 + 15 * level;
            player.health += health;
            player.currentLocation.debris.Add(new Debris(health, new Vector2((float)(Game1.player.getStandingX() + 8), (float)Game1.player.getStandingY()), Color.Green, 1f, (Character)Game1.player));
            Game1.playSound("healSound");
            player.addMagicExp(health / 2);
        }
    }
}

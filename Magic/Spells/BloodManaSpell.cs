using Microsoft.Xna.Framework;
using Magic.Schools;
using StardewValley;
using SpaceCore;

namespace Magic.Spells
{
    public class BloodManaSpell : Spell
    {
        public const float MANA_RATIO_BASE = 0.30f;
        public const float MANA_RATIO_INCR = 0.05f;

        public BloodManaSpell() : base(SchoolId.Eldritch, "bloodmana")
        {
        }

        public override int getManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override bool canCast(Farmer player, int level)
        {
            return player.getCurrentMana() != player.getMaxMana() && player.health > 10 + 10 * level;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            int health = 10 + 10 * level;
            player.health -= health;
            player.currentLocation.debris.Add(new Debris(health, new Vector2((float)(Game1.player.getStandingX() + 8), (float)Game1.player.getStandingY()), Color.Red, 1f, (Character)Game1.player));
            Game1.playSound("ow");
            Game1.hitShakeTimer = 100 * health;

            int mana = (int)(health * (MANA_RATIO_BASE + MANA_RATIO_INCR * level));
            player.addMana(mana);
            player.currentLocation.debris.Add(new Debris(mana, new Vector2((float)(Game1.player.getStandingX() + 8), (float)Game1.player.getStandingY()), Color.Blue, 1f, (Character)Game1.player));
            Game1.playSound("powerup");
            /*
            player.AddCustomSkillExperience(Magic.Skill,-mana);
            if (player.GetCustomSkillExperience(Magic.Skill) < 0)
                player.AddCustomSkillExperience(Magic.Skill,-player.getMagicExp());
            */

            return null;
        }
    }
}

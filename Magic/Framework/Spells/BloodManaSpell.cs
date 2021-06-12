using Magic.Framework.Schools;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Magic.Framework.Spells
{
    internal class BloodManaSpell : Spell
    {
        public const float ManaRatioBase = 0.30f;
        public const float ManaRatioIncr = 0.05f;

        public BloodManaSpell()
            : base(SchoolId.Eldritch, "bloodmana") { }

        public override int GetManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override bool CanCast(Farmer player, int level)
        {
            return player.GetCurrentMana() != player.GetMaxMana() && player.health > 10 + 10 * level;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            int health = 10 + 10 * level;
            player.health -= health;
            player.currentLocation.debris.Add(new Debris(health, new Vector2(Game1.player.getStandingX() + 8, Game1.player.getStandingY()), Color.Red, 1f, Game1.player));
            Game1.playSound("ow");
            Game1.hitShakeTimer = 100 * health;

            int mana = (int)(health * (BloodManaSpell.ManaRatioBase + BloodManaSpell.ManaRatioIncr * level));
            player.AddMana(mana);
            player.currentLocation.debris.Add(new Debris(mana, new Vector2(Game1.player.getStandingX() + 8, Game1.player.getStandingY()), Color.Blue, 1f, Game1.player));
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

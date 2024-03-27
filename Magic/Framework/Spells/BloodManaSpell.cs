using Magic.Framework.Schools;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Magic.Framework.Spells
{
    internal class BloodManaSpell : Spell
    {
        /*********
        ** Fields
        *********/
        private const float ManaRatioBase = 0.30f;
        private const float ManaRatioIncr = 0.05f;


        /*********
        ** Public methods
        *********/
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
            player.currentLocation.debris.Add(new Debris(health, new Vector2(player.StandingPixel.X + 8, player.StandingPixel.Y), Color.Red, 1f, player));
            player.LocalSound("ow");
            Game1.hitShakeTimer = 100 * health;

            int mana = (int)(health * (BloodManaSpell.ManaRatioBase + BloodManaSpell.ManaRatioIncr * level));
            player.AddMana(mana);
            player.currentLocation.debris.Add(new Debris(mana, new Vector2(player.StandingPixel.X + 8, player.StandingPixel.Y), Color.Blue, 1f, player));
            player.LocalSound("powerup");
            /*
            player.AddCustomSkillExperience(Magic.Skill,-mana);
            if (player.GetCustomSkillExperience(Magic.Skill) < 0)
                player.AddCustomSkillExperience(Magic.Skill,-player.getMagicExp());
            */

            return null;
        }
    }
}

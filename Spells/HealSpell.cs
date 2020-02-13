using Microsoft.Xna.Framework;
using Magic.Schools;
using StardewValley;
using SpaceCore;

namespace Magic.Spells
{
    public class HealSpell : Spell
    {
        public HealSpell() : base(SchoolId.Life, "heal")
        {
        }

        public override int getManaCost(Farmer player, int level)
        {
            return 7;
        }

        public override bool canCast(Farmer player, int level)
        {
            return base.canCast(player, level) && player.health != player.maxHealth;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            int health = 10 + 15 * level + (player.CombatLevel + 1) * 2;
            player.health += health;
            if (player.health >= player.maxHealth)
                player.health = player.maxHealth;
            player.currentLocation.debris.Add(new Debris(health, new Vector2((float)(Game1.player.getStandingX() + 8), (float)Game1.player.getStandingY()), Color.Green, 1f, (Character)Game1.player));
            Game1.playSound("healSound");
            player.AddCustomSkillExperience(Magic.Skill, health / 2);

            return null;
        }
    }
}

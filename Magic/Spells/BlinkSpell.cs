using Magic.Schools;
using SpaceCore;
using StardewValley;

namespace Magic.Spells
{
    public class BlinkSpell : Spell
    {
        public BlinkSpell()
            : base(SchoolId.Toil, "blink") { }

        public override int GetManaCost(Farmer player, int level)
        {
            return 10;
        }

        public override int GetMaxCastingLevel()
        {
            return 1;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            player.position.X = targetX - player.GetBoundingBox().Width / 2;
            player.position.Y = targetY - player.GetBoundingBox().Height / 2;
            Game1.playSound("powerup");
            player.AddCustomSkillExperience(Magic.Skill, 4);

            return null;
        }
    }
}

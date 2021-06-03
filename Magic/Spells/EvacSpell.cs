using StardewModdingAPI.Events;
using Magic.Schools;
using StardewValley;
using SpaceCore;

namespace Magic.Spells
{
    public class EvacSpell : Spell
    {
        public EvacSpell() : base(SchoolId.Life, "evac")
        {
        }

        public override int getMaxCastingLevel()
        {
            return 1;
        }

        public override int getManaCost(Farmer player, int level)
        {
            return 25;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            player.position.X = enterX;
            player.position.Y = enterY;
            player.AddCustomSkillExperience(Magic.Skill, 5);
            return null;
        }

        private static float enterX, enterY;
        internal static void onLocationChanged()
        {
            enterX = Game1.player.position.X;
            enterY = Game1.player.position.Y;
        }
    }
}

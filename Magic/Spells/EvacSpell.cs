using Magic.Schools;
using SpaceCore;
using StardewValley;

namespace Magic.Spells
{
    public class EvacSpell : Spell
    {
        public EvacSpell()
            : base(SchoolId.Life, "evac") { }

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
            player.position.X = EvacSpell.enterX;
            player.position.Y = EvacSpell.enterY;
            player.AddCustomSkillExperience(Magic.Skill, 5);
            return null;
        }

        private static float enterX, enterY;
        internal static void onLocationChanged()
        {
            EvacSpell.enterX = Game1.player.position.X;
            EvacSpell.enterY = Game1.player.position.Y;
        }
    }
}

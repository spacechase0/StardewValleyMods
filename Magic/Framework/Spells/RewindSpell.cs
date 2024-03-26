using Magic.Framework.Schools;
using SpaceCore;
using StardewValley;

namespace Magic.Framework.Spells
{
    internal class RewindSpell : Spell
    {
        /*********
        ** Public methods
        *********/
        public RewindSpell()
            : base(SchoolId.Arcane, "rewind") { }

        public override int GetMaxCastingLevel()
        {
            return 1;
        }

        public override bool CanCast(Farmer player, int level)
        {
            return base.CanCast(player, level) && player.Items.ContainsId("336", 1);
        }

        public override int GetManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            player.Items.ReduceId("336", 1);
            Game1.timeOfDay -= 200;
            player.AddCustomSkillExperience(Magic.Skill, 25);
            return null;
        }
    }
}

using Magic.Game.Interface;
using Magic.Schools;
using Magic.Spells;
using SpaceCore;
using StardewValley;

namespace Magic
{
    public class RewindSpell : Spell
    {
        public RewindSpell()
        :   base( SchoolId.Arcane, "rewind" )
        {
        }

        public override int getMaxCastingLevel()
        {
            return 1;
        }

        public override bool canCast(Farmer player, int level)
        {
            return base.canCast(player, level) && player.hasItemInInventory(336, 1);
        }

        public override int getManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            player.consumeObject(336, 1);
            Game1.timeOfDay -= 200;
            player.AddCustomSkillExperience(Magic.Skill, 25);
            return null;
        }
    }
}
using Magic.Framework.Schools;
using Magic.Framework.Spells.Effects;
using StardewValley;

namespace Magic.Framework.Spells
{
    internal class ShockwaveSpell : Spell
    {
        /*********
        ** Public methods
        *********/
        public ShockwaveSpell()
            : base(SchoolId.Nature, "shockwave") { }

        public override bool CanCast(Farmer player, int level)
        {
            return base.CanCast(player, level) && player.yJumpVelocity == 0;
        }

        public override int GetManaCost(Farmer player, int level)
        {
            return 10;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            player.jump();
            return new Shockwave(player, level);
        }
    }
}

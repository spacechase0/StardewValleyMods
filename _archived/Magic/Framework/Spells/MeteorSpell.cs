using Magic.Framework.Schools;
using Magic.Framework.Spells.Effects;
using StardewValley;
using SObject = StardewValley.Object;

namespace Magic.Framework.Spells
{
    internal class MeteorSpell : Spell
    {
        /*********
        ** Public methods
        *********/
        public MeteorSpell()
            : base(SchoolId.Eldritch, "meteor") { }

        public override int GetManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override bool CanCast(Farmer player, int level)
        {
            return base.CanCast(player, level) && player.hasItemInInventory(SObject.iridium, 1);
        }

        public override int GetMaxCastingLevel()
        {
            return 1;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            player.consumeObject(SObject.iridium, 1);
            return new Meteor(player, targetX, targetY);
        }
    }
}

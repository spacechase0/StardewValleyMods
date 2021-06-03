using SpaceShared;
using StardewValley;

namespace Magic.Spells
{
    public class DummySpell : Spell
    {
        public DummySpell(string school, string id) : base(school, id)
        {
        }

        public override int getManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            Log.debug($"{player.Name} cast {Id}.");
            return null;
        }
    }
}

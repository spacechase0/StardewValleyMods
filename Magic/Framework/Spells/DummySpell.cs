using SpaceShared;
using StardewValley;

namespace Magic.Framework.Spells
{
    internal class DummySpell : Spell
    {
        /*********
        ** Public methods
        *********/
        public DummySpell(string school, string id)
            : base(school, id) { }

        public override int GetManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            Log.Debug($"{player.Name} cast {this.Id}.");
            return null;
        }
    }
}

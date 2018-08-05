using SFarmer = StardewValley.Farmer;

namespace Magic.Spells
{
    public class DummySpell : Spell
    {
        public DummySpell(string school, string id) : base(school, id)
        {
        }

        public override int getManaCost(SFarmer player, int level)
        {
            return 0;
        }

        public override void onCast(SFarmer player, int level, int targetX, int targetY)
        {
            Log.debug(player.Name + " casted " + Id + ".");
        }
    }
}

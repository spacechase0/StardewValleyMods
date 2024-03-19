using MageDelve.Mana;
using StardewValley;

namespace MageDelve.Skill
{
    internal class ManaCapProfession : GenericProfession
    {
        private int amount;

        /*********
        ** Public methods
        *********/
        public ManaCapProfession(ArcanaSkill skill, string theId, int amt)
            : base(skill, theId)
        {
            amount = amt;
        }

        public override void DoImmediateProfessionPerk()
        {
            Game1.player.SetMaxMana(Game1.player.GetMaxMana() + amount);
        }
    }
}

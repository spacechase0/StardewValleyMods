using StardewValley;

namespace Magic.Framework.Skills
{
    internal class ManaCapProfession : GenericProfession
    {
        /*********
        ** Public methods
        *********/
        public ManaCapProfession(Skill skill, string theId)
            : base(skill, theId) { }

        public override void DoImmediateProfessionPerk()
        {
            Game1.player.SetMaxMana(Game1.player.GetMaxMana() + 500);
        }
    }
}

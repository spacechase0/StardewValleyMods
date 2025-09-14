using StardewValley;

namespace Magic.Framework.Skills
{
    internal class UpgradePointProfession : GenericProfession
    {
        /*********
        ** Public methods
        *********/
        public UpgradePointProfession(Skill skill, string theId)
            : base(skill, theId) { }

        public override void DoImmediateProfessionPerk()
        {
            Game1.player.GetSpellBook().UseSpellPoints(-2);
        }
    }
}

using Magic.Framework.Schools;
using SpaceShared;
using StardewValley;

namespace Magic.Framework.Spells
{
    internal class AnalyzeSpell : Spell
    {
        /*********
        ** Public methods
        *********/
        public AnalyzeSpell()
            : base(SchoolId.Arcane, "analyze")
        {
            this.CanCastInMenus = true;
        }

        public override int GetManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override int GetMaxCastingLevel()
        {
            return 1;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            if (Magic.OnAnalyzeCast != null)
                Util.InvokeEvent<AnalyzeEventArgs>("OnAnalyzeCast", Magic.OnAnalyzeCast.GetInvocationList(), player, new AnalyzeEventArgs(targetX, targetY));

            return null;
        }
    }
}

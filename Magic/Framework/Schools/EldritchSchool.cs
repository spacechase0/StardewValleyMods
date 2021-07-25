using Magic.Framework.Spells;

namespace Magic.Framework.Schools
{
    internal class EldritchSchool : School
    {
        /*********
        ** Public methods
        *********/
        public EldritchSchool()
            : base(SchoolId.Eldritch) { }

        public override Spell[] GetSpellsTier1()
        {
            return new[] { SpellManager.Get("eldritch:meteor"), SpellManager.Get("eldritch:bloodmana") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new[] { SpellManager.Get("eldritch:lucksteal") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new[] { SpellManager.Get("eldritch:spirit") };
        }
    }
}

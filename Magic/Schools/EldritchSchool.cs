using Magic.Spells;

namespace Magic.Schools
{
    internal class EldritchSchool : School
    {
        public EldritchSchool()
            : base(SchoolId.Eldritch) { }

        public override Spell[] GetSpellsTier1()
        {
            return new[] { SpellBook.get("eldritch:meteor"), SpellBook.get("eldritch:bloodmana") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new[] { SpellBook.get("eldritch:lucksteal") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new[] { SpellBook.get("eldritch:spirit") };
        }
    }
}

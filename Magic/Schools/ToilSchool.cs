using Magic.Spells;

namespace Magic.Schools
{
    internal class ToilSchool : School
    {
        public ToilSchool()
            : base(SchoolId.Toil) { }

        public override Spell[] GetSpellsTier1()
        {
            return new[] { SpellBook.Get("toil:cleardebris"), SpellBook.Get("toil:till") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new[] { SpellBook.Get("toil:water") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new[] { SpellBook.Get("toil:blink") };
        }
    }
}

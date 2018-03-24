using Magic.Spells;

namespace Magic.Schools
{
    internal class ToilSchool : School
    {
        public ToilSchool() : base( SchoolId.Toil )
        {
        }

        public override Spell[] GetSpellsTier1()
        {
            return new Spell[] { SpellBook.get("toil:cleardebris"), SpellBook.get("toil:till") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new Spell[] { SpellBook.get("toil:water") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new Spell[] { SpellBook.get("toil:blink") };
        }
    }
}
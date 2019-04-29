using Magic.Spells;

namespace Magic.Schools
{
    internal class ArcaneSchool : School
    {
        public ArcaneSchool() : base( SchoolId.Arcane )
        {
        }

        public override Spell[] GetSpellsTier1()
        {
            return new Spell[] { SpellBook.get("arcane:analyze") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new Spell[] { SpellBook.get("arcane:magicmissle"), SpellBook.get("arcane:enchant") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new Spell[] { SpellBook.get("arcane:rewind") };
        }
    }
}
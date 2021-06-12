using Magic.Spells;

namespace Magic.Schools
{
    internal class ArcaneSchool : School
    {
        public ArcaneSchool()
            : base(SchoolId.Arcane) { }

        public override Spell[] GetSpellsTier1()
        {
            return new[] { SpellBook.Get("arcane:analyze"), SpellBook.Get("arcane:magicmissle") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new[] { SpellBook.Get("arcane:disenchant"), SpellBook.Get("arcane:enchant") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new[] { SpellBook.Get("arcane:rewind") };
        }
    }
}

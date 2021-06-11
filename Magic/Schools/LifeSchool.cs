using Magic.Spells;

namespace Magic.Schools
{
    internal class LifeSchool : School
    {
        public LifeSchool()
            : base(SchoolId.Life) { }

        public override Spell[] GetSpellsTier1()
        {
            return new[] { SpellBook.get("life:evac") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new[] { SpellBook.get("life:heal"), SpellBook.get("life:haste") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new[] { SpellBook.get("life:buff") };
        }
    }
}

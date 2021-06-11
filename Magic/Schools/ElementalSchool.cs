using Magic.Spells;

namespace Magic.Schools
{
    internal class ElementalSchool : School
    {
        public ElementalSchool()
            : base(SchoolId.Elemental) { }

        public override Spell[] GetSpellsTier1()
        {
            return new[] { SpellBook.get("elemental:fireball"), SpellBook.get("elemental:frostbolt") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new[] { SpellBook.get("elemental:descend") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new[] { SpellBook.get("elemental:teleport") };
        }
    }
}

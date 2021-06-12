using Magic.Framework.Spells;

namespace Magic.Framework.Schools
{
    internal class ElementalSchool : School
    {
        public ElementalSchool()
            : base(SchoolId.Elemental) { }

        public override Spell[] GetSpellsTier1()
        {
            return new[] { SpellBook.Get("elemental:fireball"), SpellBook.Get("elemental:frostbolt") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new[] { SpellBook.Get("elemental:descend") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new[] { SpellBook.Get("elemental:teleport") };
        }
    }
}

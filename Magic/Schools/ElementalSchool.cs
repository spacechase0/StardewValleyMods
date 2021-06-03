using Magic.Spells;

namespace Magic.Schools
{
    internal class ElementalSchool : School
    {
        public ElementalSchool() : base( SchoolId.Elemental )
        {
        }

        public override Spell[] GetSpellsTier1()
        {
            return new Spell[] { SpellBook.get("elemental:fireball"), SpellBook.get("elemental:frostbolt") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new Spell[] { SpellBook.get("elemental:descend") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new Spell[] { SpellBook.get("elemental:teleport") };
        }
    }
}
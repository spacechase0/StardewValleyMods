using Magic.Framework.Spells;

namespace Magic.Framework.Schools
{
    internal class ElementalSchool : School
    {
        /*********
        ** Public methods
        *********/
        public ElementalSchool()
            : base(SchoolId.Elemental) { }

        public override Spell[] GetSpellsTier1()
        {
            return new[] { SpellManager.Get("elemental:fireball"), SpellManager.Get("elemental:frostbolt") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new[] { SpellManager.Get("elemental:descend") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new[] { SpellManager.Get("elemental:teleport") };
        }
    }
}

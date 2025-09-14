using Magic.Framework.Spells;

namespace Magic.Framework.Schools
{
    internal class ToilSchool : School
    {
        /*********
        ** Public methods
        *********/
        public ToilSchool()
            : base(SchoolId.Toil) { }

        public override Spell[] GetSpellsTier1()
        {
            return new[] { SpellManager.Get("toil:cleardebris"), SpellManager.Get("toil:till") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new[] { SpellManager.Get("toil:water") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new[] { SpellManager.Get("toil:blink") };
        }
    }
}

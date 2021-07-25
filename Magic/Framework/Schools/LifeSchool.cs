using Magic.Framework.Spells;

namespace Magic.Framework.Schools
{
    internal class LifeSchool : School
    {
        /*********
        ** Public methods
        *********/
        public LifeSchool()
            : base(SchoolId.Life) { }

        public override Spell[] GetSpellsTier1()
        {
            return new[] { SpellManager.Get("life:evac") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new[] { SpellManager.Get("life:heal"), SpellManager.Get("life:haste") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new[] { SpellManager.Get("life:buff") };
        }
    }
}

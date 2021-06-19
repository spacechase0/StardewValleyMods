using Magic.Framework.Spells;

namespace Magic.Framework.Schools
{
    internal class NatureSchool : School
    {
        public NatureSchool()
            : base(SchoolId.Nature) { }

        public override Spell[] GetSpellsTier1()
        {
            return new[] { SpellBook.Get("nature:lantern"), SpellBook.Get("nature:tendrils") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new[] { SpellBook.Get("nature:shockwave") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new[] { SpellBook.Get("nature:photosynthesis") };
        }
    }
}

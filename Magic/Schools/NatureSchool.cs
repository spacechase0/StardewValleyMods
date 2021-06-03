using Magic.Spells;

namespace Magic.Schools
{
    internal class NatureSchool : School
    {
        public NatureSchool() : base( SchoolId.Nature )
        {
        }

        public override Spell[] GetSpellsTier1()
        {
            return new Spell[] { SpellBook.get("nature:lantern"), SpellBook.get("nature:tendrils") };
        }

        public override Spell[] GetSpellsTier2()
        {
            return new Spell[] { SpellBook.get("nature:shockwave") };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new Spell[] { SpellBook.get("nature:photosynthesis") };
        }
    }
}
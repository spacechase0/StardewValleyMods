using Magic.Framework.Spells;

namespace Magic.Framework.Schools
{
    internal class ArcaneSchool : School
    {
        /*********
        ** Public methods
        *********/
        public ArcaneSchool()
            : base(SchoolId.Arcane) { }

        public override Spell[] GetSpellsTier1()
        {
            return new[] { SpellManager.Get("arcane:analyze"), SpellManager.Get("arcane:magicmissle") };
        }

        public override Spell[] GetSpellsTier2()
        {
            string EnchantSpell = "arcane:enchant";
            string DisenchantSpell = "arcane:disenchant";
            if (Mod.Config.EnchantSpell == true)
            {
                EnchantSpell = "arcane:enchant_costly";
                DisenchantSpell = "arcane:disenchant_costly";
            }

            return new[] { SpellManager.Get(DisenchantSpell), SpellManager.Get(EnchantSpell) };
        }

        public override Spell[] GetSpellsTier3()
        {
            return new[] { SpellManager.Get("arcane:rewind") };
        }
    }
}

using Magic.Spells;

namespace Magic
{
    public class PreparedSpell
    {
        public string SpellId { get; }
        public int Level { get; }

        public PreparedSpell() { }

        public PreparedSpell(string spellId, int level)
        {
            SpellId = spellId;
            Level = level;
        }

        public PreparedSpell( Spell spell, int level )
        {
            SpellId = spell.ParentSchoolId + ":" + spell.Id;
            Level = level;
        }
    }
}

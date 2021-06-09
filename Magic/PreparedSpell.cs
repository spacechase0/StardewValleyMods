using Magic.Spells;

namespace Magic
{
    public class PreparedSpell
    {
        public string SpellId { get; set; }
        public int Level { get; set; }

        public PreparedSpell() { }

        public PreparedSpell(string spellId, int level)
        {
            this.SpellId = spellId;
            this.Level = level;
        }

        public PreparedSpell(Spell spell, int level)
        {
            this.SpellId = spell.ParentSchoolId + ":" + spell.Id;
            this.Level = level;
        }
    }
}

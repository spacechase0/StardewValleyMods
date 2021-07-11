using System.Collections.Generic;

namespace Magic.Framework
{
    /// <summary>A hotbar of prepared spells.</summary>
    internal class PreparedSpellBar
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The prepared spells on the bar.</summary>
        public List<PreparedSpell> Spells { get; set; } = new();


        /*********
        ** Public methods
        *********/
        /// <summary>Get the spell in the given slot, if any.</summary>
        /// <param name="index">The slot index.</param>
        public PreparedSpell GetSlot(int index)
        {
            return index < this.Spells.Count
                ? this.Spells[index]
                : null;
        }

        /// <summary>Set a spell slot.</summary>
        /// <param name="index">The slot index.</param>
        /// <param name="spell">The spell to add.</param>
        public void SetSlot(int index, PreparedSpell spell)
        {
            // nothing to do
            if (spell == null && index < this.Spells.Count)
                return;

            // resize if needed
            for (int i = this.Spells.Count - 1; i < index; i++)
                this.Spells.Add(null);

            // set value
            this.Spells[index] = spell;
        }
    }
}

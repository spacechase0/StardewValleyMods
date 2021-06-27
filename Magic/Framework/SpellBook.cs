using System.Collections.Generic;
using Newtonsoft.Json;
using SpaceShared;
using StardewValley;

namespace Magic.Framework
{
    /// <summary>A player's learned and prepared spells.</summary>
    internal class SpellBook
    {
        [JsonIgnore]
        public Farmer Owner { get; internal set; }

        public Dictionary<string, int> KnownSpells = new();
        public PreparedSpell[][] Prepared = {
            new PreparedSpell[] { null, null, null, null, null },
            new PreparedSpell[] { null, null, null, null, null }
        };
        public int SelectedPrepared;

        public PreparedSpell[] GetPreparedSpells()
        {
            if (this.SelectedPrepared >= this.Prepared.Length)
                return new PreparedSpell[5];
            return this.Prepared[this.SelectedPrepared];
        }

        public void SwapPreparedSet()
        {
            this.SelectedPrepared = (this.SelectedPrepared + 1) % this.Prepared.Length;
            Log.Trace("Swapped prepared spell set to set " + (this.SelectedPrepared + 1) + "/" + this.Prepared.Length + ".");
        }
    }
}

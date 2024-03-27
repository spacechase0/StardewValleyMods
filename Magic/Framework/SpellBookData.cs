using System.Collections.Generic;
using System.Linq;
using SpaceShared;
using StardewValley;

namespace Magic.Framework
{
    /// <summary>A raw wrapper around the player's <see cref="Character.modData"/> fields.</summary>
    internal class SpellBookData
    {
        /*********
        ** Fields
        *********/
        /// <summary>The prefix added to mod data keys.</summary>
        private const string Prefix = "spacechase0.Magic";

        /// <summary>The data key for the updated tick.</summary>
        private const string UpdatedKey = SpellBookData.Prefix + "/Updated";

        /// <summary>The data key for the player's free spell points.</summary>
        private const string FreePointsKey = SpellBookData.Prefix + "/FreePoints";

        /// <summary>The data key for the player's known spells.</summary>
        private const string KnownSpellsKey = SpellBookData.Prefix + "/KnownSpells";

        /// <summary>The data key for the player's prepared spells.</summary>
        private const string PreparedSpellsKey = SpellBookData.Prefix + "/PreparedSpells";

        /// <summary>The data key for the player's selected bar of prepared spells.</summary>
        private const string SelectedBarKey = SpellBookData.Prefix + "/SelectedBar";

        /// <summary>The player's mod data.</summary>
        private StardewValley.Mods.ModDataDictionary Data => this.Player.modData;

        /// <summary>The mod data version for which the fields were cached.</summary>
        private int? UpdatedTick;


        /*********
        ** Accessors
        *********/
        /// <summary>The underlying player.</summary>
        public Farmer Player { get; }

        /// <summary>The number of spell points available to spend.</summary>
        public int FreePoints { get; set; }

        /// <summary>The player's learned spells.</summary>
        public IDictionary<string, PreparedSpell> KnownSpells { get; set; }

        /// <summary>The player's spell hotbars.</summary>
        public IList<PreparedSpellBar> Prepared { get; set; }

        /// <summary>The currently selected hotbar, as an index in the <see cref="Prepared"/> list.</summary>
        public int SelectedPrepared { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="player">The underlying player.</param>
        public SpellBookData(Farmer player)
        {
            this.Player = player;
        }

        /// <summary>Rebuild the cached metadata from the player's <see cref="Character.modData"/> field if needed.</summary>
        public void UpdateIfNeeded()
        {
            int updatedTick = this.Data.GetInt(SpellBookData.UpdatedKey);

            if (this.UpdatedTick != updatedTick)
            {
                this.FreePoints = this.Data.GetInt(SpellBookData.FreePointsKey, min: 0);
                this.KnownSpells = this.Data.GetCustom(SpellBookData.KnownSpellsKey, parse: this.ParseKnownSpells, suppressError: false) ?? new Dictionary<string, PreparedSpell>();
                this.Prepared = this.Data.GetCustom(SpellBookData.PreparedSpellsKey, parse: this.ParsePreparedSpells, suppressError: false) ?? new List<PreparedSpellBar>();
                this.SelectedPrepared = this.Data.GetInt(SpellBookData.SelectedBarKey, min: 0);
                this.UpdatedTick = updatedTick;
            }
        }

        /// <summary>Save the cached metadata to the player's <see cref="Character.modData"/> field.</summary>
        public void Save()
        {
            this.Data.SetInt(SpellBookData.FreePointsKey, this.FreePoints, min: 0);
            this.Data.SetCustom(SpellBookData.KnownSpellsKey, this.KnownSpells.Values, serialize: this.SerializeSpells);
            this.Data.SetCustom(SpellBookData.PreparedSpellsKey, this.Prepared, serialize: this.SerializeSpellBars);
            this.Data.SetInt(SpellBookData.SelectedBarKey, this.SelectedPrepared, min: 0);
            this.Data.SetInt(SpellBookData.UpdatedKey, Game1.ticks);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse serialized known spells.</summary>
        /// <param name="raw">The raw serialized string.</param>
        private IDictionary<string, PreparedSpell> ParseKnownSpells(string raw)
        {
            Dictionary<string, PreparedSpell> spells = new();
            foreach (var spell in this.ParseSpells(raw))
                spells[spell.SpellId] = spell;
            return spells;
        }

        /// <summary>Parse serialized prepared spells.</summary>
        /// <param name="raw">The raw serialized string.</param>
        private List<PreparedSpellBar> ParsePreparedSpells(string raw)
        {
            return (raw ?? "")
                .Split('|')
                .Select(rawBar => new PreparedSpellBar { Spells = this.ParseSpells(rawBar) })
                .ToList();
        }

        /// <summary>Parse a serialized known-spells list.</summary>
        /// <param name="raw">The raw serialized string.</param>
        private List<PreparedSpell> ParseSpells(string raw)
        {
            List<PreparedSpell> spells = new();

            if (string.IsNullOrWhiteSpace(raw))
                return spells;

            foreach (string rawSpell in raw.Split(','))
            {
                if (string.IsNullOrWhiteSpace(rawSpell))
                    spells.Add(null);
                else
                {
                    string[] parts = rawSpell.Split(new[] { '=' }, 2);

                    string key = parts[0];
                    if (parts.Length < 2 || !int.TryParse(parts[1], out int level))
                        level = 0;

                    spells.Add(new PreparedSpell(key, level));
                }
            }

            return spells;
        }

        /// <summary>Serialize spells for storage.</summary>
        /// <param name="spells">The spells to serialize.</param>
        private string SerializeSpells(IEnumerable<PreparedSpell> spells)
        {
            return string.Join(",", spells.Select(p => p != null ? $"{p.SpellId}={p.Level}" : "")).TrimEnd(',');
        }

        /// <summary>Serialize spell bars for storage.</summary>
        /// <param name="spellBars">The spell bars to serialize.</param>
        private string SerializeSpellBars(IList<PreparedSpellBar> spellBars)
        {
            return spellBars?.Any() == true
                ? string.Join("|", spellBars.Select(p => this.SerializeSpells(p.Spells)))
                : null;
        }
    }
}

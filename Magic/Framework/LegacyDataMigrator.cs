using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;

namespace Magic.Framework
{
    /// <summary>Handles migrating legacy data for a save file.</summary>
    internal class LegacyDataMigrator
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The legacy save file path for the Magic mod.</summary>
        private string OldFilePath => Path.Combine(Constants.CurrentSavePath, "magic0.2.json");


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor"><inheritdoc cref="Monitor" path="/summary"/></param>
        public LegacyDataMigrator(IMonitor monitor)
        {
            this.Monitor = monitor;
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        public void OnSaveLoaded()
        {
            if (!Context.IsMainPlayer)
                return;

            // handle legacy data file
            if (File.Exists(this.OldFilePath))
            {
                long[] players = this.TryApply(JsonConvert.DeserializeObject<LegacySaveData>(File.ReadAllText(this.OldFilePath))).ToArray();
                if (players.Any())
                    this.Monitor.Log($"Migrated legacy data file for players {string.Join(", ", players)}.");
            }
        }

        /// <summary>Raised after the player finishes saving.</summary>
        public void OnSaved()
        {
            if (!Context.IsMainPlayer)
                return;

            // handle legacy data file
            if (File.Exists(this.OldFilePath))
            {
                File.Delete(this.OldFilePath);
                this.Monitor.Log($"Deleted legacy data file: {this.OldFilePath}.");
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Apply legacy save data to the player's current fields, if valid and the field doesn't already have a value.</summary>
        /// <param name="legacyData">The save data to apply.</param>
        /// <returns>Returns the player IDs whose fields were changed.</returns>
        private IEnumerable<long> TryApply(LegacySaveData legacyData)
        {
            foreach (var pair in legacyData?.Players ?? new())
            {
                // get player spellbook
                Farmer player = Game1.getFarmerMaybeOffline(pair.Key);
                if (player == null)
                    continue;
                SpellBook book = player.GetSpellBook();

                // apply data
                bool changed = false;
                book.Mutate(data =>
                {
                    // free spell point
                    if (data.FreePoints <= 0)
                    {
                        int freePoints = pair.Value.FreePoints;
                        if (freePoints > 0)
                        {
                            data.FreePoints = Math.Max(0, freePoints);
                            changed = true;
                        }
                    }

                    // known spells
                    if (!data.KnownSpells.Any())
                    {
                        PreparedSpell[] knownSpells = (pair.Value.SpellBook?.KnownSpells ?? new())
                            .Select(p => new PreparedSpell(p.Key, p.Value))
                            .ToArray();
                        if (knownSpells.Any())
                        {
                            data.KnownSpells = knownSpells.ToDictionary(p => p.SpellId);
                            changed = true;
                        }
                    }

                    // prepared spells
                    if (!data.Prepared.Any())
                    {
                        var preparedSpells = (pair.Value.SpellBook?.Prepared ?? new PreparedSpell[][] { })
                            .Select(spells => new PreparedSpellBar { Spells = spells.ToList() })
                            .ToArray();
                        if (preparedSpells.Any(bar => bar.Spells.Any(spell => spell != null)))
                        {
                            data.Prepared = preparedSpells;
                            changed = true;
                        }
                    }

                    // prepared index
                    if (data.SelectedPrepared <= 0)
                    {
                        int index = pair.Value.SpellBook?.SelectedPrepared ?? 0;
                        if (index > 0)
                        {
                            data.SelectedPrepared = index;
                            changed = true;
                        }
                    }
                });

                if (changed)
                    yield return pair.Key;
            }
        }

        /// <summary>The data model for the legacy data file.</summary>
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "Used for legacy data deserialization.")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local", Justification = "Used for legacy data deserialization.")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Used for legacy data deserialization.")]
        private class LegacySaveData
        {
            public Dictionary<long, PlayerData> Players { get; set; }

            public class PlayerData
            {
                public int FreePoints { get; set; }
                public SpellBookData SpellBook { get; set; }
            }

            public class SpellBookData
            {
                public Dictionary<string, int> KnownSpells { get; set; }
                public PreparedSpell[][] Prepared { get; set; }
                public int SelectedPrepared { get; set; }
            }
        }
    }
}

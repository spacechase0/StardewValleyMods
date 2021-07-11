using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;

namespace ManaBar.Framework
{
    /// <summary>Handles migrating legacy data for a save file.</summary>
    internal class LegacyDataMigrator
    {
        /*********
        ** Fields
        *********/
        /// <summary>An API for reading and storing local mod data.</summary>
        private readonly IDataHelper DataHelper;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The legacy save file path for the Magic mod.</summary>
        private string OldMagicFilePath => Path.Combine(Constants.CurrentSavePath, "magic0.2.json");

        /// <summary>The legacy data key in the save file.</summary>
        private readonly string OldSaveKey = "spacechase0.ManaBar.Mana";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="dataHelper"><inheritdoc cref="DataHelper" path="/summary"/></param>
        /// <param name="monitor"><inheritdoc cref="Monitor" path="/summary"/></param>
        public LegacyDataMigrator(IDataHelper dataHelper, IMonitor monitor)
        {
            this.DataHelper = dataHelper;
            this.Monitor = monitor;
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        public void OnSaveLoaded()
        {
            if (!Context.IsMainPlayer)
                return;

            // handle legacy save data
            {
                long[] players = this.TryApply(this.DataHelper.ReadSaveData<LegacySaveData>(this.OldSaveKey)).ToArray();
                if (players.Any())
                    this.Monitor.Log($"Migrated legacy save data for players {string.Join(", ", players)}.");

                this.DataHelper.WriteSaveData<LegacySaveData>(this.OldSaveKey, null);
            }

            // handle legacy data file
            if (File.Exists(this.OldMagicFilePath))
            {
                long[] players = this.TryApply(JsonConvert.DeserializeObject<LegacySaveData>(File.ReadAllText(this.OldMagicFilePath))).ToArray();
                if (players.Any())
                    this.Monitor.Log($"Migrated legacy data file for players {string.Join(", ", players)}.");
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Apply legacy save data to the player's current fields, if valid and the field doesn't already have a value.</summary>
        /// <param name="data">The save data to apply.</param>
        /// <returns>Returns the player IDs whose fields were changed.</returns>
        private IEnumerable<long> TryApply(LegacySaveData data)
        {
            foreach (var pair in data?.Players ?? new())
            {
                if (pair.Value?.Mana is not > 0 && pair.Value?.ManaCap is not > 0)
                    continue;

                Farmer player = Game1.getFarmerMaybeOffline(pair.Key);
                if (this.SetMaxManaIfNotSet(player, pair.Value.ManaCap) | this.SetCurrentManaIfNotSet(player, pair.Value.Mana))
                    yield return pair.Key;
            }
        }

        /// <summary>Set a player's current mana points if valid and they don't already have a value.</summary>
        /// <param name="player">The player to update.</param>
        /// <param name="mana">The value to set.</param>
        /// <returns>Returns whether the field was changed.</returns>
        private bool SetCurrentManaIfNotSet(Farmer player, int mana)
        {
            return this.SetIfNotSet(player, mana, ModDataManager.GetCurrentMana, ModDataManager.SetCurrentMana);
        }

        /// <summary>Set a player's max mana points if valid and they don't already have a value.</summary>
        /// <param name="player">The player to update.</param>
        /// <param name="mana">The value to set.</param>
        /// <returns>Returns whether the field was changed.</returns>
        private bool SetMaxManaIfNotSet(Farmer player, int mana)
        {
            return this.SetIfNotSet(player, mana, ModDataManager.GetMaxMana, ModDataManager.SetMaxMana);
        }

        /// <summary>Set a player's mana field value if valid and it doesn't already have a value.</summary>
        /// <param name="player">The player to update.</param>
        /// <param name="points">The value to set.</param>
        /// <param name="getCurrent">Get the current field value.</param>
        /// <param name="set">Set the field value.</param>
        /// <returns>Returns whether the field was changed.</returns>
        private bool SetIfNotSet(Farmer player, int points, Func<Farmer, int> getCurrent, Action<Farmer, int> set)
        {
            // no value to set, or it already has a value
            if (player == null || points <= 0 || getCurrent(player) > 0)
                return false;

            // set value
            set(player, points);
            return true;

        }

        /// <summary>The data model for mana in the legacy Magic data file, and the legacy Mana Bar save data.</summary>
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "Used for legacy data deserialization.")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Used for legacy data deserialization.")]
        private class LegacySaveData
        {
            public Dictionary<long, PlayerData> Players { get; set; }

            public class PlayerData
            {
                public int Mana { get; set; }
                public int ManaCap { get; set; }
            }
        }
    }
}

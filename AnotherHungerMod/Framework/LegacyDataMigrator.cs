using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace AnotherHungerMod.Framework
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

        /// <summary>The internal prefix for Another Hunger Mod's save keys in the raw save data.</summary>
        private readonly string InternalSaveKeyPrefix = "smapi/mod-data/spacechase0.anotherhungermod/";


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
            if (!Context.IsMainPlayer || Game1.CustomData == null)
                return;

            // migrate each key
            foreach (string saveKey in this.GetSaveKeys())
            {
                if (this.TryLoadSaveData(saveKey, out long playerId, out LegacySaveData data))
                {
                    Farmer player = Game1.getFarmerMaybeOffline(playerId);
                    if (player != null)
                    {
                        if (!player.HasFedSpouse())
                            player.SetFedSpouse(data.FedSpouseMeal);
                        if (player.GetFullness() == 0)
                            ModDataManager.SetFullness(player, (float)data.Fullness);
                    }
                }

                this.DataHelper.WriteSaveData(saveKey, null as object);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the mod's keys in the save data.</summary>
        private string[] GetSaveKeys()
        {
            ISet<string> saveKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string key in Game1.CustomData.Keys)
            {
                if (key.StartsWith(this.InternalSaveKeyPrefix, StringComparison.OrdinalIgnoreCase))
                    saveKeys.Add(key.Substring(this.InternalSaveKeyPrefix.Length));
            }

            return saveKeys.ToArray();
        }

        /// <summary>Load legacy save data with the given key, if it's valid.</summary>
        /// <param name="key">The save key to load.</param>
        /// <param name="playerId">The parsed <see cref="Farmer.UniqueMultiplayerID"/> value.</param>
        /// <param name="data">The save data for the player.</param>
        private bool TryLoadSaveData(string key, out long playerId, out LegacySaveData data)
        {
            const string prefix = "spacechase0.anotherhungermod.";

            // extract player ID
            if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                playerId = 0;
                data = null;
                return false;
            }
            {
                string raw = key.Substring(prefix.Length);
                if (!long.TryParse(raw, out playerId))
                {
                    playerId = 0;
                    data = null;
                    return false;
                }
            }

            // load data
            data = this.DataHelper.ReadSaveData<LegacySaveData>(key);
            return data != null;
        }

        /// <summary>The data model for the legacy save data.</summary>
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "Used for legacy data deserialization.")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Used for legacy data deserialization.")]
        internal class LegacySaveData
        {
            public double Fullness { get; set; }
            public bool FedSpouseMeal { get; set; }
        }
    }
}

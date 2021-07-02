using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JsonNet.PrivateSettersContractResolvers;
using Newtonsoft.Json;
using SkillPrestige.Logging;
using StardewValley;

namespace SkillPrestige
{
    /// <summary>
    /// Represents the save file data for the Skill Prestige Mod.
    /// </summary>
    [Serializable]
    public class PrestigeSaveData
    {
        private const string DataFileName = @"Data.json";
        private static readonly string DataFilePath = Path.Combine(SkillPrestigeMod.ModPath, DataFileName);
        public static PrestigeSet CurrentlyLoadedPrestigeSet => Instance.PrestigeSaveFiles[CurrentlyLoadedSaveFileUniqueId];
        private static ulong CurrentlyLoadedSaveFileUniqueId { get; set; }

        private static PrestigeSaveData _instance;

        /// <summary>
        /// Set of prestige data saved per save file unique ID.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global - no, it can't be made private or it won't be serialized.
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global - setter used by deserializer.
        public IDictionary<ulong, PrestigeSet> PrestigeSaveFiles { get; set; }
        
        private PrestigeSaveData()
        {
            PrestigeSaveFiles = new Dictionary<ulong, PrestigeSet>();
            Logger.LogInformation("Created new prestige save data instance.");
        }

        // ReSharper disable once MemberCanBePrivate.Global - used publically, resharper is wrong.
        public static PrestigeSaveData Instance
        {
            get => _instance ?? (_instance = new PrestigeSaveData());
            // ReSharper disable once UnusedMember.Global - used by deseralizer.
            set => _instance = value;
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global - removing this removes lazy load in accessor for the instance. 
        public void Save()
        {
            Logger.LogInformation("Writing prestige save data to disk...");
            File.WriteAllLines(DataFilePath, new[] { JsonConvert.SerializeObject(Instance) });
            Logger.LogInformation("Prestige save data written to disk.");
        }

        public void Read()
        {
            if (!File.Exists(DataFilePath)) SetupDataFile();
            Logger.LogInformation("Deserializing prestige save data...");
            var settings = new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver()};
            _instance = JsonConvert.DeserializeObject<PrestigeSaveData>(File.ReadAllText(DataFilePath), settings);
            Logger.LogInformation("Prestige save data loaded.");
        }

        private void UpdatePrestigeSkillsForCurrentFile()
        {
            Logger.LogVerbose("Checking for missing prestige data...");
            var missingPrestiges = PrestigeSet.CompleteEmptyPrestigeSet.Prestiges.Where(x => !CurrentlyLoadedPrestigeSet.Prestiges.Select(y => y.SkillType).Contains(x.SkillType)).ToList();
            if (!missingPrestiges.Any()) return;
            Logger.LogInformation("Missing Prestige data found. Loading new prestige data...");
            var prestiges = new List<Prestige>(CurrentlyLoadedPrestigeSet.Prestiges);
            prestiges.AddRange(missingPrestiges);
            CurrentlyLoadedPrestigeSet.Prestiges = prestiges;
            Save();
            Logger.LogInformation("Missing Prestige data loaded.");
        }

        private void SetupDataFile()
        {
            Logger.LogInformation("Creating new data file...");
            try
            {
                Save();
            }
            catch (Exception exception)
            {
                Logger.LogCritical($"An error occured while attempting to create a data file. details: {Environment.NewLine} {exception}");
                throw;
            }
            Logger.LogInformation("Successfully created new data file.");
        }

        public void UpdateCurrentSaveFileInformation()
        {
            if (CurrentlyLoadedSaveFileUniqueId == Game1.uniqueIDForThisGame) return;
            Logger.LogInformation("Save file change detected.");
            if (!Instance.PrestigeSaveFiles.ContainsKey(Game1.uniqueIDForThisGame))
            {
                Instance.PrestigeSaveFiles.Add(Game1.uniqueIDForThisGame, PrestigeSet.CompleteEmptyPrestigeSet);
                Save();   
                Logger.LogInformation($"Save file not found in list, adding save file to prestige data. Id = {Game1.uniqueIDForThisGame}");
            }
            CurrentlyLoadedSaveFileUniqueId = Game1.uniqueIDForThisGame;
            UpdatePrestigeSkillsForCurrentFile();
            Read();
        }

    }
}

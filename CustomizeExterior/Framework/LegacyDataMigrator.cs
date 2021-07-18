using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;

namespace CustomizeExterior.Framework
{
    /// <summary>Handles migrating legacy data.</summary>
    internal static class LegacyDataMigrator
    {
        /*********
        ** Fields
        *********/
        /// <summary>The legacy key in the save data.</summary>
        private const string LegacyDataKay = "building-exteriors";

        /// <summary>The absolute path to the legacy file containing building exteriors.</summary>
        private static string LegacyFilePath => Path.Combine(Constants.CurrentSavePath, "building-exteriors.json");



        /*********
        ** Public methods
        *********/
        /// <summary>Migrate data when the save is loaded.</summary>
        /// <param name="dataHelper">The SMAPI data helper.</param>
        public static void OnSaveLoaded(IDataHelper dataHelper)
        {
            if (!Context.IsMainPlayer)
                return;

            // load legacy save/file data
            SavedExteriors savedExteriors = dataHelper.ReadSaveData<SavedExteriors>(LegacyDataMigrator.LegacyDataKay);
            string source = "save data";
            if (savedExteriors == null)
            {
                FileInfo legacyFile = new(LegacyFilePath);
                if (legacyFile.Exists)
                {
                    savedExteriors = JsonConvert.DeserializeObject<SavedExteriors>(File.ReadAllText(legacyFile.FullName));
                    source = "data file";
                }
            }

            // migrate to modData fields
            if (savedExteriors?.Chosen?.Any() == true)
            {
                Log.Info($"Migrating legacy {source} to the current format...");

                Farm farm = Game1.getFarm();
                foreach (var pair in savedExteriors.Chosen)
                {
                    // get info
                    string key = pair.Key;
                    string folderName = pair.Value;
                    if (key == "null" || string.IsNullOrWhiteSpace(folderName) || folderName == "/")
                        continue;

                    // remove seasonal indicator (now handled automatically)
                    if (folderName.StartsWith("%"))
                        folderName = folderName.Substring(1);

                    // migrate
                    if (string.Equals(key, "FarmHouse", StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Trace($"Setting farmhouse to '{folderName}'...");
                        farm.SetFarmhouseAssetPackName(folderName);
                    }
                    else
                    {
                        bool found = false;

                        foreach (var building in farm.buildings)
                        {
                            string buildingKey = building is GreenhouseBuilding ? "Greenhouse" : building.nameOfIndoors;
                            if (string.Equals(key, buildingKey, StringComparison.OrdinalIgnoreCase))
                            {
                                Log.Trace($"Setting '{buildingKey}' building at ({building.tileX.Value}, {building.tileY.Value}) to '{folderName}'...");
                                building.SetAssetPack(folderName);
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                            Log.Trace($"Ignored saved exterior ('{key}': '{folderName}') because no matching building was found.");
                    }
                }

                Log.Trace("Migration complete!");
            }

            // remove legacy save data
            dataHelper.WriteSaveData(LegacyDataMigrator.LegacyDataKay, null as SavedExteriors);
        }

        /// <summary>Migrate data when the save is written.</summary>
        public static void OnSaved()
        {
            if (!Context.IsMainPlayer)
                return;

            // remove legacy data file
            FileInfo legacyFile = new(LegacyFilePath);
            if (legacyFile.Exists)
                legacyFile.Delete();
        }

        /// <summary>The data model for the legacy save data.</summary>
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local", Justification = "Used for legacy data deserialization.")]
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "Used for legacy data deserialization.")]
        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local", Justification = "Used for legacy data deserialization.")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Used for legacy data deserialization.")]
        private class SavedExteriors
        {
            public Dictionary<string, string> Chosen { get; set; } = new();
        }
    }
}

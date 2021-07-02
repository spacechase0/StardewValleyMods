using System;
using System.IO;
using JsonNet.PrivateSettersContractResolvers;
using Newtonsoft.Json;
using SkillPrestige.Logging;

namespace SkillPrestige
{
    /// <summary>
    /// Represents options for this mod.
    /// </summary>
    [Serializable]
    public class Options
    {
        /// <summary>
        /// The logging verbosity for the mod. A log level set to Verbose will log all entries.
        /// </summary>
        public LogLevel LogLevel { get; private set; }

        /// <summary>
        /// Whether or not testing mode is enabled, which adds testing specific commands to the system.
        /// </summary>
        public bool TestingMode { get; set; }

        private Options() { }
        private static Options _instance;
        public static Options Instance 
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new Options();
                LoadOptions();
                return _instance;
            }
        }

       private static void LoadOptions()
        {
            Logger.LogDisplay($"options file path: {SkillPrestigeMod.OptionsPath}");
            if (!File.Exists(SkillPrestigeMod.OptionsPath)) SetupOptionsFile();
            var settings = new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver() };
            Logger.LogDisplay("Deserializing options file...");
            _instance = JsonConvert.DeserializeObject<Options>(File.ReadAllText(SkillPrestigeMod.OptionsPath), settings);
            Logger.LogInformation("Options loaded.");
        }

        private static void SetupOptionsFile()
        {
            Logger.LogDisplay("Creating new options file...");
            try
            {
                Instance.LogLevel = LogLevel.Warning;
                Save();
            }
            catch(Exception exception)
            {
                Logger.LogOptionsError($"Error while attempting to create an options file. {Environment.NewLine} {exception}");
                throw;
            }
            Logger.LogInformation("Successfully created new options file.");
        }

        private static void Save()
        {
            File.WriteAllLines(SkillPrestigeMod.OptionsPath, new[] { JsonConvert.SerializeObject(_instance) });
            Logger.LogInformation("Options file saved.");
        }

    }
}
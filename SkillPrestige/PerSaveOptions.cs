using System;
using System.IO;
using JsonNet.PrivateSettersContractResolvers;
using Newtonsoft.Json;
using SkillPrestige.Logging;

namespace SkillPrestige
{
    /// <summary>
    /// Represents options for this mod per save file.
    /// </summary>
    [Serializable]
    public class PerSaveOptions
    {
        /// <summary>
        /// Whether or not to reset the recipes of a skill on load.
        /// </summary>
        public bool ResetRecipesOnPrestige { get; set; }

        public bool UseExperienceMultiplier { get; set; }

        public decimal ExperienceMultiplier { get; private set; }

        public int CostOfTierOnePrestige { get; set; }

        public int CostOfTierTwoPrestige { get; set; }

        /// <summary>
        /// Number of prestige points gained per prestige of a skill.
        /// </summary>
        public int PointsPerPrestige { get; set; }
        
        /// <summary>
        /// A mode where the player pays for prestige via the points gained after reaching level 10, never resetting to 0.
        /// </summary>
        public bool PainlessPrestigeMode { get; set; }

        public int ExperienceNeededPerPainlessPrestige { get; set; }

        private PerSaveOptions() { }
        private static PerSaveOptions _instance;

        public static PerSaveOptions Instance 
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new PerSaveOptions();
                LoadPerSaveOptions();
                return _instance;
            }
        }

        private static void LoadPerSaveOptions()
        {
            Logger.LogInformation($"per save options file path: {SkillPrestigeMod.CurrentSaveOptionsPath}");
            if (!File.Exists(SkillPrestigeMod.CurrentSaveOptionsPath)) SetupPerSaveOptionsFile();
            var settings = new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver() };
            Logger.LogInformation("Deserializing per save options file...");
            try
            {
                _instance = JsonConvert.DeserializeObject<PerSaveOptions>(File.ReadAllText(SkillPrestigeMod.CurrentSaveOptionsPath), settings);
                if (Instance.CostOfTierOnePrestige <= 0)
                {
                    Logger.LogWarning("Tier one prestige cost loaded without value, defaulting to a cost of 1.");
                    Instance.CostOfTierOnePrestige = 1;
                }
                if (Instance.CostOfTierTwoPrestige <= 0)
                {
                    Logger.LogWarning("Tier two prestige cost loaded without value, defaulting to a cost of 2.");
                    Instance.CostOfTierTwoPrestige = 2;
                }
                if (Instance.ExperienceMultiplier <= 0)
                {
                    Logger.LogWarning("Experience Multiplier loaded without value, defaulting to 10%, turning on experience multiplier usage.");
                    Instance.ExperienceMultiplier = 0.1m;
                    Instance.UseExperienceMultiplier = true;
                }
                if (Instance.PointsPerPrestige <= 0)
                {
                    Logger.LogWarning("Points per prestige loaded without value, defaulting to a 1 point per prestige.");
                    Instance.PointsPerPrestige = 1;
                }
                if (Instance.ExperienceNeededPerPainlessPrestige <= 0)
                {
                    Logger.LogWarning("Experience points needed per painless prestige loaded without value, defaulting to 15000 points per painless prestige.");
                    Instance.ExperienceNeededPerPainlessPrestige = 15000;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error deserializing per-save options file. {Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}"); 
                Logger.LogInformation(" Attempting to create new per-save options file...");
                SetupPerSaveOptionsFile();
            }
            Logger.LogInformation("Per save options loaded.");
        }

        private static void SetupPerSaveOptionsFile()
        {
            Logger.LogInformation("Creating new options file...");
            try
            {
                Instance.ResetRecipesOnPrestige =  true;
                Instance.UseExperienceMultiplier =  true;
                Instance.ExperienceMultiplier =  0.1m;
                Instance.CostOfTierOnePrestige =  1;
                Instance.CostOfTierTwoPrestige =  2;
                Instance.PointsPerPrestige =  1;
                Instance.ExperienceNeededPerPainlessPrestige = 15000;
                Save();
            }
            catch(Exception exception)
            {
                Logger.LogError($"Error while attempting to create a per save options file. {Environment.NewLine} {exception}");
                throw;
            }
            Logger.LogInformation("Successfully created new per save options file.");
        }

        public static void Save()
        {
            Directory.CreateDirectory(SkillPrestigeMod.PerSaveOptionsDirectory);
            File.WriteAllLines(SkillPrestigeMod.CurrentSaveOptionsPath, new[] { JsonConvert.SerializeObject(_instance) });
            Logger.LogInformation("Per save options file saved.");
        }

        public static void ClearLoadedPerSaveOptionsFile()
        {
            _instance = null;
        }

        /// <summary>
        /// Empty procedure to force the lazy load of the instance.
        /// </summary>
        // ReSharper disable once MemberCanBeMadeStatic.Global - the whole point of this is to force the load of the instance.
        public void Check() { }
    }
}
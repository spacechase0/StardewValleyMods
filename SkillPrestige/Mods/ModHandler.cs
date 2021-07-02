using System;
using System.Collections.Generic;
using System.Linq;
using SkillPrestige.Logging;

namespace SkillPrestige.Mods
{
    /// <summary>
    /// Register your skill mod with this class to add it to the prestige system.
    /// </summary>
    public static class ModHandler
    {
        private static readonly List<ISkillMod> Mods = new List<ISkillMod>();

        /// <summary>
        /// Registers another skill mod to work with the skill prestige system.
        /// </summary>
        /// <param name="mod">The mod you wish to register. the mod and its profession Ids cannot already exist in the system, 
        /// and the mod must implement ISkillMod. It is recommended to inherit from SkillPrestige's SkillMod class.</param>
        public static void RegisterMod(ISkillMod mod)
        {
            if (!mod.IsFound)
            {
                Logger.LogInformation($"{mod.DisplayName} Mod not found. Mod not registered.");
                return;
            }
            try
            {
                Logger.LogInformation($"Registering mod: {mod.DisplayName} ...");
                if (Mods.Any(x => x.GetType() == mod.GetType()))
                {
                    Logger.LogWarning($"Cannot load mod: {mod.DisplayName}, as it is already loaded.");
                    return;
                }
                var intersectingMods = GetIntersectingModProfessions(mod);
                if (intersectingMods.Any())
                {
                    Logger.LogWarning($"Cannot load skill mod: {mod.DisplayName}, as it collides with another mod's skills. Details:");
                    foreach (var intersectingMod in intersectingMods)
                    {
                        Logger.LogWarning($"Skill mod {mod.DisplayName} registration failed due to {intersectingMod.Key.DisplayName}, for profession ids: {string.Join(",", intersectingMod.Value)}");
                    }
                    return;
                }
                Mods.Add(mod);
                Logger.LogInformation($"Registered mod: {mod.DisplayName}");
            }
            catch (Exception exception)
            {
                Logger.LogWarning($"Failed to register mod. please ensure mod implements the ISKillMod interface correctly and none of its members generate errors when called. {Environment.NewLine}{exception.Message}{Environment.NewLine}{exception.StackTrace}");
            }
        }


        /// <summary>
        /// Returns the set of mods with profession Ids that collide with already loaded professions Ids.
        /// </summary>
        /// <param name="mod"></param>
        /// <returns></returns>
        private static IDictionary<ISkillMod, IEnumerable<int>> GetIntersectingModProfessions(ISkillMod mod)
        {
            var intersectingMods = new Dictionary<ISkillMod, IEnumerable<int>>();
            Logger.LogInformation($"Loaded mods: {Mods.Count}");
            foreach (var loadedMod in Mods)
            {
                var loadedModProfessions = loadedMod.AdditionalSkills?.SelectMany(x => x.GetAllProfessionIds());
                if (loadedModProfessions == null) continue;
                var modProfessions = mod.AdditionalSkills.SelectMany(x => x.GetAllProfessionIds());
                var intersectingProfessions = loadedModProfessions.Intersect(modProfessions).ToList();
                Logger.LogInformation($"intersecting profession for {loadedMod.DisplayName} and {mod.DisplayName}: {intersectingProfessions.Count}");
                if (intersectingProfessions.Any())
                {
                    intersectingMods.Add(loadedMod, intersectingProfessions);
                }
            }
            return intersectingMods;
        }

        /// <summary>
        /// returns the sets of empty prestiges from mods for saving.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Prestige> GetAddedEmptyPrestiges()
        {
            return Mods.Where(x => x.AdditonalPrestiges != null).SelectMany(x => x.AdditonalPrestiges);
        }

        /// <summary>
        /// returns the sets of skills added by other mods.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Skill> GetAddedSkills()
        {
            return Mods.Where(x => x.AdditionalSkills != null).SelectMany(x => x.AdditionalSkills);
        }

    }
}

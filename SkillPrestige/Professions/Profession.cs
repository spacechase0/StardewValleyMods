using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkillPrestige.Logging;
using SkillPrestige.Professions.Registration;
using StardewValley;
using StardewValley.Menus;

namespace SkillPrestige.Professions
{

    /// <summary>
    /// Represents a profession in Stardew Valley.
    /// </summary>
    public abstract partial class Profession
    {
        /// <summary>
        /// Static constructor to, when the profession class is first called, use reflection to register all static profession variables declared in partial methods.
        /// </summary>
        static Profession()
        {
            Logger.LogInformation("Registering professions...");
            //gets all non abstract classes that implement IProfessionRegistration.
            var concreteProfessionRegistrations = AppDomain.CurrentDomain.GetNonSystemAssemblies().SelectMany(x => x.GetTypesSafely())
                .Where(x => typeof(IProfessionRegistration).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract).ToList();
            Logger.LogVerbose($"{concreteProfessionRegistrations.Count} concrete profession registrations found.");
            foreach (var registration in concreteProfessionRegistrations)
            {
                ((IProfessionRegistration) Activator.CreateInstance(registration)).RegisterProfessions();
            }

            foreach (var profession in Skill.AllSkills.Where(x => Skill.DefaultSkills.Select(y => y.Type).Contains(x.Type)).SelectMany(x => x.Professions).Where(x => string.IsNullOrWhiteSpace(x.DisplayName) || x.EffectText == null || !x.EffectText.Any()))
            {
                Logger.LogVerbose($"getting display text from the game for profession id {profession.Id}");
                if (string.IsNullOrWhiteSpace(profession.DisplayName))
                {
                    profession.DisplayName = typeof(LevelUpMenu).InvokeStaticFunction<string>("getProfessionName", profession.Id);
                }
                if (profession.EffectText == null || !profession.EffectText.Any())
                {
                    profession.EffectText = Game1.content.LoadString($@"Strings\UI:LevelUp_ProfessionDescription_{profession.DisplayName}").Split('\n');
                }
            }
            Logger.LogInformation("Professions registered.");
        }

        protected Profession()
        {
            EffectText = new List<string>();
        }

        /// <summary>
        /// The profession Id used by Stardew Valley.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The level the profession is available at, should only be 5 or 10.
        /// </summary>
        public abstract int LevelAvailableAt { get; }
        
        public string DisplayName { get; set; }

        public IEnumerable<string> EffectText { get; set; }

        /// <summary>
        /// Any special handling the profession may need in order to be implemented and/or removed.
        /// </summary>
        public IProfessionSpecialHandling SpecialHandling { get; set; }


        /// <summary>
        /// The location of the professions icon in Stardew Valley's cursors texture file image.
        /// If you are implementing this for your own mod, override this and supply the source rectangle for the texture you wish to use.
        /// If you have a custom texture, also override the Texture property.
        /// </summary>
        // ReSharper disable once VirtualMemberNeverOverridden.Global - expected to be overridden in externally inherited class.
        public virtual Rectangle IconSourceRectangle => new Rectangle(Id % 6 * 16, 624 + Id / 6 * 16, 16, 16);

        /// <summary>
        /// The texture that contains the profession's icon.
        /// </summary>
        // ReSharper disable once VirtualMemberNeverOverridden.Global - expected to be overridden in externally inherited class.
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global - expected to be set in external project.
        public virtual Texture2D Texture { get; set; } = Game1.mouseCursors;


        /// <summary>
        /// Removes all professions from a skill.
        /// </summary>
        /// <param name="skill">the skill you wish to remove all professions for.</param>
        public static void RemoveProfessions(Skill skill)
        {
            Logger.LogInformation($"Removing all professions for {skill.Type.Name} skill from player...");
            foreach (var profession in skill.Professions.Where(x => Game1.player.professions.Contains(x.Id)))
            {
                Logger.LogVerbose($"Removing {profession.DisplayName} profession from player.");
                Game1.player.professions.Remove(profession.Id);
                profession.SpecialHandling?.RemoveEffect();
            }
            Logger.LogInformation($"All professions for {skill.Type.Name} skill removed from player.");
        }
        
        /// <summary>
        /// Adds all professions that should exist for the player based upon their prestige data.
        /// </summary>
        public static void AddMissingProfessions()
        {
            Logger.LogInformation("Adding professions that should be loaded.");
            var professions = Game1.player.professions;
            foreach (var profession in Skill.AllSkills.SelectMany(x => x.Professions).Where(x => PrestigeSaveData.CurrentlyLoadedPrestigeSet.Prestiges.SelectMany(y => y.PrestigeProfessionsSelected).Contains(x.Id)))
            {
                if (professions.Contains(profession.Id))
                {
                    Logger.LogVerbose($"Profession {profession.DisplayName} already found.");
                    continue;
                }
                professions.Add(profession.Id);
                Logger.LogVerbose($"Profession {profession.DisplayName} added.");
                profession.SpecialHandling?.ApplyEffect();
            }
        }
    }
}

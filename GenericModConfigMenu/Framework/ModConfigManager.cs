using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace GenericModConfigMenu.Framework
{
    /// <summary>Manages the registered mod configurations.</summary>
    internal class ModConfigManager
    {
        /*********
        ** Fields
        *********/
        /// <summary>The registered mod config menus.</summary>
        private readonly Dictionary<string, ModConfig> Configs = new(StringComparer.OrdinalIgnoreCase);


        /*********
        ** Public methods
        *********/
        /// <summary>Get the config menu metadata for a mod.</summary>
        /// <param name="manifest">The mod's manifest instance.</param>
        /// <param name="assert">Whether to throw an exception if the mod doesn't have a registered config.</param>
        /// <exception cref="KeyNotFoundException">The mod doesn't have a registered config, and <paramref name="assert"/> is true.</exception>
        public ModConfig Get(IManifest manifest, bool assert)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            if (this.Configs.TryGetValue(manifest.UniqueID, out ModConfig value))
                return value;

            return assert
                ? throw new KeyNotFoundException($"The '{manifest.Name}' mod hasn't registered a config menu.")
                : null;
        }

        /// <summary>Get all registered config menus.</summary>
        public IEnumerable<ModConfig> GetAll()
        {
            return this.Configs.Values;
        }

        /// <summary>Set the config menu metadata for a mod.</summary>
        /// <param name="manifest">The mod's manifest instance.</param>
        /// <param name="config">The config menu metadata.</param>
        public void Set(IManifest manifest, ModConfig config)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            this.Configs[manifest.UniqueID] = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>Remove the config menu metadata for a mod.</summary>
        /// <param name="manifest">The mod's manifest instance.</param>
        public void Remove(IManifest manifest)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            this.Configs.Remove(manifest.UniqueID);
        }
    }
}

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
        /// <exception cref="ArgumentNullException">The manifest is null.</exception>
        /// <exception cref="ArgumentException">The manifest is missing required fields.</exception>
        /// <exception cref="KeyNotFoundException">The mod doesn't have a registered config, and <paramref name="assert"/> is true.</exception>
        public ModConfig Get(IManifest manifest, bool assert)
        {
            this.AssertManifest(manifest);

            lock (this.Configs)
            {
                if (this.Configs.TryGetValue(manifest.UniqueID, out ModConfig value))
                    return value;

                return assert
                    ? throw new KeyNotFoundException($"The '{manifest.Name}' mod hasn't registered a config menu.")
                    : null;
            }
        }

        /// <summary>Get all registered config menus.</summary>
        public IEnumerable<ModConfig> GetAll()
        {
            lock (this.Configs)
            {
                return this.Configs.Values;
            }
        }

        /// <summary>Set the config menu metadata for a mod.</summary>
        /// <param name="manifest">The mod's manifest instance.</param>
        /// <param name="config">The config menu metadata.</param>
        /// <exception cref="ArgumentNullException">The manifest or config is null.</exception>
        /// <exception cref="ArgumentException">The manifest is missing required fields.</exception>
        public void Set(IManifest manifest, ModConfig config)
        {
            lock (this.Configs)
            {
                this.AssertManifest(manifest);

                this.Configs[manifest.UniqueID] = config ?? throw new ArgumentNullException(nameof(config));
            }
        }

        /// <summary>Remove the config menu metadata for a mod.</summary>
        /// <param name="manifest">The mod's manifest instance.</param>
        /// <exception cref="ArgumentNullException">The manifest is null.</exception>
        /// <exception cref="ArgumentException">The manifest is missing required fields.</exception>
        public void Remove(IManifest manifest)
        {
            lock (this.Configs)
            {
                this.AssertManifest(manifest);

                if (this.Configs.ContainsKey(manifest.UniqueID))
                    this.Configs.Remove(manifest.UniqueID);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Assert that the given manifest is valid.</summary>
        /// <param name="manifest">The manifest to validate.</param>
        /// <exception cref="ArgumentNullException">The manifest is null.</exception>
        /// <exception cref="ArgumentException">The manifest is missing required fields.</exception>
        private void AssertManifest(IManifest manifest)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));
            if (string.IsNullOrWhiteSpace(manifest.UniqueID))
                throw new ArgumentException($"The '{manifest.Name}' mod manifest doesn't have a unique ID value.", nameof(manifest));
            if (string.IsNullOrWhiteSpace(manifest.Name))
                throw new ArgumentException($"The '{manifest.UniqueID}' mod manifest doesn't have a name value.", nameof(manifest));
        }
    }
}

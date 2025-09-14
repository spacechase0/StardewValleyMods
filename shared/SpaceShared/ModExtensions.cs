using System;
using System.Collections.Generic;
using SpaceShared.APIs;
using StardewModdingAPI;

namespace SpaceShared
{
    /// <summary>Provides common extensions for general mod logic.</summary>
    internal static class ModExtensions
    {
        /****
        ** SMAPI
        ****/
        /// <inheritdoc cref="IModRegistry.GetApi{T}"/>
        /// <param name="modRegistry">The mod registry to extend.</param>
        /// <param name="uniqueId">The mod's unique ID.</param>
        /// <param name="label">A human-readable name for the mod.</param>
        /// <param name="minVersion">The minimum supported version of the API.</param>
        /// <param name="monitor">The monitor with which to log errors.</param>
        public static TInterface GetApi<TInterface>(this IModRegistry modRegistry, string uniqueId, string label, string minVersion, IMonitor monitor) where TInterface : class
        {
            // fetch mod info
            IManifest manifest = modRegistry.Get(uniqueId)?.Manifest;
            if (manifest == null)
                return null;

            // check mod version
            if (manifest.Version.IsOlderThan(minVersion))
            {
                monitor.Log($"Detected {label} {manifest.Version}, but need {minVersion} or later. Disabled integration with this mod.", LogLevel.Warn);
                return null;
            }

            // fetch API
            TInterface api = modRegistry.GetApi<TInterface>(uniqueId);
            if (api == null)
            {
                monitor.Log($"Detected {label}, but couldn't fetch its API. Disabled integration with this mod.", LogLevel.Warn);
                return null;
            }

            return api;
        }

        /// <summary>Get the mod API for Generic Mod Config Menu, if it's loaded and compatible.</summary>
        /// <param name="modRegistry">The mod registry to extend.</param>
        /// <param name="monitor">The monitor with which to log errors.</param>
        /// <returns>Returns the API instance if available, else <c>null</c>.</returns>
        public static IGenericModConfigMenuApi GetGenericModConfigMenuApi(this IModRegistry modRegistry, IMonitor monitor)
        {
            return modRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu", "Generic Mod Config Menu", "1.8.0", monitor);
        }
    }
}

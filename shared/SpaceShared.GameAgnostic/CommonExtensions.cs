using System;
using System.Collections.Generic;
//using SpaceShared.APIs;
//using StardewModdingAPI;

namespace SpaceShared
{
    /// <summary>Provides common extensions for general mod logic.</summary>
    internal static class CommonExtensions
    {
        /*********
        ** Public methods
        *********/
        public static void Shuffle<T>(this List<T> list, Random r = null)
        {
            r ??= new Random();
            for (int i = 0; i < list.Count; ++i)
            {
                int ri = r.Next(list.Count);

                var tmp = list[i];
                list[i] = list[ri];
                list[ri] = tmp;
            }
        }

        /****
        ** Objects
        ****/
        /// <summary>
        /// Apparently, in .NET Core, a hash code for a given string will be different between runs.
        /// https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
        /// This gets one that will be the same.
        /// </summary>
        /// <param name="str">The string to get the hash code of.</param>
        /// <returns>The deterministic hash code.</returns>
        public static int GetDeterministicHashCode(this string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }


        /****
        ** Arrays
        ****/
        /// <summary>Get a value from an array if it's in range.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="array">The array to search.</param>
        /// <param name="index">The index of the value within the array to find.</param>
        /// <param name="value">The value at the given index, if found.</param>
        /// <returns>Returns whether the index was within the array bounds.</returns>
        public static bool TryGetIndex<T>(this T[] array, int index, out T value)
        {
            if (array == null || index < 0 || index >= array.Length)
            {
                value = default;
                return false;
            }

            value = array[index];
            return true;
        }

        /// <summary>Get a value from an array if it's in range, else get the default value.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="array">The array to search.</param>
        /// <param name="index">The index of the value within the array to find.</param>
        /// <param name="defaultValue">The default value if the value isn't in range.</param>
        public static T GetOrDefault<T>(this T[] array, int index, T defaultValue = default)
        {
            return array.TryGetIndex(index, out T value)
                ? value
                : defaultValue;
        }

        /// <summary>Get a value from an array if it's in range and can be parsed, else get the default value.</summary>
        /// <typeparam name="TRaw">The raw value type.</typeparam>
        /// <typeparam name="TParsed">The parsed value type.</typeparam>
        /// <param name="array">The array to search.</param>
        /// <param name="index">The index of the value within the array to find.</param>
        /// <param name="tryParse">Try to parse the raw value</param>
        /// <param name="defaultValue">The default value if the value isn't in range or isn't valid.</param>
        public static TParsed GetOrDefault<TRaw, TParsed>(this TRaw[] array, int index, Func<TRaw, TParsed> tryParse, TParsed defaultValue = default)
        {
            if (!array.TryGetIndex(index, out TRaw value))
                return defaultValue;

            try
            {
                return tryParse(value);
            }
            catch
            {
                return defaultValue;
            }
        }


        /****
        ** Dictionaries
        ****/
        /// <summary>Get a value from a dictionary if it exists, else get the default value.</summary>
        /// <typeparam name="TKey">The dictionary key type.</typeparam>
        /// <typeparam name="TValue">The dictionary value type.</typeparam>
        /// <param name="dictionary">The dictionary to search.</param>
        /// <param name="key">The key within the dictionary to find.</param>
        /// <param name="defaultValue">The default value if the value isn't in range.</param>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            return dictionary.TryGetValue(key, out TValue value)
                ? value
                : defaultValue;
        }

        /// <summary>Get a value from a dictionary if it exists, else get the default value.</summary>
        /// <typeparam name="TKey">The dictionary key type.</typeparam>
        /// <typeparam name="TValue">The dictionary value type.</typeparam>
        /// <typeparam name="TParsed">The parsed value type.</typeparam>
        /// <param name="dictionary">The dictionary to search.</param>
        /// <param name="key">The key within the dictionary to find.</param>
        /// <param name="tryParse">Try to parse the raw value</param>
        /// <param name="defaultValue">The default value if the value isn't in range.</param>
        public static TParsed GetOrDefault<TKey, TValue, TParsed>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue, TParsed> tryParse, TParsed defaultValue = default)
        {
            if (!dictionary.TryGetValue(key, out TValue value))
                return defaultValue;

            try
            {
                return tryParse(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        /****
        ** SMAPI
        ****/
        /*
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
        */
    }
}

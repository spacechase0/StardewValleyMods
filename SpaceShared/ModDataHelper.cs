using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using StardewValley;
using StardewValley.Mods;

namespace SpaceShared
{
    /// <summary>Provides common utility methods for reading and writing to <see cref="ModDataDictionary"/> fields.</summary>
    internal static class ModDataHelper
    {
        /*********
        ** Public methods
        *********/
        /****
        ** Bool
        ****/
        /// <summary>Read a boolean value from the mod data if it exists and is valid, else get the default value.</summary>
        /// <param name="data">The mod data dictionary.</param>
        /// <param name="key">The data key within the <paramref name="data"/>.</param>
        /// <param name="default">The default value if the field is missing or invalid.</param>
        public static bool GetBool(this ModDataDictionary data, string key, bool @default = false)
        {
            return data.TryGetValue(key, out string raw) && bool.TryParse(raw, out bool value)
                ? value
                : @default;
        }

        /// <summary>Write a boolean value into the mod data, or remove it if it matches the <paramref name="default"/>.</summary>
        /// <param name="data">The mod data dictionary.</param>
        /// <param name="key">The data key within the <paramref name="data"/>.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="default">The default value if the field is missing or invalid. If the value matches the default, it won't be written to the data to avoid unneeded serialization and network sync.</param>
        public static void SetBool(this ModDataDictionary data, string key, bool value, bool @default = false)
        {
            if (value == @default)
                data.Remove(key);
            else
                data[key] = value.ToString(CultureInfo.InvariantCulture);
        }

        /****
        ** Float
        ****/
        /// <summary>Read a float value from the mod data if it exists and is valid, else get the default value.</summary>
        /// <param name="data">The mod data dictionary.</param>
        /// <param name="key">The data key within the <paramref name="data"/>.</param>
        /// <param name="default">The default value if the field is missing or invalid.</param>
        /// <param name="min">The minimum value to consider valid, or <c>null</c> to allow any value.</param>
        public static float GetFloat(this ModDataDictionary data, string key, float @default = 0, float? min = null)
        {
            return data.TryGetValue(key, out string raw) && float.TryParse(raw, out float value) && value >= min
                ? value
                : @default;
        }

        /// <summary>Write a float value into the mod data, or remove it if it matches the <paramref name="default"/>.</summary>
        /// <param name="data">The mod data dictionary.</param>
        /// <param name="key">The data key within the <paramref name="data"/>.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="default">The default value if the field is missing or invalid. If the value matches the default, it won't be written to the data to avoid unneeded serialization and network sync.</param>
        /// <param name="min">The minimum value to consider valid, or <c>null</c> to allow any value.</param>
        /// <param name="max">The maximum value to consider valid, or <c>null</c> to allow any value.</param>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "We're comparing to a marker value, so precision loss isn't an issue.")]
        public static void SetFloat(this ModDataDictionary data, string key, float value, float @default = 0, float? min = null, float? max = null)
        {
            if (value < min)
                value = min.Value;
            if (value > max)
                value = max.Value;

            if (value == @default)
                data.Remove(key);
            else
                data[key] = value.ToString(CultureInfo.InvariantCulture);
        }

        /****
        ** Int
        ****/
        /// <summary>Read an integer value from the mod data if it exists and is valid, else get the default value.</summary>
        /// <param name="data">The mod data dictionary.</param>
        /// <param name="key">The data key within the <paramref name="data"/>.</param>
        /// <param name="default">The default value if the field is missing or invalid.</param>
        /// <param name="min">The minimum value to consider valid, or <c>null</c> to allow any value.</param>
        public static int GetInt(this ModDataDictionary data, string key, int @default = 0, int? min = null)
        {
            return data.TryGetValue(key, out string raw) && int.TryParse(raw, out int value) && value >= min
                ? value
                : @default;
        }

        /// <summary>Write an integer value into the mod data, or remove it if it matches the <paramref name="default"/>.</summary>
        /// <param name="data">The mod data dictionary.</param>
        /// <param name="key">The data key within the <paramref name="data"/>.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="default">The default value if the field is missing or invalid. If the value matches the default, it won't be written to the data to avoid unneeded serialization and network sync.</param>
        /// <param name="min">The minimum value to consider valid, or <c>null</c> to allow any value.</param>
        public static void SetInt(this ModDataDictionary data, string key, int value, int @default = 0, int? min = null)
        {
            if (value == @default || value <= min)
                data.Remove(key);
            else
                data[key] = value.ToString(CultureInfo.InvariantCulture);
        }
        
        /****
        ** Custom
        ****/
        /// <summary>Read a value from the mod data with custom parsing if it exists and can be parsed, else get the default value.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="data">The mod data dictionary.</param>
        /// <param name="key">The data key within the <paramref name="data"/>.</param>
        /// <param name="parse">Parse the raw value.</param>
        /// <param name="default">The default value if the field is missing or invalid.</param>
        /// <param name="suppressError">Whether to return the default value if <paramref name="parse"/> throws an exception; else rethrow it.</param>
        public static T GetCustom<T>(this ModDataDictionary data, string key, Func<string, T> parse, T @default = default, bool suppressError = true)
        {
            if (!data.TryGetValue(key, out string raw))
                return @default;

            try
            {
                return parse(raw);
            }
            catch when (suppressError)
            {
                return @default;
            }
        }

        /// <summary>Write a value into the mod data with custom serialization, or remove it if it serializes to null or an empty string.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="data">The mod data dictionary.</param>
        /// <param name="key">The field key.</param>
        /// <param name="value">The value to save.</param>
        /// <param name="serialize">Serialize the value to its string representation.</param>
        public static void SetCustom<T>(this ModDataDictionary data, string key, T value, Func<T, string> serialize = null)
        {
            string serialized = serialize != null
                ? serialize(value)
                : value?.ToString();

            if (string.IsNullOrWhiteSpace(serialized))
                data.Remove(key);
            else
                data[key] = serialized;
        }
    }
}

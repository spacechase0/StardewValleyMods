using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using StardewValley;

namespace JsonAssets.Framework
{
    /// <summary>Provides extension methods used within Json Assets.</summary>
    internal static class InternalExtensions
    {
        /// <summary>Recursively call methods marked <see cref="OnDeserializedAttribute"/> on the data model.</summary>
        /// <param name="parent">The root object to scan.</param>
        /// <remarks>This is used to allow data model normalization for instances which are constructed manually instead of being deserialized.</remarks>
        public static void InvokeOnDeserialized(this object parent)
        {
            static void RecursivelyNormalizeImpl(object parent, HashSet<object> visited)
            {
                // can't normalize null parent
                if (parent == null || !visited.Add(parent))
                    return;

                // get recurseable type
                Type type = parent.GetType();
                if (type.IsValueType || type.FullName == null || (!type.FullName.StartsWith("JsonAssets.") && !type.FullName.StartsWith("System.Collections.")))
                    return;

                // call OnDeserialized if present
                MethodInfo onDeserialized = type
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .FirstOrDefault(p => !p.IsSpecialName && p.GetCustomAttribute(typeof(OnDeserializedAttribute)) != null);
                if (onDeserialized != null)
                {
                    StreamingContext context = new(StreamingContextStates.Other);
                    onDeserialized.Invoke(parent, new object[] { context });
                }

                // recurse into properties
                if (parent is IEnumerable enumerable)
                {
                    foreach (object value in enumerable)
                        RecursivelyNormalizeImpl(value, visited);
                }

                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (property.CanRead && !property.GetIndexParameters().Any())
                        RecursivelyNormalizeImpl(property.GetValue(parent), visited);
                }
            }

            RecursivelyNormalizeImpl(parent, new());
        }

        /// <summary>Remove null values from the list.</summary>
        /// <typeparam name="T">The list value type.</typeparam>
        /// <param name="values">The list to filter.</param>
        /// <remarks>This is mainly useful for normalizing models deserialized from JSON, since a double-comma will add a null value.</remarks>
        public static void FilterNulls<T>(this IList<T> values)
            where T : class
        {
            if (values is List<T> list)
                list.RemoveAll(p => p == null);
            else
            {
                for (int i = values.Count - 1; i >= 0; i--)
                {
                    if (values[i] == null)
                        values.RemoveAt(i);
                }
            }
        }

        /// <summary>Get the translated name for the item, or the default name if not translated.</summary>
        /// <param name="item">The item instance.</param>
        public static string LocalizedName(this ITranslatableItem item)
        {
            return InternalExtensions.GetLocalized(item.Name, item.NameLocalization);
        }

        /// <summary>Get the translated description for the item, or the default description if not translated.</summary>
        /// <param name="item">The item instance.</param>
        public static string LocalizedDescription(this ITranslatableItem item)
        {
            return InternalExtensions.GetLocalized(item.Description, item.DescriptionLocalization) ?? string.Empty;
        }

        /// <summary>Get the translated text, or the default description if not translated.</summary>
        /// <param name="defaultText">The default text if no translation matches.</param>
        /// <param name="translations">The translations by language code.</param>
        private static string GetLocalized(string defaultText, IDictionary<string, string> translations)
        {
            var locale = LocalizedContentManager.CurrentLanguageCode;

            return translations.TryGetValue(locale.ToString(), out string translated) || translations.TryGetValue("default", out translated)
                ? translated
                : defaultText;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

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
    }
}

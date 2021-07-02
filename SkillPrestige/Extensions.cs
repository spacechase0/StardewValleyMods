using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using SkillPrestige.Logging;
using System.Runtime.CompilerServices;

namespace SkillPrestige
{
    /// <summary>
    /// Extension methods created and used for SkillPrestige.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// gets the field from an object through reflection, even if it is a private field.
        /// </summary>
        /// <typeparam name="T">The type that contains the parameter member</typeparam>
        /// <param name="instance">The instance of the type you wish to get the field from.</param>
        /// <param name="fieldName">The name of the field you wish to retreive.</param>
        /// <returns>Returns null if no field member found.</returns>
        public static object GetInstanceField<T>(this T instance, string fieldName)
        {
            //Logger.LogVerbose($"Obtaining instance field {fieldName} on object of type {instance.GetType().FullName}");
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var memberInfo = instance.GetType().GetField(fieldName, bindingAttributes);
            return memberInfo?.GetValue(instance);
        }

        /// <summary>
        /// sets the field from an object through reflection, even if it is a private field.
        /// </summary>
        /// <typeparam name="T">The type that contains the parameter member</typeparam>
        /// <typeparam name="TMember">The type of the parameter member</typeparam>
        /// <param name="instance">The instance of the type you wish to set the field value of.</param>
        /// <param name="fieldName">>The name of the field you wish to set.</param>
        /// <param name="value">The value you wish to set the field to.</param>
        // ReSharper disable once UnusedMember.Global
        public static void SetInstanceField<T, TMember>(this T instance, string fieldName, TMember value)
        {
            //Logger.LogVerbose($"Obtaining instance field {fieldName} on object of type {instance.GetType().FullName}");
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var memberInfo = instance.GetType().GetField(fieldName, bindingAttributes);
            memberInfo?.SetValue(instance, value);
        }

        /// <summary>
        /// Invokes and returns the value of a static function, even if it is a private member.
        /// </summary>
        /// <typeparam name="TReturn">The type of the returned function.</typeparam>
        /// <param name="type">The type that contains the static function.</param>
        /// <param name="functionName">The name of the static function.</param>
        /// <param name="arguments">The arguments passed to the static function.</param>
        /// <returns></returns>
        public static TReturn InvokeStaticFunction<TReturn>(this Type type, string functionName, params object[] arguments)
        {
            try
            {
                const BindingFlags bindingAttributes = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                var method = type.GetMethod(functionName, bindingAttributes);
                if (method == null) return default(TReturn);
                var result = method.Invoke(null, arguments);
                return !(result is TReturn) ? default(TReturn) : (TReturn)result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return default(TReturn);
            }
        }

        /// <summary>
        /// sets the field of a base class of an object through reflection, even if it is a private field.
        /// </summary>
        /// <typeparam name="T">The type that directly inherits from the type contains the parameter member</typeparam>
        /// <typeparam name="TMember">The type of the parameter member</typeparam>
        /// <param name="instance">The instance of the type you wish to set the field value of.</param>
        /// <param name="fieldName">>The name of the field you wish to set.</param>
        /// <param name="value">The value you wish to set the field to.</param>
        public static void SetInstanceFieldOfBase<T, TMember>(this T instance, string fieldName, TMember value)
        {
            //Logger.LogVerbose($"Obtaining instance field {fieldName} on object of type {instance.GetType().FullName}");
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var memberInfo = instance.GetType().BaseType?.GetField(fieldName, bindingAttributes);
            memberInfo?.SetValue(instance, value);
        }

        public static IEnumerable<Assembly> GetNonSystemAssemblies(this AppDomain appDomain)
        {
            return appDomain.GetAssemblies().Where(x => !x.FullName.StartsWithOneOf("mscorlib", "System", "Microsoft", "Windows", "Newtonsoft"));
        }


        /// <summary>
        /// Gets types from an assembly as long as those types can safely be retrieved.
        /// </summary>
        /// <param name="assembly">Assembly you wish to obtain types from.</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypesSafely(this Assembly assembly)
        {
            try
            {
                Logger.LogVerbose($"Attempting to obtain types of assembly {assembly.FullName} safely...");
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                Logger.LogInformation($"Failed to load a type from assembly {assembly.FullName}. details: {Environment.NewLine} {exception}");
                return exception.Types.Where(x => x != null);
            }
        }


        /// <summary>
        /// Replaces any method with another method, given that the signatures match.
        /// </summary>
        /// <param name="typeToReplace"></param>
        /// <param name="methodNameToReplace"></param>
        /// <param name="replacementType"></param>
        /// <param name="replacementMethodName"></param>
        public static void ReplaceMethod(this Type typeToReplace, string methodNameToReplace, Type replacementType, string replacementMethodName)
        {
            var methodToReplace = typeToReplace.GetMethod(methodNameToReplace, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var methodToInject = replacementType.GetMethod(replacementMethodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            RuntimeHelpers.PrepareMethod(methodToReplace.MethodHandle);
            RuntimeHelpers.PrepareMethod(methodToInject.MethodHandle);

            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    var injectedMethodPointer = (int*) methodToInject.MethodHandle.Value.ToPointer() + 2;
                    var targetMethodPointer = (int*) methodToReplace.MethodHandle.Value.ToPointer() + 2;
                    *targetMethodPointer = *injectedMethodPointer;
                }
                else
                {

                    var injectedMethodPointer = (long*) methodToInject.MethodHandle.Value.ToPointer() + 1;
                    var targetMethodPointer = (long*) methodToReplace.MethodHandle.Value.ToPointer() + 1;
                    *targetMethodPointer = *injectedMethodPointer;
                }
            }
        }

        /// <summary>
        /// Word-wraps text for XNA sprite fonts
        /// </summary>
        /// <param name="text">the text that will be word-wrapped</param>
        /// <param name="font">the XNA Sprite Font to measure the string with.</param>
        /// <param name="maxLineWidth">The maximum line width in pixels before a word is wrapped.</param>
        /// <returns></returns>
        public static string WrapText(this string text, SpriteFont font, float maxLineWidth)
        {
            var words = text.Split(' ');
            var stringBuilder = new StringBuilder();
            var lineWidth = 0f;
            var spaceWidth = font.MeasureString(" ").X;

            foreach (var word in words)
            {
                var size = font.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    stringBuilder.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    if (size.X > maxLineWidth)
                    {
                        if (stringBuilder.ToString() == string.Empty)
                        {
                            stringBuilder.Append((word.Insert(word.Length / 2, " ") + " ").WrapText(font, maxLineWidth));
                        }
                        else
                        {
                            stringBuilder.Append("\n" + (word.Insert(word.Length / 2, " ") + " ").WrapText(font, maxLineWidth));
                        }
                    }
                    else
                    {
                        stringBuilder.Append("\n" + word + " ");
                        lineWidth = size.X + spaceWidth;
                    }
                }
            }
            return stringBuilder.ToString();
        }

        public static bool IsOneOf<T>(this T item, params T[] itemsToCheck)
        {
            return itemsToCheck.Contains(item);
        }

        public static bool IsOneOf<T>(this T item, IEnumerable<T> itemsToCheck)
        {
            return itemsToCheck.Contains(item);
        }

        /// <summary>
        /// Checks to see if the text starts with any one of the passed strings.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="stringsToCheck"></param>
        /// <returns></returns>
        // ReSharper disable once MemberCanBePrivate.Global - I want this to be available.
        public static bool StartsWithOneOf(this string text, params string[] stringsToCheck)
        {
            return stringsToCheck.Any(text.StartsWith);
        }

        /// <summary>
        /// Clamps a value within a given range.
        /// </summary>
        /// <typeparam name="T">Type of item to compare. Must be IComparable.</typeparam>
        /// <param name="value">Value to clamp.</param>
        /// <param name="minimum">Minimum allowable value.</param>
        /// <param name="maximum">Maximum allowable value.</param>
        /// <returns>The value given as long as it is in the given range, otherwise it gives the closest available value within the range (inclusive).</returns>
        public static T Clamp<T>(this T value, T minimum, T maximum) where T : IComparable<T>
        {
            if(minimum.CompareTo(maximum) > 0) throw new ArgumentException("Minimum cannot exceed maximum.");
            return value.CompareTo(minimum) < 0 ? minimum : value.CompareTo(maximum) > 0 ? maximum : value;
        }

        /// <summary>
        /// Detects if a value is in a given set.
        /// </summary>
        /// <typeparam name="T">The type of item to check.</typeparam>
        /// <param name="value">the value to look for in the given array.</param>
        /// <param name="items">The parameter array of values to compare to the given value.</param>
        /// <returns>True if the value given is in the array of comparison items.</returns>
        public static bool In<T>(this T value, params T[] items)
        {
            return items.Contains(value);
        }

        /// <summary>
        ///  Detects if a value is in a given set.
        /// </summary>
        /// <typeparam name="T">The type of item to check.</typeparam>
        /// <param name="value">the value to look for in the given enumeration.</param>
        /// <param name="items">The enumeration of values to compare to the given value.</param>
        /// <returns>True if the value given is in the enumeration of comparison items.</returns>
        public static bool In<T>(this T value, IEnumerable<T> items)
        {
            return items.Contains(value);
        }

        /// <summary>
        /// Returns Math.Floor as an integer.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Floor(this decimal value)
        {
            return Convert.ToInt32(Math.Floor(value));
        }

        /// <summary>
        /// Returns Math.Floor as an integer.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Floor(this float value)
        {
            return Convert.ToInt32(Math.Floor(value));
        }

        /// <summary>
        /// Returns Math.Floor as an integer.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Floor(this double value)
        {
            return Convert.ToInt32(Math.Floor(value));
        }

        /// <summary>
        /// Returns Math.Ceiling as an integer.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Ceiling(this decimal value)
        {
            return Convert.ToInt32(Math.Ceiling(value));
        }

        /// <summary>
        /// Returns Math.Ceiling as an integer.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Ceiling(this float value)
        {
            return Convert.ToInt32(Math.Ceiling(value));
        }

        /// <summary>
        /// Returns Math.Ceiling as an integer.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Ceiling(this double value)
        {
            return Convert.ToInt32(Math.Ceiling(value));
        }

        /// <summary>
        /// Removes all non-alphanumeric characters.
        /// </summary>
        /// <param name="value">The string to manipulate.</param>
        /// <returns>The value string given, sans any non-alphanumeric characters.</returns>
        public static string RemoveNonAlphanumerics(this string value)
        {
            foreach (var character in value)
            {
                if (!char.IsLetterOrDigit(character))
                    value = value.Replace(character.ToString(), "");
            }
            return value;
        }
    }
}

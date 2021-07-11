namespace SpaceShared
{
    /// <summary>Provides common extensions for general mod logic.</summary>
    internal static class CommonExtensions
    {
        /*********
        ** Public methods
        *********/
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
        public static T GetOrDefault<T>(this T[] array, int index)
        {
            return array.TryGetIndex(index, out T value)
                ? value
                : default;
        }
    }
}

using System;
using System.Collections.Generic;

namespace CustomCritters.Framework
{
    internal static class Extensions
    {
        private static readonly Random Rand = new();
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Extensions.Rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}

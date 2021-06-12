using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceCore.Framework.Utilities
{
    internal class IWeighted
    {
        public int Weight { get; set; }

        public static IWeighted Choose(List<IWeighted> choices, int totalWeight)
        {
            int num = new Random().Next(totalWeight);

            for (int i = 0; i < choices.Count; ++i)
            {
                IWeighted curr = choices[i];
                if (num <= curr.Weight)
                    return curr;
                else
                    num -= curr.Weight;
            }

            return choices.Last();
        }

        public static T Choose<T>(List<T> choices, int totalWeight)
        where T : IWeighted
        {
            int num = new Random().Next(totalWeight);

            for (int i = 0; i < choices.Count; ++i)
            {
                T curr = choices[i];
                if (num <= curr.Weight)
                    return curr;
                else
                    num -= curr.Weight;
            }

            return choices.Last();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceShared
{
    public class Weighted<T>// : ICloneable where T : ICloneable
    {
        public double Weight { get; set; }
        public T Value { get; set; }

        public Weighted()
        : this(1.0, default)
        {
        }

        public Weighted(double weight, T value)
        {
            this.Weight = weight;
            this.Value = value;
        }

        /*
        public object Clone()
        {
            return new Weighted<T>(this.Weight, (T)this.Value?.Clone());
        }
        */
    }

    public static class WeightedExtensions
    {
        public static T Choose<T>(this Weighted<T>[] choices, Random r = null)// where T : ICloneable
        {
            if (choices.Length == 0)
                return default;
            if (choices.Length == 1)
                return choices[0].Value;

            if (r == null)
                r = new Random();

            double sum = choices.Sum(choice => choice.Weight);

            double chosenWeight = r.NextDouble() * sum;
            foreach (var choice in choices)
            {
                if (chosenWeight < choice.Weight) // might need change to <=
                    return choice.Value;
                chosenWeight -= choice.Weight;
            }

            throw new Exception("This should never happen");
        }

        public static T Choose<T>(this List<Weighted<T>> choices, Random r = null)// where T : ICloneable
        {
            return choices.ToArray().Choose(r);
        }
    }
}

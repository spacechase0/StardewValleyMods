using System;

namespace DynamicGameAssets
{
    public class Weighted<T> : ICloneable where T : ICloneable
    {
        public double Weight { get; set; }
        public T Value { get; set; }

        public Weighted(double weight, T value)
        {
            this.Weight = weight;
            this.Value = value;
        }

        public object Clone()
        {
            return new Weighted<T>(this.Weight, (T)this.Value?.Clone());
        }
    }
}

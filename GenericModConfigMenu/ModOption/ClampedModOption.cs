using System;
using SpaceShared;
using StardewModdingAPI;

namespace GenericModConfigMenu.ModOption
{
    internal class ClampedModOption<T> : SimpleModOption<T>
    {
        public T Minimum { get; set; }
        public T Maximum { get; set; }
        public T Interval { get; set; }

        public override T Value
        {
            get { return base.Value; }
            set { base.Value = Util.Adjust(Util.Clamp(this.Minimum, value, this.Maximum), this.Interval); }
        }

        public ClampedModOption(string name, string desc, Type type, Func<T> theGetter, Action<T> theSetter, T theMin, T theMax, T interval, string id, IManifest mod)
            : base(name, desc, type, theGetter, theSetter, id, mod)
        {
            this.Minimum = theMin;
            this.Maximum = theMax;
            this.Interval = interval;
        }
    }
}

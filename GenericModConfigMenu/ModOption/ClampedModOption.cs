using SpaceShared;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            set { base.Value = Util.Adjust(Util.Clamp(Minimum, value, Maximum), Interval); }
        }

        public ClampedModOption( string name, string desc, Type type, Func<T> theGetter, Action<T> theSetter, T theMin, T theMax, T interval, string id, IManifest mod )
        :   base( name, desc, type, theGetter, theSetter, id, mod )
        {
            Minimum = theMin;
            Maximum = theMax;
            Interval = interval;
        }
    }
}

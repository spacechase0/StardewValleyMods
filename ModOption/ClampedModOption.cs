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

        public override T Value
        {
            get { return base.Value; }
            set { base.Value = Util.Clamp< T >( Minimum, value, Maximum ); }
        }

        public ClampedModOption( string name, string desc, Type type, Func<T> theGetter, Action<T> theSetter, T theMin, T theMax )
        :   base( name, desc, type, theGetter, theSetter )
        {
            Minimum = theMin;
            Maximum = theMax;
        }
    }
}

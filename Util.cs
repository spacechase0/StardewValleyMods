using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu
{
    public struct Range<T>
    {
        public T Minimum;
        public T Maximum;
    }

    public class Util
    {
        public static T Clamp< T >(T min, T t, T max)
        {
            if (Comparer<T>.Default.Compare(min, t) > 0)
                return min;
            if (Comparer<T>.Default.Compare(max, t) < 0)
                return max;
            return t;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceShared
{
    public class Holder<T>
    {
        public T Value;

        public Holder() { }
        public Holder( T value )
        {
            this.Value = value;
        }
    }
}

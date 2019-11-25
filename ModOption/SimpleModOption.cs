using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu.ModOption
{
    internal class SimpleModOption<T> : BaseModOption
    {
        public Type Type { get; }
        protected Func<T> getter;
        protected Action<T> setter;

        private T state;
        public virtual T Value
        {
            get { return state; }
            set { state = value; }
        }

        public override void SyncToMod()
        {
            state = getter.Invoke();
        }

        public override void Save()
        {
            setter.Invoke(state);
        }

        public SimpleModOption( string name, string desc, Type type, Func<T> theGetter, Action<T> theSetter )
        :   base( name, desc )
        {
            Type = type;
            getter = theGetter;
            setter = theSetter;

            state = getter.Invoke();
        }
    }
}

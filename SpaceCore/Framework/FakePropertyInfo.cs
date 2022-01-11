using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Framework
{
    internal class FakePropertyInfo : PropertyInfo
    {
        private CustomPropertyInfo parent;

        public FakePropertyInfo( CustomPropertyInfo parent )
        {
            this.parent = parent;
        }

        public override Type PropertyType => parent.PropertyType;
        public override PropertyAttributes Attributes => PropertyAttributes.None;
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override string Name => parent.Name;
        public override Type DeclaringType => parent.DeclaringType;
        public override Type ReflectedType => parent.DeclaringType; // TODO: Will this work for subclasses? Like, GameLocation -> BuildableGameLocation -> Farm ?
        public override MethodInfo[] GetAccessors( bool nonPublic )
        {
            return new MethodInfo[]
            {
                parent.Getter,
                parent.Setter,
                parent.Getter,
                parent.Setter,
            };
        }
        public override object[] GetCustomAttributes( bool inherit )
        {
            return new object[ 0 ];
        }
        public override object[] GetCustomAttributes( Type attributeType, bool inherit )
        {
            return new object[ 0 ];
        }

        public override MethodInfo GetGetMethod( bool nonPublic )
        {
            return GetAccessors( nonPublic )[ 0 ];
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return new ParameterInfo[ 0 ];
        }

        public override MethodInfo GetSetMethod( bool nonPublic )
        {
            return GetAccessors( nonPublic )[ 1 ];
        }

        public override object GetValue( object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture )
        {
            return parent.Getter.Invoke( null, new object[] { obj } );
        }

        public override bool IsDefined( Type attributeType, bool inherit )
        {
            return false;
        }

        public override void SetValue( object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture )
        {
            parent.Setter.Invoke( null, new object[] { obj, value } );
        }
    }
}

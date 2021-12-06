using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Framework
{
    internal class CustomPropertyInfo
    {
        private PropertyInfo propInfo;

        public Type DeclaringType { get; set; }
        public string Name { get; set; }
        public Type PropertyType { get; set; }
        public MethodInfo Getter { get; set; }
        public MethodInfo Setter { get; set; }

        public PropertyInfo GetFakePropertyInfo()
        {
            if ( propInfo == null )
                propInfo = new FakePropertyInfo( this );
            return propInfo;
        }
    }
}

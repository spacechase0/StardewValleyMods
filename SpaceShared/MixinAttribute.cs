using System;

namespace SpaceShared
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = false )]
    public class MixinAttribute : Attribute
    {
        public Type Type { get; }

        public MixinAttribute( Type type )
        {
            this.Type = type;
        }
    }
}

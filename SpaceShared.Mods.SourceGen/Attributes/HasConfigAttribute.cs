using System;

namespace SpaceShared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class HasConfigAttribute<ConfigurationType> : Attribute
{
}

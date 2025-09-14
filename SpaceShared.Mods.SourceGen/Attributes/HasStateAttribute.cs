using System;

namespace SpaceShared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class HasStateAttribute<StateType> : Attribute
{
}

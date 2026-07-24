using System;

namespace SpaceShared.Attributes;

// TODO: This could probably be merged with "HasState" into a more general "HasPerScreen"
[AttributeUsage(AttributeTargets.Class)]
internal sealed class HasAssetsAttribute<TAssetInstancesType> : Attribute
{
}

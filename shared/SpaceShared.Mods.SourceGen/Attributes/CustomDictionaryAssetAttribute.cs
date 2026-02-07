using System;

namespace SpaceShared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class CustomDictionaryAssetAttribute : Attribute
{
    public string AssetName { get; }

    public CustomDictionaryAssetAttribute(string assetName)
    {
        this.AssetName = assetName;
    }
}

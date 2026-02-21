using System;

namespace SpaceShared.Attributes;

internal abstract class DictionaryAssetDataAttributeBase : Attribute
{
    public string AssetName { get; }
    public string KeyPattern { get; }
    public bool OwnedAsset { get; init; } = false;

    public DictionaryAssetDataAttributeBase(string assetName, string keyPattern)
    {
        this.AssetName = assetName;
        this.KeyPattern = keyPattern;
    }
}

[AttributeUsage(AttributeTargets.Class)]
internal sealed class DictionaryAssetDataAttribute<ValueType> : DictionaryAssetDataAttributeBase
{
    public DictionaryAssetDataAttribute(string assetName, string keyPattern = "&")
    : base(assetName, keyPattern)
    {
    }
}

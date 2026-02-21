using System;

namespace SpaceShared.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal sealed class DictionaryAssetDataKeyAttribute : Attribute
{
    public string Key { get; }
    public bool IgnoreSubstitutions { get; init; } = false;

    public DictionaryAssetDataKeyAttribute(string Key)
    {
        this.Key = Key;
    }
}

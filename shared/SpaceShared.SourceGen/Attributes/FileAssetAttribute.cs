using System;

namespace SpaceShared.Attributes;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class FileAssetAttribute : Attribute
{
    public string AssetName { get; }
    public string? ContentPipelineName { get; }

    public FileAssetAttribute(string AssetName, string? ContentPipelineName = null)
    {
        this.AssetName = AssetName;
        this.ContentPipelineName = ContentPipelineName;
    }
}

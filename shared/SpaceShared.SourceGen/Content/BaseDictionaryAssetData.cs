using System;
using System.Collections.Generic;

namespace SpaceShared.Content;

internal abstract class BaseDictionaryAssetData
{
    public string ModId { get; internal set; } = null!;

    public delegate string ProvideSubsitutionDelegate(string assetName, string fieldName);
    public virtual Dictionary<string, ProvideSubsitutionDelegate> KeySubstitutions => new()
    {
        ["$"] = (_, _) => ModId,
        ["&"] = (_, f) => f,
    };
}

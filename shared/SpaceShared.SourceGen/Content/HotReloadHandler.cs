using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using SpaceShared.Attributes;

#if DEBUG

[assembly: MetadataUpdateHandler(typeof(SpaceShared.Content.HotReloadHandler))]

namespace SpaceShared.Content;

internal static class HotReloadHandler
{
    public static event Action<Type> PendingHotReloads;

    public static void ClearCache(Type[] updated)
    {
        foreach (var type in updated)
        {
            var customAttr = type.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.Name.StartsWith($"{nameof(DictionaryAssetDataAttribute<>)}`"));
            if (customAttr == null)
                continue;

            // Calling ContentRegistry.Reload directly acts really weird
            // Static state missing, `Type` instances not having correct equality, etc.
            // I'm guessing that calling it directly makes it use the temporary assembly for compilation/reload,
            // and not the actually useful one.
            PendingHotReloads?.Invoke(type);
        }
    }
}

#endif

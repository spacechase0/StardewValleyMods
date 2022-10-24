using System;
using System.Collections.Generic;

using SpaceShared;

using StardewValley;

using StardewModdingAPI;

using SObject = StardewValley.Object;
using SpaceCore.Framework.Extensions;

namespace JsonAssets.Utilities;
internal static class ItemResolver
{
    private static IAssetName Game1ObjectInfo;
    private static IAssetName ClothingInfo;

    private static Lazy<Dictionary<string, int>> SObjectMap = new(GenerateObjectMap);
    private static Lazy<Dictionary<string, int>> ClothingMap = new(GenerateClothingMap);

    internal static void Initialize(IGameContentHelper parser)
    {
        Game1ObjectInfo = parser.ParseAssetName(@"Data\ObjectInformation");
        ClothingInfo = parser.ParseAssetName(@"Data\ClothingInformation");
    }

    internal static void Reset(IReadOnlySet<IAssetName> assets)
    {
        if (assets.Contains(Game1ObjectInfo) && SObjectMap.IsValueCreated)
            SObjectMap = new(GenerateObjectMap);
        if (assets.Contains(ClothingInfo) && ClothingMap.IsValueCreated)
            ClothingMap = new(GenerateClothingMap);
    }

    internal static int GetObjectID(object data)
    {
        if (data is long inputId)
            return (int)inputId;

        if (Mod.instance.ObjectIds.TryGetValue((string)data, out int jaId))
            return jaId;

        if (SObjectMap.Value.TryGetValue((string)data, out int id))
            return id;

        if (int.TryParse((string)data, out int val))
            return val;

        Log.Warn($"No idea what '{data}' is!");
        return 0;
    }

    internal static int GetClothingID(object data)
    {
        if (data is long inputId)
            return (int)inputId;

        if (Mod.instance.ClothingIds.TryGetValue((string)data, out int jaId))
            return jaId;

        if (ClothingMap.Value.TryGetValue((string)data, out int id))
            return id;

        if (int.TryParse((string)data, out int val))
            return val;

        Log.Warn($"No idea what '{data}' is!");
        return 0;
    }

    private static Dictionary<string, int> GenerateObjectMap()
    {
        Log.Info("Building map to resolve normal objects.");

        var objectinfo = Game1.objectInformation ?? Game1.content.Load<Dictionary<int, string>>(Game1ObjectInfo.BaseName);

        Dictionary<string, int> mapping = new(objectinfo.Count)
        {
            // Special cases
            ["Egg"] = 176,
            ["Brown Egg"] = 180,
            ["Large Egg"] = 174,
            ["Large Brown Egg"] = 182,
            ["Strange Doll"] = 126,
            ["Strange Doll 2"] = 127,
        };

        // Processing from the data.
        foreach ((int id, string data) in objectinfo)
        {
            // category asdf should never end up in the player inventory.
            var cat = data.GetNthChunk('/', SObject.objectInfoTypeIndex);
            if (cat.Equals("asdf", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var name = data.GetNthChunk('/', SObject.objectInfoNameIndex);
            if (name.Equals("Stone", StringComparison.OrdinalIgnoreCase) && id != 390)
            {
                continue;
            }
            if (name.Equals("Weeds", StringComparison.OrdinalIgnoreCase)
                || name.Equals("SupplyCrate", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Twig", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Rotten Plant", StringComparison.OrdinalIgnoreCase)
                || name.Equals("???", StringComparison.OrdinalIgnoreCase)
                || name.Equals("DGA Dummy Object", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Egg", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Large Egg", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Strange Doll", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            if (!mapping.TryAdd(name.ToString(), id))
            {
                Log.Warn($"{name.ToString()} with {id} seems to be a duplicate SObject and may not be resolved correctly.");
            }
        }
        return mapping;
    }

    private static Dictionary<string, int> GenerateClothingMap()
    {
        Log.Info("Building map to resolve Clothing");

        Dictionary<string, int> mapping = new(Game1.clothingInformation.Count)
        {
            ["Prismatic Shirt"] = 1999,
            ["Dark Prismatic Shirt"] = 1998,
        };

        foreach ((int id, string data) in Game1.clothingInformation)
        {
            var nameSpan = data.GetNthChunk('/', SObject.objectInfoNameIndex);
            if (nameSpan.Equals("Prismatic Shirt", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string name = nameSpan.ToString();
            if (!mapping.TryAdd(name, id))
            {
                Log.Warn($"{name} with {id} seems to be a duplicate ClothingItem and may not be resolved correctly.");
            }
        }
        return mapping;
    }
}

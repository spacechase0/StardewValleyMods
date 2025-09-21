#nullable enable

using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace GenericModConfigMenu.Framework;
internal static class AssetManager
{
    private const string BasePath = "Mods/GenericModConfigMenu";
    internal static string ConfigButton { get; } = PathUtilities.NormalizeAssetName(BasePath + "/ConfigButton");
    internal static string KeyboardButton { get; } = PathUtilities.NormalizeAssetName(BasePath + "/KeyboardButton");
    internal static string SortNameButton { get; } = PathUtilities.NormalizeAssetName(BasePath + "/SortNameButton");
    internal static string SortCreatedButton { get; } = PathUtilities.NormalizeAssetName(BasePath + "/SortCreatedButton");
    internal static string SortModifiedButton { get; } = PathUtilities.NormalizeAssetName(BasePath + "/SortModifiedButton");

    internal static void Apply(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(ConfigButton))
            e.LoadFromModFile<Texture2D>("assets/config-button.png", AssetLoadPriority.Exclusive);
        else if (e.Name.IsEquivalentTo(KeyboardButton))
            e.LoadFromModFile<Texture2D>("assets/keybinds-button.png", AssetLoadPriority.Exclusive);
        else if (e.Name.IsEquivalentTo(SortNameButton))
            e.LoadFromModFile<Texture2D>("assets/sort_name-button.png", AssetLoadPriority.Exclusive);
        else if (e.Name.IsEquivalentTo(SortCreatedButton))
            e.LoadFromModFile<Texture2D>("assets/sort_created-button.png", AssetLoadPriority.Exclusive);
        else if (e.Name.IsEquivalentTo(SortModifiedButton))
            e.LoadFromModFile<Texture2D>("assets/sort_modified-button.png", AssetLoadPriority.Exclusive);
    }
}

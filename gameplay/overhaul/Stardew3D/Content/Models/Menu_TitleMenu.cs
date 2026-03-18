using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Models;

[DictionaryAssetData<ModelData>("Models", "($/Menu)StardewValley.Menus.TitleMenu/Clickables/&", OwnedAsset = true)]
internal partial class Menu_TitleMenu : SpaceShared.Content.BaseDictionaryAssetData
{
    [DictionaryAssetDataKey("($/Menu)StardewValley.Menus.TitleMenu")]
    public MenuModelData Menu => new()
    {
        ModelFilePath = $"{ModId}:assets/menus/Title.gltf",
        SubModelPath = "/title",

        UseExistingTransformHierarchy = -1,

        Clickables = new()
        {
            { "New", new()
            {
                ModelId = $"({ModId}/Menu)StardewValley.Menus.TitleMenu/Clickables/New",
                HoverAnimation = "hover",
            } },
            { "Load", new()
            {
                ModelId = $"({ModId}/Menu)StardewValley.Menus.TitleMenu/Clickables/Load",
                HoverAnimation = "hover",
            } },
            { "Co-op", new()
            {
                ModelId = $"({ModId}/Menu)StardewValley.Menus.TitleMenu/Clickables/Coop",
                HoverAnimation = "hover",
            } },
            { "Exit", new()
            {
                ModelId = $"({ModId}/Menu)StardewValley.Menus.TitleMenu/Clickables/Exit",
                HoverAnimation = "hover",
            } },
        },
    };

    public KeyValuePair<string, MenuModelData>[] Clickables =>
        new string[] { "New", "Load", "Coop", "Exit" }
        .Select(k => new KeyValuePair<string, MenuModelData>(k, new()
        {
            ModelFilePath = $"{ModId}:assets/menus/Title.gltf",
            SubModelPath = $"/buttons/{k.ToLower()}",

            AdditionalAnimationData = new()
            {
                { "hover", new()
                {
                    Loop = ModelData.AnimationMetadata.LoopFinishMode.Hold,
                    Actions =
                    [
                        new() { Time = 0.01f, ForReverse = false, Actions = [ "SwapTexture titleButtons_idle.png titleButtons_hover.png" ] },
                        new() { Time = 0.09f, ForReverse = true, Actions = [ "SwapTexture titleButtons_hover.png, titleButtons_idle.png" ] },
                    ],
                } }
            }
        }))
        .ToArray();
}

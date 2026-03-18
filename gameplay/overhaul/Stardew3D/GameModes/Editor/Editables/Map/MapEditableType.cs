using StardewModdingAPI.Utilities;
using StardewValley;

namespace Stardew3D.GameModes.Editor.Editables.Map;
internal class MapEditableType : IEditableType
{
    internal static LocalizedContentManager vanillaOnlyContent;

    public string Id => $"{Mod.Instance.ModManifest.UniqueID}/Map";
    public string TypeName => "Map";
    public string TypeListName => "Maps";

    public EditableTree GetListing()
    {
        if (vanillaOnlyContent == null)
            vanillaOnlyContent = new(GameRunner.instance.Services, Game1.content.RootDirectory);


        EditableTree vanilla = new();
        foreach (var entry in DataLoader.Locations(vanillaOnlyContent))
        {
            string path = PathUtilities.NormalizeAssetName(entry.Value.CreateOnLoad?.MapPath);
            if (path == null)
                continue;

            IEditable editable = new MapEditable(Game1.game1.xTileContent, entry.Key, entry.Value.CreateOnLoad.MapPath);
            vanilla.Entries.Add(entry.Value.CreateOnLoad.MapPath, editable);
        }

        EditableTree ret = new();
        ret.SubTrees.Add("Stardew Valley (Unmodded)", vanilla);
        // TODO: Populate with modded maps
        return ret;
    }

    public void OnRequestNew(EditorGameMode editor)
    {
        throw new NotImplementedException();
    }
}

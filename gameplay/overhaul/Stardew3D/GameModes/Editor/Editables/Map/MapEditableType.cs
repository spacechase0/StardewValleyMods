using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Stardew3D;
using Stardew3D.GameModes.Editor;
using Stardew3D.GameModes.Editor.Editables;
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
        foreach (var entry in Directory.GetFiles(Path.Combine(Game1.content.RootDirectory, "Maps"), "*.xnb", SearchOption.AllDirectories))
        {
            if (!entry.EndsWith(".xnb"))
                continue;
            string assetName = entry.Replace('\\', '/');
            assetName = assetName.Substring(0, assetName.Length - 4);
            assetName = assetName.Substring(assetName.IndexOf("Maps/"));

            string assetFileName = assetName.Substring(5);

            string[] parts = assetFileName.Split('/');

            IEditable editable = new MapEditable(assetName);

            EditableTree curr = vanilla;
            for (int i = 0; i < parts.Length - 1; ++i)
            {
                if (!curr.SubTrees.TryGetValue(parts[i], out var subtree))
                    curr.SubTrees.Add(parts[i], subtree = new());
                curr = subtree;
            }
            curr.Entries.Add(parts.Last(), editable);
        }

        EditableTree ret = new();
        ret.SubTrees.Add("Stardew Valley", vanilla);
        // TODO: Populate with modded maps
        return ret;
    }

    public void OnRequestNew(EditorGameMode editor)
    {
        throw new NotImplementedException();
    }
}

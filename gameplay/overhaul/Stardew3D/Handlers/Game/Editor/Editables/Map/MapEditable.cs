using System.Collections.Generic;
using MLEM.Ui.Elements;

namespace Stardew3D.Handlers.Game.Editor.Editables.Map;

internal class MapEditable : IEditable
{
    public string Id { get; init; }
    public bool HasUnsavedChanges { get; private set; }

    public MapEditable(string assetName)
    {
        Id = assetName.Substring(assetName.LastIndexOf('/'));
    }

    public void Dispose()
    {
    }

    public Element PopulatePanelContents()
    {
        throw new System.NotImplementedException();
    }

    public Dictionary<string, string> Save()
    {
        throw new System.NotImplementedException();
        HasUnsavedChanges = false;
    }
}

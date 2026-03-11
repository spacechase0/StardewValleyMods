using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MLEM.Ui.Elements;

namespace Stardew3D.GameModes.Editor.Editables;
public interface IEditable : IDisposable
{
    public string Id { get; }

    public Element PopulatePanelContents();
    public void BeforeHidePanelContents() { }

    // TODO: Callback for handling/drawing the non-UI contents

    public bool HasUnsavedChanges { get; }
    public Dictionary<string, string> Save(); // format -> contents, ex. ".tmx" -> "..."
}

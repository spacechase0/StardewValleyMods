using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MLEM.Ui.Elements;
using Stardew3D.Handlers.Render;

namespace Stardew3D.GameModes.Editor.Editables.Map.EditingModes;

public abstract class BaseEditingMode
{
    public abstract string Id { get; }
    public virtual LocationRenderer.ShowMissingType ShowMissingInLocation => LocationRenderer.ShowMissingType.None;

    public MapEditable Editable { get; }

    public BaseEditingMode(MapEditable editable)
    {
        Editable = editable;
    }

    public abstract ICollection<Element> PopulatePanelContents();

    public abstract void Update();
    public abstract void Render();
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stardew3D.GameModes.Editor;

namespace Stardew3D.GameModes.Editor.Editables;

public interface IEditableType
{
    public string Id { get; }
    public string TypeName { get; }
    public string TypeListName { get; }

    public EditableTree GetListing();

    public void OnRequestNew(EditorGameMode editor);
}

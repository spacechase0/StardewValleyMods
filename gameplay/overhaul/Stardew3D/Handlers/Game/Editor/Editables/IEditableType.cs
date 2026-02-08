using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew3D.Handlers.Game.Editor.Editables;

public interface IEditableType
{
    public string Id { get; }
    public string TypeName { get; }
    public string TypeListName { get; }

    public EditableTree GetListing();

    public void OnRequestNew(EditorGameHandler editor);
}

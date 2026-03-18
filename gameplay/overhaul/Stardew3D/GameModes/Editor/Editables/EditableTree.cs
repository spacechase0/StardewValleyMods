namespace Stardew3D.GameModes.Editor.Editables;
public class EditableTree
{
    public Dictionary<string, IEditable> Entries { get; } = new();
    public Dictionary<string, EditableTree> SubTrees { get; } = new();
}

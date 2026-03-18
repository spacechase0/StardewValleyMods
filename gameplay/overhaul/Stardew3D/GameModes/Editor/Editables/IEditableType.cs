namespace Stardew3D.GameModes.Editor.Editables;

public interface IEditableType
{
    public string Id { get; }
    public string TypeName { get; }
    public string TypeListName { get; }

    public EditableTree GetListing();

    public void OnRequestNew(EditorGameMode editor);
}

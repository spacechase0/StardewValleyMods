using Microsoft.Xna.Framework.Graphics;
using MLEM.Ui.Elements;
using Stardew3D.Rendering;

namespace Stardew3D.GameModes.Editor.Editables;
public interface IEditable : IDisposable
{
    public string Id { get; }

    public ICollection<Element> PopulatePanelContents();
    public void BeforeHidePanelContents() { }

    public void Update() { }
    public void RenderMenu(SpriteBatch sb) { }
    public void RenderWorld(RenderBatcher b) { }
    public void AfterRenderWorld() { }

    public bool HasUnsavedChanges { get; }
    public Dictionary<string, string> Save(); // format -> contents, ex. ".tmx" -> "..."
}

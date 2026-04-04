using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MLEM.Ui.Elements;
using Stardew3D.Handlers;

namespace Stardew3D.GameModes.Editor.Editables.Map.EditingModes;
public class WallEditingMode : BaseEditingMode
{
    public override string Id => "Walls";

    public override LocationHandler.ShowMissingType ShowMissingInLocation => LocationHandler.ShowMissingType.Walls;

    public WallEditingMode(MapEditable editable)
        : base(editable)
    {
    }

    public override ICollection<Element> PopulatePanelContents()
    {
        // TODO
        return [];
    }

    public override void Update()
    {
    }

    public override void Render()
    {
    }
}

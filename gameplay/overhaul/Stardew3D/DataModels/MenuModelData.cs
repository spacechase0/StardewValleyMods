using Microsoft.Xna.Framework;

namespace Stardew3D.DataModels;
public class MenuModelData : ModelData
{
    public override string Type => $"{Mod.Instance.ModManifest.UniqueID}/Menu";

    public class ClickableModelData : OtherModelReference
    {
        public string HoverAnimation { get; set; }
        public string ClickAnimation { get; set; }

        public BoundingBox? BoundingBoxOverride { get; set; } = null;
    }
    public Dictionary<string, ClickableModelData> Clickables { get; set; } = new();

    public new static MenuModelData Get(string id)
    {
        return ModelData.Get(id) as MenuModelData;
    }
}

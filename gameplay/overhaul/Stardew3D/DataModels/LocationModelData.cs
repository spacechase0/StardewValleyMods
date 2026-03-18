using Microsoft.Xna.Framework;

namespace Stardew3D.DataModels;
public class LocationModelData : ModelData
{
    public override string Type => $"{Mod.Instance.ModManifest.UniqueID}/Location";

    public class Portal
    {
        public string OtherLocation { get; set; }
        public string MatchingPortal { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Facing { get; set; }
    }
    public Dictionary<string, Portal> Portals { get; set; } = new();

    public new static LocationModelData Get(string id)
    {
        return ModelData.Get(id) as LocationModelData;
    }
}

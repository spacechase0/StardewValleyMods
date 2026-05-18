using SpaceShared.Attributes;

namespace Stardew3D.DataModels;

[CustomDictionaryAsset("FloorWallAssociations")]
public partial class FloorWallAssociationData
{
    static partial void AfterRefreshData()
    {
        Mod.State.ClearHandlerState();
    }

    public string WallDefinitionId { get; set; }

}

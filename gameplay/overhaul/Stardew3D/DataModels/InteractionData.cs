using SpaceShared.Attributes;

namespace Stardew3D.DataModels;

[CustomDictionaryAsset("Interactions")]
public partial class InteractionData
{
    static partial void AfterRefreshData()
    {
        Mod.State.ClearHandlerState();
    }

    public List<InteractionArea> Areas { get; set; } = new();

}

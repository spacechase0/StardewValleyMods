using SpaceShared.Attributes;

namespace Stardew3D.DataModels;

[CustomDictionaryAsset("Interactions")]
public partial class InteractionData
{
    public List<InteractionArea> Areas { get; set; } = new();

    static partial void AfterRefreshData()
    {
        Mod.State.ActiveMode?.SwitchOff(Mod.State.ActiveMode);
        Mod.State.ActiveMode?.SwitchOn(Mod.State.ActiveMode);
    }
}

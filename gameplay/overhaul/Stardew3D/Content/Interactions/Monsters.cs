using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Interactions;

[DictionaryAssetData<InteractionData>("Interactions", "($/CharacterType)&", OwnedAsset = true)]
internal partial class Monsters : SpaceShared.Content.BaseDictionaryAssetData
{
    public InteractionData Bat => new()
    {
        Size = new(0.875f, 0.875f, 0.875f),
    };

    public InteractionData GreenSlime => new()
    {
        Size = new(0.875f, 0.875f, 0.875f),
    };
}

using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Interactions;

[DictionaryAssetData<InteractionData>("Interactions", "($/&)", OwnedAsset = true)]
internal partial class Generic : SpaceShared.Content.BaseDictionaryAssetData
{
    public InteractionData Grass => new()
    {
        Size = new(0.875f, 0.875f, 0.875f),
    };

    public InteractionData Tree => new()
    {
        Size = new(0.5f, 3, 0.5f),
    };

    public InteractionData HoeDirt => new()
    {
        Size = new(1, 1 / 16f, 1),
    };

    public InteractionData Flooring => new()
    {
        Size = new(1, 1 / 16f, 1),
    };

    public InteractionData Crop => new()
    {
        Size = new(0.75f, 1.25f, 0.75f),
    };
}

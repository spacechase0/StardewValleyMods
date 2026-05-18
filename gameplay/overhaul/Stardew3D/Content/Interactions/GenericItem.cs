using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Interactions;

[DictionaryAssetData<InteractionData>("Interactions", "(&)", OwnedAsset = true)]
internal partial class GenericItem : SpaceShared.Content.BaseDictionaryAssetData
{
    public InteractionData O => new()
    {
        Size = new( 0.875f, 0.875f, 0.875f ),
    };

    public InteractionData BC => new()
    {
        Size = new( 0.875f, 1.875f, 0.875f ),
    };
}

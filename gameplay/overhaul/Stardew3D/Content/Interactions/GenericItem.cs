using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Interactions;

[DictionaryAssetData<InteractionData>("Interactions", "(&)", OwnedAsset = true)]
internal partial class GenericItem : SpaceShared.Content.BaseDictionaryAssetData
{
    public InteractionData O => new()
    {
        Areas =
        [
            new BoxInteractionArea()
            {
                Purpose = $"{ModId}/Action",
                Size = new( 0.875f, 0.875f, 0.875f ),
                Translation = new( 0, 0.875f / 2, 0 ),
            }
        ],
    };

    public InteractionData BC => new()
    {
        Areas =
        [
            new BoxInteractionArea()
            {
                Purpose = $"{ModId}/Action",
                Size = new( 0.875f, 1.875f, 0.875f ),
                Translation = new( 0, 1.875f / 2, 0 ),
            }
        ],
    };
}

using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Interactions;

[DictionaryAssetData<InteractionData>("Interactions", "($/CharacterType)&", OwnedAsset = true)]
internal partial class Monsters : SpaceShared.Content.BaseDictionaryAssetData
{
    public InteractionData Bat => new()
    {
        Areas =
        [
            new BoxInteractionArea()
            {
                Purpose = $"{ModId}/Action",
                Size = new( 0.875f, 0.875f, 0.875f ),
                Translation = new( 0, 0.875f/2, 0 ),
            }
        ],
    };

    public InteractionData GreenSlime => new()
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
}

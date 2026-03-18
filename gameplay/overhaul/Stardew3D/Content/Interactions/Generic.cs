using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Interactions;

[DictionaryAssetData<InteractionData>("Interactions", "($/&)", OwnedAsset = true)]
internal partial class Generic : SpaceShared.Content.BaseDictionaryAssetData
{
    public InteractionData NPC => new()
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

    public InteractionData Monster => new()
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

    public InteractionData Grass => new()
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

    public InteractionData Tree => new()
    {
        Areas =
        [
            new BoxInteractionArea()
            {
                Purpose = $"{ModId}/Action",
                Size = new( 0.5f, 3, 0.5f ),
                Translation = new( 0, 1.5f, 0 ),
            }
        ],
    };

    public InteractionData ResourceClump => new()
    {
        Areas =
        [
            new BoxInteractionArea()
            {
                Purpose = $"{ModId}/Action",
                Size = new( 1.75f, 1, 1.75f ),
                Translation = new( 0, 0.5f, 0 ),
            }
        ],
    };
}

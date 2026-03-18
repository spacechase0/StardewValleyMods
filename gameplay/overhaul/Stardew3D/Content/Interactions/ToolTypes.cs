using Microsoft.Xna.Framework;
using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Interactions;

[DictionaryAssetData<InteractionData>("Interactions", "($/T)&", OwnedAsset = true)]
internal partial class ToolTypes : SpaceShared.Content.BaseDictionaryAssetData
{
    public InteractionData Axe => new()
    {
        Areas =
        [
            new BoxInteractionArea()
            {
                Purpose = $"{ModId}/ToolAction/Impact",
                Size = new( 0.1875f, 0.5f, 0.125f ),
                Translation = new( -0.3125f, 0.625f, 0 ),
                Rotation = new( 0, 0, MathHelper.ToRadians( 45 ) ),
            }
        ],
    };

    public InteractionData Pickaxe => new()
    {
        Areas =
        [
            new BoxInteractionArea()
            {
                Purpose = $"{ModId}/ToolAction/Impact",
                Size = new( 0.1875f, 0.1875f, 0.125f ),
                Translation = new( -0.4375f, 0.3125f+1f/16, 0 ),
                Rotation = new( 0, 0, MathHelper.ToRadians( 75 ) ),
            },
            new BoxInteractionArea()
            {
                Purpose = $"{ModId}/ToolAction/Impact",
                Size = new( 0.1875f, 0.1875f, 0.125f ),
                Translation = new( -0.1875f+0.25f+1f/32, 0.9375f, 0 ),
                Rotation = new( 0, 0, MathHelper.ToRadians( 195 ) ),
            }
        ],
    };
}

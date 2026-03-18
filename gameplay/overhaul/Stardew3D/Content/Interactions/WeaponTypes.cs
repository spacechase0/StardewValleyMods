using Microsoft.Xna.Framework;
using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Interactions;

[DictionaryAssetData<InteractionData>("Interactions", "($/W)&", OwnedAsset = true)]
internal partial class WeaponsTypes : SpaceShared.Content.BaseDictionaryAssetData
{
    public InteractionData _3 => new() // MeleeWeapon.defenseSword
    {
        Areas =
        [
            new BoxInteractionArea()
            {
                Purpose = $"{ModId}/ToolAction/Impact",
                Size = new( 0.125f, 0.875f, 0.125f ),
                Translation = new( -4/16f, 10/16f, 0 ),
                Rotation = new( 0, 0, MathHelper.ToRadians( 45 ) ),
            },
            new BoxInteractionArea()
            {
                Purpose = $"{ModId}/ToolAction/Impact",
                Size = new( 0.125f, 0.875f, 0.125f ),
                Translation = new( -2/16f, 12/16f, 0 ),
                Rotation = new( 0, 0, MathHelper.ToRadians( 45 + 180 ) ),
            }
        ],
    };
}

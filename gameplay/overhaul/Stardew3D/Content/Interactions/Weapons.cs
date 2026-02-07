using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SpaceShared.Attributes;
using Stardew3D.Data;

namespace Stardew3D.Content.Interactions;

[DictionaryAssetData<InteractionData>("Interactions", "(W)&", OwnedAsset = true)]
internal partial class Weapons : SpaceShared.Content.BaseDictionaryAssetData
{
    public InteractionData _47 => new() // Scythe
    {
        Areas =
        [
            new BoxInteractionArea()
            {
                Purpose = $"{ModId}/ToolAction/Impact",
                Size = new( 0.375f, 0.75f, 0.125f ),
                Translation = new( -0.25f, 0.6875f, 0 ),
                Rotation = new( 0, 0, MathHelper.ToRadians( 130 ) ),
            }
        ],
    };
}

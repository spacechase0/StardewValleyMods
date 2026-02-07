using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceShared.Attributes;
using Stardew3D.Data;

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
                Size = new( 0.75f, 0.75f, 0.75f ),
                Translation = new( 0, 0.75f / 2, 0 ),
            }
        ],
    };
}

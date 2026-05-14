using Microsoft.Xna.Framework;
using SpaceShared.Attributes;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace Stardew3D.DataModels;

[CustomDictionaryAsset("Interactions")]
public partial class InteractionData
{
    static partial void AfterRefreshData()
    {
        Mod.State.ClearHandlerState();
    }

    public static InteractionData Get(object obj, out Vector3 size, out string interactionId)
    {
        string firstId = null;
        interactionId = null;
        InteractionData ret = null;
        Vector3? retSize = null;
        foreach (var idEntry in obj.GetExtendedQualifiedIds())
        {
            firstId ??= idEntry;

            var data = Get(idEntry);
            if ( data == null )
                continue;

            interactionId ??= idEntry;
            ret ??= data;
            retSize ??= data.Size;
        }
        ret ??= new InteractionData() { Size = new Vector3(1, 1, 1) };
        retSize ??= Vector3.One;

        retSize *= obj switch
        {
            Monster monster => Vector3.One,
            Character character => new Vector3(character.GetBoundingBox().Width / 64f, 1.875f, character.GetBoundingBox().Height / 64f),

            ResourceClump clump => new Vector3(clump.getBoundingBox().Width / 64f - 0.125f, 1, clump.getBoundingBox().Height / 64f - 0.125f),
            TerrainFeature tf => new Vector3(tf.getBoundingBox().Width / 64f, 1, tf.getBoundingBox().Height / 64f),

            Furniture furn when furn.furniture_type.Value is Furniture.painting or Furniture.sconce => new Vector3(furn.GetBoundingBox().Width / 64f, furn.GetBoundingBox().Height / 64f, 1f),
            Furniture furn when furn.furniture_type.Value is not Furniture.painting and not Furniture.sconce => new Vector3(furn.GetBoundingBox().Width / 64f, 1, furn.GetBoundingBox().Height / 64f),

            _ => new Vector3(1, 1, 1)
        };

        size = retSize.Value;
        interactionId ??= firstId;
        return ret;
    }

    public Vector3? Size { get; set; }

    public List<InteractionArea> Areas { get; set; } =
    [
        new BoxInteractionArea() { Purpose = $"{Mod.ID}/Action" },
    ];
}

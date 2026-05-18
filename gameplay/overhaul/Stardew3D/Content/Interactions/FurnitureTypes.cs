using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Interactions;

[DictionaryAssetData<InteractionData>("Interactions", "($/FurnitureType)&", OwnedAsset = true)]
internal partial class FurnitureTypes : SpaceShared.Content.BaseDictionaryAssetData
{
    // Furniture will all be scaled by the 2d bounding box size (because it can vary within a type,
    // rotations, etc.), so we really only need the new dimension introduced.
    // Paintings and windows need 2D height in the 3D Y, unlike everything else which has it in Z.

    public InteractionData _0 => new() // Chair
    {
        Size = new( 1, 1.5f, 1 ),
    };

    public InteractionData _1 => new() // Bench
    {
        Size = new( 1, 1.5f, 1 ),
    };

    public InteractionData _2 => new() // Couch
    {
        Size = new( 1, 1.5f, 1 ),
    };

    public InteractionData _3 => new() // Arm chair
    {
        Size = new( 1, 1.5f, 1 ),
    };

    public InteractionData _4 => new() // Dresser
    {
        Size = new( 1, 1.5f, 1 ),
    };

    public InteractionData _5 => new() // Long table
    {
        Size = new( 1, 1, 1 ),
    };

    public InteractionData _6 => new() // Painting
    {
        Size = new( 1, 1, 1 / 16f ),
    };

    public InteractionData _7 => new() // Lamp
    {
        Size = new( 1, 1.5f, 1 ),
    };

    public InteractionData _8 => new() // Decor
    {
        Size = new( 1, 1.5f, 1 ),
    };

    public InteractionData _9 => new() // Other
    {
        Size = new( 1, 1, 1 ),
    };

    public InteractionData _10 => new() // Bookcase
    {
        Size = new( 1, 2, 1 ),
    };

    public InteractionData _11 => new() // Table
    {
        Size = new( 1, 1, 1 ),
    };

    public InteractionData _12 => new() // Rug
    {
        Size = new( 1, 1 / 16f, 1 ),
    };

    public InteractionData _13 => new() // Window
    {
        Size = new( 1, 1, 1 / 16f ),
    };

    public InteractionData _14 => new() // Fireplace
    {
        Size = new( 1, 3, 1 ),
    };

    public InteractionData _15 => new() // Bed
    {
        Size = new( 1, 1, 1 ),
    };

    public InteractionData _16 => new() // Torch
    {
        Size = new( 1, 1, 1 ),
    };

    public InteractionData _17 => new() // Sconce
    {
        Size = new(1, 1.5f, 1),
    };
}

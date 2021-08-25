using System;
using System.Collections.Generic;
using System.ComponentModel;
using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets.PackData
{
    public class FurniturePackData : CommonPackData
    {
        public class FurnitureConfiguration
        {
            public enum SeatDirection
            {
                Any = -1,
                Up = Game1.up,
                Down = Game1.down,
                Left = Game1.left,
                Right = Game1.right,
            }

            public string Texture { get; set; }
            [DefaultValue( null )]
            public string FrontTexture { get; set; } // for seats, beds, fish tanks
            public Vector2 DisplaySize { get; set; }
            public int CollisionHeight { get; set; }
            [DefaultValue( false )]
            public bool Flipped { get; set; }
            public List<Vector2> Seats { get; set; } = new List<Vector2>();
            [DefaultValue( SeatDirection.Any )]
            public SeatDirection SittingDirection { get; set; }

            public bool ShouldSerializeSeats() { return Seats.Count > 0; }

            public Dictionary<Vector2, Dictionary<string, Dictionary<string, string>>> TileProperties { get; set; } = new Dictionary<Vector2, Dictionary<string, Dictionary<string, string>>>();

            public bool ShouldSerializeTileProperties() { return TileProperties.Count > 0; }
        }

        public enum FurnitureType
        {
            Decoration,
            Rug,
            Table,
            Dresser,
            FishTank,
            Bed,
            Painting,
            Fireplace,
            TV,
        }

        [DefaultValue( FurnitureType.Decoration )]
        public FurnitureType Type { get; set; }

        // Bed specific
        public BedFurniture.BedType BedType { get; set; } = BedFurniture.BedType.Single;

        public bool ShouldSerializeBedType() { return Type == FurnitureType.Bed; }

        // TV specific
        public Vector2 ScreenPosition { get; set; }
        public int ScreenSize { get; set; }

        public bool ShouldSerializeScreenPositione() { return Type == FurnitureType.TV; }
        public bool ShouldSerializeScreenSize() { return Type == FurnitureType.TV; }

        // Fish tank specific
        public int TankSwimmingCapacity { get; set; } = -1;
        public int TankGroundCapacity { get; set; } = -1;
        public int TankDecorationCapacity { get; set; } = -1;

        public bool ShouldSerializeTankSwimmingCapacity() { return Type == FurnitureType.FishTank; }
        public bool ShouldSerializeTankGroundCapacity() { return Type == FurnitureType.FishTank; }
        public bool ShouldSerializeTankDecorationCapacity() { return Type == FurnitureType.FishTank; }

        public List<FurnitureConfiguration> Configurations { get; set; } = new List<FurnitureConfiguration>();

        [JsonIgnore]
        public string Name => parent.smapiPack.Translation.Get($"furniture.{ID}.name");
        [JsonIgnore]
        public string Description => parent.smapiPack.Translation.Get($"furniture.{ID}.description");

        public int GetVanillaFurnitureType()
        {
            switch ( Type )
            {
                case FurnitureType.Decoration: return Furniture.decor;
                case FurnitureType.Rug: return Furniture.rug;
                case FurnitureType.Table: return Furniture.table;
                case FurnitureType.Dresser: return Furniture.dresser;
                case FurnitureType.FishTank: return Furniture.other;
                case FurnitureType.Bed: return Furniture.bed;
                case FurnitureType.Painting: return Furniture.painting;
                case FurnitureType.Fireplace: return Furniture.fireplace;
                case FurnitureType.TV: return Furniture.decor;
            }

            return Furniture.other;
        }

        public override TexturedRect GetTexture()
        {
            return parent.GetTexture(Configurations[0].Texture, (int) Configurations[0].DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int) Configurations[0].DisplaySize.Y * Game1.tileSize / Game1.pixelZoom );
        }

        public override void OnDisabled()
        {
            MyUtility.iterateAllItems((item) =>
            {
                if (item is IDGAItem jfurn)
                {
                    if (jfurn.SourcePack == parent.smapiPack.Manifest.UniqueID && jfurn.Id == ID)
                        return null;
                }
                return item;
            });
        }

        public override Item ToItem()
        {
            switch ( Type )
            {
                case FurnitureType.Bed:
                    return new CustomBedFurniture( this );
                case FurnitureType.TV:
                    return new CustomTVFurniture( this );
                case FurnitureType.FishTank:
                    return new CustomFishTankFurniture( this );
                case FurnitureType.Dresser:
                    return new CustomStorageFurniture( this );
                default:
                    return new CustomBasicFurniture( this );
            }
        }
    }
}

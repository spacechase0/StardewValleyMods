using System;
using System.Collections.Generic;
using System.ComponentModel;
using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpaceShared;
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
            [JsonConverter( typeof( StringEnumConverter ) )]
            public SeatDirection SittingDirection { get; set; }

            public bool ShouldSerializeSeats() { return this.Seats.Count > 0; }

            public Dictionary<Vector2, Dictionary<string, Dictionary<string, string>>> TileProperties { get; set; } = new Dictionary<Vector2, Dictionary<string, Dictionary<string, string>>>();

            public bool ShouldSerializeTileProperties() { return this.TileProperties.Count > 0; }
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
        [JsonConverter( typeof( StringEnumConverter ) )]
        public FurnitureType Type { get; set; }

        // Bed specific
        [JsonConverter( typeof( StringEnumConverter ) )]
        public BedFurniture.BedType BedType { get; set; } = BedFurniture.BedType.Single;

        public bool ShouldSerializeBedType() { return this.Type == FurnitureType.Bed; }

        // TV specific
        public Vector2 ScreenPosition { get; set; }
        public int ScreenSize { get; set; }

        public bool ShouldSerializeScreenPositione() { return this.Type == FurnitureType.TV; }
        public bool ShouldSerializeScreenSize() { return this.Type == FurnitureType.TV; }

        // Fish tank specific
        public int TankSwimmingCapacity { get; set; } = -1;
        public int TankGroundCapacity { get; set; } = -1;
        public int TankDecorationCapacity { get; set; } = -1;

        public bool ShouldSerializeTankSwimmingCapacity() { return this.Type == FurnitureType.FishTank; }
        public bool ShouldSerializeTankGroundCapacity() { return this.Type == FurnitureType.FishTank; }
        public bool ShouldSerializeTankDecorationCapacity() { return this.Type == FurnitureType.FishTank; }

        public List<FurnitureConfiguration> Configurations { get; set; } = new List<FurnitureConfiguration>();

        [JsonIgnore]
        public string Name => this.pack.smapiPack.Translation.Get($"furniture.{this.ID}.name");
        [JsonIgnore]
        public string Description => this.pack.smapiPack.Translation.Get($"furniture.{this.ID}.description");

        public int GetVanillaFurnitureType()
        {
            switch (this.Type )
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
            if (this.Configurations.Count == 0 )
                return this.pack.GetTexture( null, 16, 16 );

            return this.pack.GetTexture(this.Configurations[0].Texture, (int) this.Configurations[0].DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int) this.Configurations[0].DisplaySize.Y * Game1.tileSize / Game1.pixelZoom );
        }

        public override void OnDisabled()
        {
            SpaceUtility.iterateAllItems((item) =>
            {
                if (item is IDGAItem jfurn)
                {
                    if (jfurn.SourcePack == this.pack.smapiPack.Manifest.UniqueID && jfurn.Id == this.ID)
                        return null;
                }
                return item;
            });
        }

        public override Item ToItem()
        {
            switch (this.Type )
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

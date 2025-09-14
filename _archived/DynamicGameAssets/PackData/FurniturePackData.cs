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

            [DefaultValue(null)]
            public string FrontTexture { get; set; } // for seats, beds, fish tanks

            [DefaultValue(null)]
            public string NightTexture { get; set; } // for lamps, windows, sconces

            public Vector2 DisplaySize { get; set; }
            public int CollisionHeight { get; set; }

            [DefaultValue(false)]
            public bool Flipped { get; set; }

            public List<Vector2> Seats { get; set; } = new();

            [DefaultValue(SeatDirection.Any)]
            [JsonConverter(typeof(StringEnumConverter))]
            public SeatDirection SittingDirection { get; set; }

            public bool ShouldSerializeSeats() { return this.Seats.Count > 0; }

            public Dictionary<Vector2, Dictionary<string, Dictionary<string, string>>> TileProperties { get; set; } = new();

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
            Lamp,
            Sconce,
            Window,
            Chair,
            Bench,
            Couch,
            Armchair
        }

        [DefaultValue(FurnitureType.Decoration)]
        [JsonConverter(typeof(StringEnumConverter))]
        public FurnitureType Type { get; set; }

        [DefaultValue(true)]
        public bool ShowInCatalogue { get; set; } = true;

        // Bed specific
        [JsonConverter(typeof(StringEnumConverter))]
        public BedFurniture.BedType BedType { get; set; } = BedFurniture.BedType.Single;

        public bool ShouldSerializeBedType() { return this.Type == FurnitureType.Bed; }

        // TV specific
        public Vector2 ScreenPosition { get; set; }
        public float ScreenSize { get; set; }

        public bool ShouldSerializeScreenPositione() { return this.Type == FurnitureType.TV; }
        public bool ShouldSerializeScreenSize() { return this.Type == FurnitureType.TV; }

        // Fish tank specific
        public int TankSwimmingCapacity { get; set; } = -1;
        public int TankGroundCapacity { get; set; } = -1;
        public int TankDecorationCapacity { get; set; } = -1;

        public bool ShouldSerializeTankSwimmingCapacity() { return this.Type == FurnitureType.FishTank; }
        public bool ShouldSerializeTankGroundCapacity() { return this.Type == FurnitureType.FishTank; }
        public bool ShouldSerializeTankDecorationCapacity() { return this.Type == FurnitureType.FishTank; }

        public List<FurnitureConfiguration> Configurations { get; set; } = new();

        [JsonIgnore]
        public string Name => this.pack.smapiPack.Translation.Get($"furniture.{this.ID}.name");
        [JsonIgnore]
        public string Description => this.pack.smapiPack.Translation.Get($"furniture.{this.ID}.description");
        [JsonIgnore]
        public string CategoryTextOverride => this.pack.smapiPack.Translation.Get($"furniture.{this.ID}.category").UsePlaceholder(false).ToString();

        public int GetVanillaFurnitureType()
        {
            return this.Type switch
            {
                FurnitureType.Decoration => Furniture.decor,
                FurnitureType.Rug => Furniture.rug,
                FurnitureType.Table => Furniture.table,
                FurnitureType.Dresser => Furniture.dresser,
                FurnitureType.FishTank => Furniture.other,
                FurnitureType.Bed => Furniture.bed,
                FurnitureType.Painting => Furniture.painting,
                FurnitureType.Fireplace => Furniture.fireplace,
                FurnitureType.TV => Furniture.decor,
                FurnitureType.Lamp => Furniture.lamp,
                FurnitureType.Sconce => Furniture.sconce,
                FurnitureType.Window => Furniture.window,
                FurnitureType.Chair => Furniture.chair,
                FurnitureType.Bench => Furniture.bench,
                FurnitureType.Couch => Furniture.couch,
                FurnitureType.Armchair => Furniture.armchair,
                _ => Furniture.other
            };
        }

        public override TexturedRect GetTexture()
        {
            if (this.Configurations.Count == 0)
                return this.pack.GetTexture(null, 16, 16);

            return this.pack.GetTexture(this.Configurations[0].Texture, (int)this.Configurations[0].DisplaySize.X * Game1.tileSize / Game1.pixelZoom, (int)this.Configurations[0].DisplaySize.Y * Game1.tileSize / Game1.pixelZoom);
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
            return this.Type switch
            {
                FurnitureType.Bed => new CustomBedFurniture(this),
                FurnitureType.TV => new CustomTVFurniture(this),
                FurnitureType.FishTank => new CustomFishTankFurniture(this),
                FurnitureType.Dresser => new CustomStorageFurniture(this),
                _ => new CustomBasicFurniture(this)
            };
        }
    }
}

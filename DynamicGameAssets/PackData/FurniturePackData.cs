using System;
using System.Collections.Generic;
using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
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
            public string FrontTexture { get; set; } // for seats
            public Vector2 DisplaySize { get; set; }
            public int CollisionHeight { get; set; }
            public bool Flipped { get; set; }
            public List<Vector2> Seats { get; set; } = new List<Vector2>();
            public SeatDirection SittingDirection { get; set; }

            public Dictionary<Vector2, Dictionary<string, Dictionary<string, string>>> TileProperties { get; set; } = new Dictionary<Vector2, Dictionary<string, Dictionary<string, string>>>();
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
        }

        public FurnitureType Type { get; set; }

        public List<FurnitureConfiguration> Configurations { get; set; } = new List<FurnitureConfiguration>();

        public string Name => parent.smapiPack.Translation.Get($"furniture.{ID}.name");
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
            }

            return Furniture.other;
        }

        public override TexturedRect GetTexture()
        {
            return parent.GetTexture(Configurations[0].Texture, (int) Configurations[0].DisplaySize.X, (int) Configurations[0].DisplaySize.Y);
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
            // todo - per-type
            return new CustomBasicFurniture( this );
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpaceShared;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    public class ObjectPackData : CommonPackData
    {
        public string Texture { get; set; }

        [DefaultValue(null)]
        public string TextureColor { get; set; }

        [JsonIgnore]
        public string Name => this.pack.smapiPack.Translation.Get($"object.{this.ID}.name");

        [JsonIgnore]
        public string Description => this.pack.smapiPack.Translation.Get($"object.{this.ID}.description");

        [DefaultValue(null)]
        public string Plants { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum VanillaCategory
        {
            Vegetable = StardewValley.Object.VegetableCategory,
            Fruit = StardewValley.Object.FruitsCategory,
            Flower = StardewValley.Object.flowersCategory,
            Gem = StardewValley.Object.GemCategory,
            Fish = StardewValley.Object.FishCategory,
            Egg = StardewValley.Object.EggCategory,
            Milk = StardewValley.Object.MilkCategory,
            Cooking = StardewValley.Object.CookingCategory,
            Crafting = StardewValley.Object.CraftingCategory,
            Mineral = StardewValley.Object.mineralsCategory,
            Meat = StardewValley.Object.meatCategory,
            Metal = StardewValley.Object.metalResources,
            Junk = StardewValley.Object.junkCategory,
            Syrup = StardewValley.Object.syrupCategory,
            MonsterLoot = StardewValley.Object.monsterLootCategory,
            ArtisanGoods = StardewValley.Object.artisanGoodsCategory,
            Seeds = StardewValley.Object.SeedsCategory,
            Ring = StardewValley.Object.ringCategory,
            AnimalGoods = StardewValley.Object.sellAtPierresAndMarnies,
            Greens = StardewValley.Object.GreensCategory,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public VanillaCategory Category { get; set; } = VanillaCategory.Junk;

        [JsonIgnore]
        public string CategoryTextOverride => this.pack.smapiPack.Translation.Get($"object.{this.ID}.category").UsePlaceholder(false).ToString();

        [DefaultValue(null)]
        public Color? CategoryColorOverride { get; set; } = null;

        [DefaultValue(StardewValley.Object.inedible)]
        public int Edibility { get; set; } = StardewValley.Object.inedible;

        [DefaultValue(null)]
        public int? EatenHealthRestoredOverride { get; set; } = null;

        [DefaultValue(null)]
        public int? EatenStaminaRestoredOverride { get; set; } = null;

        [DefaultValue(false)]
        public bool EdibleIsDrink { get; set; } = false;

        public class FoodBuffsData : ICloneable
        {
            [DefaultValue(0)]
            public int Farming { get; set; } = 0;

            [DefaultValue(0)]
            public int Fishing { get; set; } = 0;

            [DefaultValue(0)]
            public int Mining { get; set; } = 0;

            [DefaultValue(0)]
            public int Luck { get; set; } = 0;

            [DefaultValue(0)]
            public int Foraging { get; set; } = 0;

            [DefaultValue(0)]
            public int MaxStamina { get; set; } = 0;

            [DefaultValue(0)]
            public int MagnetRadius { get; set; } = 0;

            [DefaultValue(0)]
            public int Speed { get; set; } = 0;

            [DefaultValue(0)]
            public int Defense { get; set; } = 0;

            [DefaultValue(0)]
            public int Attack { get; set; } = 0;

            public int Duration { get; set; } = 0;

            public override bool Equals(object obj)
            {
                if (obj is FoodBuffsData other)
                {
                    if (this.Farming != other.Farming) return false;
                    if (this.Fishing != other.Fishing) return false;
                    if (this.Mining != other.Mining) return false;
                    if (this.Luck != other.Luck) return false;
                    if (this.Foraging != other.Foraging) return false;
                    if (this.MaxStamina != other.MaxStamina) return false;
                    if (this.MagnetRadius != other.MagnetRadius) return false;
                    if (this.Speed != other.Speed) return false;
                    if (this.Defense != other.Defense) return false;
                    if (this.Attack != other.Attack) return false;
                    if (this.Duration != other.Duration) return false;
                    return true;
                }
                return false;
            }

            public object Clone() => this.MemberwiseClone();
        }

        public FoodBuffsData EdibleBuffs { get; set; } = new();

        public bool ShouldSerializeEdibleBuffs() { return !this.EdibleBuffs.Equals(new FoodBuffsData()); }

        [DefaultValue(null)]
        public int? SellPrice { get; set; } = 0;

        [DefaultValue(false)]
        public bool ForcePriceOnAllInstances { get; set; } = false;

        [DefaultValue(true)]
        public bool CanTrash { get; set; } = true;

        [DefaultValue(false)]
        public bool HideFromShippingCollection { get; set; } = false;

        [DefaultValue(true)]
        public bool IsGiftable { get; set; } = true;

        [DefaultValue(20)]
        public int UniversalGiftTaste { get; set; } = 20;

        [DefaultValue(false)]
        public bool Placeable { get; set; } = false;

        [DefaultValue(null)]
        public List<Vector2> SprinklerTiles { get; set; } = null;

        [DefaultValue(null)]
        public List<Vector2> UpgradedSprinklerTiles { get; set; } = null;

        public List<string> ContextTags { get; set; } = new();

        public bool ShouldSerializeContextTags() { return this.ContextTags.Count > 0; }

        public override void OnDisabled()
        {
            SpaceUtility.iterateAllItems((item) =>
           {
               if (item is CustomObject jobj)
               {
                   if (jobj.SourcePack == this.pack.smapiPack.Manifest.UniqueID && jobj.Id == this.ID)
                       return null;
               }
               return item;
           });

            if (this.RemoveAllTracesWhenDisabled)
            {
                foreach (var farmer in Game1.getAllFarmers())
                {
                    int fakeId = $"{this.pack.smapiPack.Manifest.UniqueID}/{this.ID}".GetDeterministicHashCode();
                    if (farmer.basicShipped.ContainsKey(fakeId))
                        farmer.basicShipped.Remove(fakeId);
                }
            }
        }

        public override Item ToItem()
        {
            return new CustomObject(this);
        }

        public override TexturedRect GetTexture()
        {
            return this.pack.GetTexture(this.Texture, 16, 16);
        }

        internal string GetFakeData()
        {
            if (this.Edibility != StardewValley.Object.inedible)
            {
                int itype = (int)this.Category;
                string str = $"{this.ID}/{this.SellPrice}/{this.Edibility}/Basic {itype}/{this.Name}/{this.Description}/";
                str += (this.EdibleIsDrink ? "drink" : "food") + "/";
                if (this.EdibleBuffs == null)
                    this.EdibleBuffs = new FoodBuffsData();
                str += $"{this.EdibleBuffs.Farming} {this.EdibleBuffs.Fishing} {this.EdibleBuffs.Mining} 0 {this.EdibleBuffs.Luck} {this.EdibleBuffs.Foraging} 0 {this.EdibleBuffs.MaxStamina} {this.EdibleBuffs.MagnetRadius} {this.EdibleBuffs.Speed} {this.EdibleBuffs.Defense} {this.EdibleBuffs.Attack}/{this.EdibleBuffs.Duration}";
                return str;
            }
            else
            {
                int itype = (int)this.Category;
                return $"{this.ID}/{this.SellPrice}/{this.Edibility}/Basic {itype}/{this.Name}/{this.Description}";
            }
        }

        public override object Clone()
        {
            var ret = (ObjectPackData)base.Clone();
            ret.EdibleBuffs = (FoodBuffsData)this.EdibleBuffs.Clone();
            if (ret.SprinklerTiles != null)
                ret.SprinklerTiles = new List<Vector2>(this.SprinklerTiles);
            if (ret.UpgradedSprinklerTiles != null)
                ret.UpgradedSprinklerTiles = new List<Vector2>(this.UpgradedSprinklerTiles);
            if (ret.ContextTags != null)
                ret.ContextTags = new List<string>(this.ContextTags);
            return ret;
        }
    }
}

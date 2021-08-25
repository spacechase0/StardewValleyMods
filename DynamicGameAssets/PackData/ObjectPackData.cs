using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGameAssets.PackData
{
    public class ObjectPackData : CommonPackData
    {
        public string Texture { get; set; }
        [DefaultValue( null )]
        public string TextureColor { get; set; }

        [JsonIgnore]
        public string Name => parent.smapiPack.Translation.Get( $"object.{ID}.name" );
        [JsonIgnore]
        public string Description => parent.smapiPack.Translation.Get( $"object.{ID}.description" );

        [DefaultValue( null )]
        public string Plants { get; set; }

        [JsonConverter( typeof( StringEnumConverter ) )]
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
        [JsonConverter( typeof( StringEnumConverter ) )]
        public VanillaCategory Category { get; set; } = VanillaCategory.Junk;
        [JsonIgnore]
        public string CategoryTextOverride => parent.smapiPack.Translation.Get( $"object.{ID}.category" ).UsePlaceholder( false ).ToString();

        [DefaultValue( null )]
        public Color? CategoryColorOverride { get; set; } = null;

        [DefaultValue( StardewValley.Object.inedible )]
        public int Edibility { get; set; } = StardewValley.Object.inedible;
        [DefaultValue( null )]
        public int? EatenHealthRestoredOverride { get; set; } = null;
        [DefaultValue( null )]
        public int? EatenStaminaRestoredOverride { get; set; } = null;
        [DefaultValue( false )]
        public bool EdibleIsDrink { get; set; } = false;
        public class FoodBuffsData : ICloneable
        {
            [DefaultValue( 0 )]
            public int Farming { get; set; } = 0;
            [DefaultValue( 0 )]
            public int Fishing { get; set; } = 0;
            [DefaultValue( 0 )]
            public int Mining { get; set; } = 0;
            [DefaultValue( 0 )]
            public int Luck { get; set; } = 0;
            [DefaultValue( 0 )]
            public int Foraging { get; set; } = 0;
            [DefaultValue( 0 )]
            public int MaxStamina { get; set; } = 0;
            [DefaultValue( 0 )]
            public int MagnetRadius { get; set; } = 0;
            [DefaultValue( 0 )]
            public int Speed { get; set; } = 0;
            [DefaultValue( 0 )]
            public int Defense { get; set; } = 0;
            [DefaultValue( 0 )]
            public int Attack { get; set; } = 0;
            public int Duration { get; set; } = 0;

            public override bool Equals( object obj )
            {
                if ( obj is FoodBuffsData other )
                {
                    if ( Farming != other.Farming ) return false;
                    if ( Fishing != other.Fishing ) return false;
                    if ( Mining != other.Mining ) return false;
                    if ( Luck != other.Luck ) return false;
                    if ( Foraging != other.Foraging ) return false;
                    if ( MaxStamina != other.MaxStamina ) return false;
                    if ( MagnetRadius != other.MagnetRadius ) return false;
                    if ( Speed != other.Speed ) return false;
                    if ( Defense != other.Defense ) return false;
                    if ( Attack != other.Attack ) return false;
                    if ( Duration != other.Duration ) return false;
                    return true;
                }
                return false;
            }

            public object Clone() => this.MemberwiseClone();
        }
        public FoodBuffsData EdibleBuffs { get; set; } = new FoodBuffsData();

        public bool ShouldSerializeEdibleBuffs() { return !EdibleBuffs.Equals(new FoodBuffsData()); }

        [DefaultValue( null )]
        public int? SellPrice { get; set; } = 0;
        [DefaultValue( false )]
        public bool ForcePriceOnAllInstances = false;

        [DefaultValue( true )]
        public bool CanTrash { get; set; } = true;
        [DefaultValue( false )]
        public bool HideFromShippingCollection { get; set; } = false;

        public class GiftTasteOverrideEntry : ICloneable
        {
            public int Amount { get; set; }
            [DefaultValue( null )]
            public string NormalTextTranslationKey { get; set; }
            [DefaultValue( null )]
            public string BirthdayTextTranslationKey { get; set; }
            [DefaultValue( null )]
            public int? EmoteId { get; set; }

            public object Clone() => this.MemberwiseClone();
        }
        [DefaultValue( true )]
        public bool IsGiftable { get; set; } = true;
        [DefaultValue( 0 )]
        public int UniversalGiftTaste { get; set; } = 0;
        public Dictionary<string, GiftTasteOverrideEntry> GiftTasteOverride { get; set; } = new Dictionary<string, GiftTasteOverrideEntry>();

        public bool ShouldSerializeGiftTasteOverride() { return GiftTasteOverride.Count > 0; }

        [DefaultValue( false )]
        public bool Placeable { get; set; } = false;
        [DefaultValue( null )]
        public List<Vector2> SprinklerTiles { get; set; } = null;
        [DefaultValue( null )]
        public List<Vector2> UpgradedSprinklerTiles { get; set; } = null;

        public List<string> ContextTags { get; set; } = new List<string>();

        public bool ShouldSerializeContextTags() { return ContextTags.Count > 0; }

        public override void OnDisabled()
        {
            MyUtility.iterateAllItems( ( item ) =>
            {
                if ( item is CustomObject jobj )
                {
                    if ( jobj.SourcePack == parent.smapiPack.Manifest.UniqueID && jobj.Id == ID )
                        return null;
                }
                return item;
            } );

            if ( RemoveAllTracesWhenDisabled )
            {
                foreach ( var farmer in Game1.getAllFarmers() )
                {
                    int fakeId = $"{parent.smapiPack.Manifest.UniqueID}/{ID}".GetDeterministicHashCode();
                    if ( farmer.basicShipped.ContainsKey( fakeId ) )
                        farmer.basicShipped.Remove( fakeId );
                }
            }
        }

        public override Item ToItem()
        {
            return new CustomObject( this );
        }

        public override TexturedRect GetTexture()
        {
            return parent.GetTexture(Texture, 16, 16);
        }

        internal string GetFakeData()
        {
            if ( Edibility != StardewValley.Object.inedible )
            {
                int itype = ( int ) Category;
                string str = $"{ID}/{SellPrice}/{Edibility}/Basic {itype}/{Name}/{Description}/";
                str += ( EdibleIsDrink ? "drink" : "food" ) + "/";
                if ( EdibleBuffs == null )
                    EdibleBuffs = new FoodBuffsData();
                str += $"{EdibleBuffs.Farming} {EdibleBuffs.Fishing} {EdibleBuffs.Mining} 0 {EdibleBuffs.Luck} {EdibleBuffs.Foraging} 0 {EdibleBuffs.MaxStamina} {EdibleBuffs.MagnetRadius} {EdibleBuffs.Speed} {EdibleBuffs.Defense} {EdibleBuffs.Attack}/{EdibleBuffs.Duration}";
                return str;
            }
            else
            {
                int itype = ( int ) Category;
                return $"{ID}/{SellPrice}/{Edibility}/Basic {itype}/{Name}/{Description}";
            }
        }

        public override object Clone()
        {
            var ret = ( ObjectPackData ) base.Clone();
            ret.EdibleBuffs = ( FoodBuffsData ) EdibleBuffs.Clone();
            if ( GiftTasteOverride != null )
            {
                ret.GiftTasteOverride = new Dictionary<string, GiftTasteOverrideEntry>();
                foreach ( var entry in GiftTasteOverride )
                {
                    ret.GiftTasteOverride.Add( entry.Key, ( GiftTasteOverrideEntry ) entry.Value.Clone() );
                }
            }
            if ( ret.SprinklerTiles != null )
                ret.SprinklerTiles = new List<Vector2>( SprinklerTiles );
            if ( ret.UpgradedSprinklerTiles != null )
                ret.UpgradedSprinklerTiles = new List<Vector2>( UpgradedSprinklerTiles );
            if ( ret.ContextTags != null )
                ret.ContextTags = new List<string>( ContextTags );
            return ret;
        }
    }
}

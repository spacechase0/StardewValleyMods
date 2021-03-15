using JsonAssets.Game;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO: Light?
// TODO: Deconstructor output patch?

namespace JsonAssets.PackData
{
    public class ObjectPackData : CommonPackData
    {
        public string Texture { get; set; }

        public string Name => parent.smapiPack.Translation.Get( $"object.{ID}.name" );
        public string Description => parent.smapiPack.Translation.Get( $"object.{ID}.description" );

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
        public VanillaCategory Category { get; set; } = VanillaCategory.Junk;
        public string CategoryTextOverride => parent.smapiPack.Translation.Get( $"object.{ID}.category" ).UsePlaceholder( false ).ToString();

        public Color? CategoryColorOverride { get; set; } = null;

        public int Edibility { get; set; } = StardewValley.Object.inedible;
        public int? EatenHealthRestoredOverride { get; set; } = null;
        public int? EatenStaminaRestoredOverride { get; set; } = null;
        public bool EdibleIsDrink { get; set; } = false;
        public class FoodBuffsData : ICloneable
        {
            public int Farming { get; set; } = 0;
            public int Fishing { get; set; } = 0;
            public int Mining { get; set; } = 0;
            public int Luck { get; set; } = 0;
            public int Foraging { get; set; } = 0;
            public int MaxStamina { get; set; } = 0;
            public int MagnetRadius { get; set; } = 0;
            public int Speed { get; set; } = 0;
            public int Defense { get; set; } = 0;
            public int Attack { get; set; } = 0;
            public int Duration { get; set; } = 0;

            public object Clone() => this.MemberwiseClone();
        }
        public FoodBuffsData EdibleBuffs { get; set; } = new FoodBuffsData();

        public int? SellPrice { get; set; } = 0;
        public bool ForcePriceOnAllInstances = false;
        public bool CanTrash { get; set; } = true;
        public bool HideFromShippingCollection { get; set; } = false;

        public class GiftTasteOverrideEntry : ICloneable
        {
            public int Amount { get; set; }
            public string NormalTextTranslationKey { get; set; }
            public string BirthdayTextTranslationKey { get; set; }
            public int? EmoteId { get; set; }

            public object Clone() => this.MemberwiseClone();
        }
        public bool IsGiftable { get; set; } = true;
        public int UniversalGiftTaste { get; set; } = 0;
        public Dictionary<string, GiftTasteOverrideEntry> GiftTasteOverride { get; set; } = new Dictionary<string, GiftTasteOverrideEntry>();

        public bool Placeable { get; set; } = false;
        public List<Vector2> SprinklerTiles { get; set; } = null;
        public List<Vector2> UpgradedSprinklerTiles { get; set; } = null;

        public List<string> ContextTags { get; set; } = new List<string>();

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
                    var fakeId = $"{parent.smapiPack.Manifest.UniqueID}/{ID}".GetHashCode();
                    if ( farmer.basicShipped.ContainsKey( fakeId ) )
                        farmer.basicShipped.Remove( fakeId );
                }
            }
        }

        public override Item ToItem()
        {
            return new CustomObject( this );
        }

        internal string GetFakeData()
        {
            if ( Edibility != StardewValley.Object.inedible )
            {
                int itype = ( int ) Category;
                var str = $"{ID}/{SellPrice}/{Edibility}/Basic {itype}/{Name}/{Description}/";
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

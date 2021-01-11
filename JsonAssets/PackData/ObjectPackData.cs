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

// TODO: Buff duration implementation (harmony)
// TODO: Light?
// TODO: Deconstructor output patch?
// TODO: Sprinkler stuff?
// TODO: Placeable data?

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
        public VanillaCategory Category { get; set; }
        public string CategoryTextOverride => parent.smapiPack.Translation.Get( $"object.{ID}.category" ).UsePlaceholder( false ).ToString();

        public Color? CategoryColorOverride { get; set; } = null;

        public int Edibility { get; set; } = StardewValley.Object.inedible;
        public int? EatenHealthRestoredOverride { get; set; } = null;
        public int? EatenStaminaRestoredOverride { get; set; } = null;
        public bool EdibleIsDrink { get; set; } = false;
        public class FoodBuffsData
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
        }
        public FoodBuffsData EdibleBuffs { get; set; } = new FoodBuffsData();

        public int? SellPrice { get; set; } = 0;
        public bool CanTrash { get; set; } = true;

        public class GiftTasteOverrideEntry
        {
            public int Amount;
            public string NormalTextTranslationKey;
            public string BirthdayTextTranslationKey;
            public int? EmoteId;
        }
        public bool IsGiftable { get; set; } = true;
        public int UniversalGiftTaste { get; set; } = 0;
        public Dictionary<string, GiftTasteOverrideEntry> GiftTasteOverride = new Dictionary<string, GiftTasteOverrideEntry>();

        public List<string> ContextTags { get; set; } = new List<string>();

        public override void OnDisabled()
        {
            throw new NotImplementedException();
        }

        public override Item ToItem()
        {
            return new CustomObject( this );
        }
    }
}

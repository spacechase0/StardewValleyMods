using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SObject = StardewValley.Object;

namespace JsonAssets.Data
{
    public class ObjectData : DataNeedsId
    {
        [JsonIgnore]
        internal string directory;
        [JsonIgnore]
        internal string imageName = "object.png";

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category_
        {
            Vegetable = SObject.VegetableCategory,
            Fruit = SObject.FruitsCategory,
            Flower = SObject.flowersCategory,
            Gem = SObject.GemCategory,
            Fish = SObject.FishCategory,
            Egg = SObject.EggCategory,
            Milk = SObject.MilkCategory,
            Cooking = SObject.CookingCategory,
            Crafting = SObject.CraftingCategory,
            Mineral = SObject.mineralsCategory,
            Meat = SObject.meatCategory,
            Metal = SObject.metalResources,
            Junk = SObject.junkCategory,
            Syrup = SObject.syrupCategory,
            MonsterLoot = SObject.monsterLootCategory,
            Seeds = SObject.SeedsCategory,
        }

        public class Recipe_
        {
            // TODO: CanPurchase and purchase price
            public int ResultCount { get; set; } = 1;
            public IList<object> Ingredients { get; set; } = new List<object>();
        }
        
        public string Description { get; set; }
        public Category_ Category { get; set; }
        public int Edibility { get; set; } = SObject.inedible;
        // TODO: Edible type to determine food or drink
        // TODO: Buffs
        public int Price { get; set; }
        // TODO: CanPurchase and where to purchase
        public bool IsColored { get; set; } = false;
        public Recipe_ Recipe { get; set; }
        // TODO: Gift taste overrides.
        
        public int GetObjectId() { return id; }

        internal string GetObjectInformation()
        {
            var itype = (int)Category;
            return $"{Name}/{Price}/{Edibility}/Basic {itype}/{Name}/{Description}";
        }
    }
}

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
            public class Ingredient
            {
                public object Object { get; set; }
                public int Count { get; set; }
            }
            // TODO: CanPurchase, where, and purchase price
            // Possibly friendship option (letters, like vanilla) and/or skill levels (on levelup?)
            public int ResultCount { get; set; } = 1;
            public IList<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

            internal string GetRecipeString( ObjectData parent )
            {
                var str = "";
                foreach (var ingredient in Ingredients)
                    str += Mod.instance.ResolveObjectId(ingredient.Object) + " " + ingredient.Count + " ";
                str = str.Substring(0, str.Length - 1);
                str += $"/what is this for?/{parent.id}/";
                if (parent.Category != Category_.Cooking)
                    str += "false/";
                str += "/null"; // TODO: Requirement
                return str;
            }
        }
        
        public string Description { get; set; }
        public Category_ Category { get; set; }
        public int Edibility { get; set; } = SObject.inedible;
        // TODO: Edible type to determine food or drink
        // TODO: Buffs
        public int Price { get; set; }
        // TODO: CanPurchase, where, and purchase price
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

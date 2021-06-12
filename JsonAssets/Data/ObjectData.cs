using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StardewValley;
using SObject = StardewValley.Object;

namespace JsonAssets.Data
{
    public class ObjectData : DataNeedsIdWithTexture
    {
        [JsonIgnore]
        public Texture2D textureColor;

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
            ArtisanGoods = SObject.artisanGoodsCategory,
            Seeds = SObject.SeedsCategory,
            Ring = SObject.ringCategory,
            AnimalGoods = SObject.sellAtPierresAndMarnies,
            Greens = SObject.GreensCategory,
            Artifact = int.MinValue, // Special case
        }

        public class Recipe_
        {
            public class Ingredient
            {
                public object Object { get; set; }
                public int Count { get; set; }
            }

            public string SkillUnlockName { get; set; } = null;
            public int SkillUnlockLevel { get; set; } = -1;

            public int ResultCount { get; set; } = 1;
            public IList<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

            public bool IsDefault { get; set; } = false;
            public bool CanPurchase { get; set; } = false;
            public int PurchasePrice { get; set; }
            public string PurchaseFrom { get; set; } = "Gus";
            public IList<string> PurchaseRequirements { get; set; } = new List<string>();
            public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();

            internal string GetRecipeString(ObjectData parent)
            {
                string str = "";
                foreach (var ingredient in this.Ingredients)
                    str += Mod.instance.ResolveObjectId(ingredient.Object) + " " + ingredient.Count + " ";
                str = str.Substring(0, str.Length - 1);
                str += $"/what is this for?/{parent.id} {this.ResultCount}/";
                if (parent.Category != Category_.Cooking)
                    str += "false/";
                if (this.SkillUnlockName?.Length > 0 && this.SkillUnlockLevel > 0)
                    str += "/" + this.SkillUnlockName + " " + this.SkillUnlockLevel;
                else
                    str += "/null";
                if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
                    str += "/" + parent.LocalizedName();
                return str;
            }
        }

        public class FoodBuffs_
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

        public string Description { get; set; }
        public Category_ Category { get; set; }
        public string CategoryTextOverride { get; set; }
        public Color CategoryColorOverride { get; set; } = new(0, 0, 0, 0);
        public bool IsColored { get; set; } = false;

        public int Price { get; set; }

        public bool CanTrash { get; set; } = true;
        public bool CanSell { get; set; } = true;
        public bool CanBeGifted { get; set; } = true;

        public bool HideFromShippingCollection { get; set; } = false;

        public Recipe_ Recipe { get; set; }

        public int Edibility { get; set; } = SObject.inedible;
        public bool EdibleIsDrink { get; set; } = false;
        public FoodBuffs_ EdibleBuffs = new();

        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Pierre";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();
        public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public class GiftTastes_
        {
            public IList<string> Love = new List<string>();
            public IList<string> Like = new List<string>();
            public IList<string> Neutral = new List<string>();
            public IList<string> Dislike = new List<string>();
            public IList<string> Hate = new List<string>();
        }
        public GiftTastes_ GiftTastes;

        public Dictionary<string, string> NameLocalization = new();
        public Dictionary<string, string> DescriptionLocalization = new();

        public List<string> ContextTags = new();

        public string LocalizedName()
        {
            var currLang = LocalizedContentManager.CurrentLanguageCode;
            /*if (currLang == LocalizedContentManager.LanguageCode.en)
                return Name;*/
            if (this.NameLocalization == null || !this.NameLocalization.ContainsKey(currLang.ToString()))
                return this.Name;
            return this.NameLocalization[currLang.ToString()];
        }

        public string LocalizedDescription()
        {
            var currLang = LocalizedContentManager.CurrentLanguageCode;
            /*if (currLang == LocalizedContentManager.LanguageCode.en)
                return Description;*/
            if (this.DescriptionLocalization == null || !this.DescriptionLocalization.ContainsKey(currLang.ToString()))
                return this.Description;
            return this.DescriptionLocalization[currLang.ToString()];
        }

        public int GetObjectId() { return this.id; }

        internal string GetObjectInformation()
        {
            if (this.Edibility != SObject.inedible)
            {
                int itype = (int)this.Category;
                string str = $"{this.Name}/{this.Price}/{this.Edibility}/" + (this.Category == Category_.Artifact ? "Arch" : $"{this.Category} {itype}") + $"/{this.LocalizedName()}/{this.LocalizedDescription()}/";
                str += (this.EdibleIsDrink ? "drink" : "food") + "/";
                if (this.EdibleBuffs == null)
                    this.EdibleBuffs = new FoodBuffs_();
                str += $"{this.EdibleBuffs.Farming} {this.EdibleBuffs.Fishing} {this.EdibleBuffs.Mining} 0 {this.EdibleBuffs.Luck} {this.EdibleBuffs.Foraging} 0 {this.EdibleBuffs.MaxStamina} {this.EdibleBuffs.MagnetRadius} {this.EdibleBuffs.Speed} {this.EdibleBuffs.Defense} {this.EdibleBuffs.Attack}/{this.EdibleBuffs.Duration}";
                return str;
            }
            else
            {
                int itype = (int)this.Category;
                return $"{this.Name}/{this.Price}/{this.Edibility}/" + (this.Category == Category_.Artifact ? "Arch" : $"Basic {itype}") + $"/{this.LocalizedName()}/{this.LocalizedDescription()}";
            }
        }
    }
}

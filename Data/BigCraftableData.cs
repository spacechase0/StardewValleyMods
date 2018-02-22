using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace JsonAssets.Data
{
    public class BigCraftableData : DataNeedsId
    {
        [JsonIgnore]
        internal Texture2D texture;

        public class Recipe_
        {
            public class Ingredient
            {
                public object Object { get; set; }
                public int Count { get; set; }
            }
            // Possibly friendship option (letters, like vanilla) and/or skill levels (on levelup?)
            public int ResultCount { get; set; } = 1;
            public IList<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

            public bool IsDefault { get; set; } = false;
            public bool CanPurchase { get; set; } = false;
            public int PurchasePrice { get; set; }
            public string PurchaseFrom { get; set; } = "Gus";
            public IList<string> PurchaseRequirements { get; set; } = new List<string>();

            internal string GetRecipeString( BigCraftableData parent )
            {
                var str = "";
                foreach (var ingredient in Ingredients)
                    str += Mod.instance.ResolveObjectId(ingredient.Object) + " " + ingredient.Count + " ";
                str = str.Substring(0, str.Length - 1);
                str += $"/what is this for?/{parent.id}/true/null";
                return str;
            }

            internal string GetPurchaseRequirementString()
            {
                var str = $"1234567890";
                foreach (var cond in PurchaseRequirements)
                    str += $"/{cond}";
                return str;
            }
        }
        
        public string Description { get; set; }

        public int Price { get; set; }

        public bool ProvidesLight { get; set; } = false;

        public Recipe_ Recipe { get; set; }

        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Pierre";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();

        // TODO: Gift taste overrides.

        public int GetCraftableId() { return id; }

        internal string GetCraftableInformation()
        {
            string str = $"{Name}/{Price}/-300/Crafting -9/{Description}/true/true/0";
            if (ProvidesLight)
                str += "/true";
            return str;
        }

        internal string GetPurchaseRequirementString()
        {
            var str = $"1234567890";
            foreach (var cond in PurchaseRequirements)
                str += $"/{cond}";
            return str;
        }
    }
}

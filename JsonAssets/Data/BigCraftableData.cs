using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;

namespace JsonAssets.Data
{
    public class BigCraftableData : DataNeedsIdWithTexture
    {
        [JsonIgnore]
        public Texture2D[] extraTextures;

        public bool ReserveNextIndex { get; set; } = false; // Deprecated
        public int ReserveExtraIndexCount { get; set; } = 0;

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

            internal string GetRecipeString(BigCraftableData parent)
            {
                var str = "";
                foreach (var ingredient in this.Ingredients)
                    str += Mod.instance.ResolveObjectId(ingredient.Object) + " " + ingredient.Count + " ";
                str = str.Substring(0, str.Length - 1);
                str += $"/what is this for?/{parent.id} {this.ResultCount}/true/";
                if (this.SkillUnlockName?.Length > 0 && this.SkillUnlockLevel > 0)
                    str += this.SkillUnlockName + " " + this.SkillUnlockLevel;
                else
                    str += "null";
                if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
                    str += "/" + parent.LocalizedName();
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
        public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public Dictionary<string, string> NameLocalization = new Dictionary<string, string>();
        public Dictionary<string, string> DescriptionLocalization = new Dictionary<string, string>();

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

        public int GetCraftableId() { return this.id; }

        internal string GetCraftableInformation()
        {
            string str = $"{this.Name}/{this.Price}/-300/Crafting -9/{this.LocalizedDescription()}/true/true/0";
            if (this.ProvidesLight)
                str += "/true";
            str += $"/{this.LocalizedName()}";
            return str;
        }
    }
}

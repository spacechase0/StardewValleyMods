using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using System.Collections.Generic;

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

            internal string GetRecipeString( BigCraftableData parent )
            {
                var str = "";
                foreach (var ingredient in Ingredients)
                    str += Mod.instance.ResolveObjectId(ingredient.Object) + " " + ingredient.Count + " ";
                str = str.Substring(0, str.Length - 1);
                str += $"/what is this for?/{parent.id} {ResultCount}/true/";
                if (SkillUnlockName?.Length > 0 && SkillUnlockLevel > 0)
                    str += SkillUnlockName + " " + SkillUnlockLevel;
                else
                    str += "null";
                if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
                    str += "/" + parent.LocalizedName();
                return str;
            }

            internal string GetPurchaseRequirementString()
            {
                if ( PurchaseRequirements == null )
                    return "";
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
        public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public Dictionary<string, string> NameLocalization = new Dictionary<string, string>();
        public Dictionary<string, string> DescriptionLocalization = new Dictionary<string, string>();

        public string LocalizedName()
        {
            var currLang = LocalizedContentManager.CurrentLanguageCode;
            /*if (currLang == LocalizedContentManager.LanguageCode.en)
                return Name;*/
            if (NameLocalization == null || !NameLocalization.ContainsKey(currLang.ToString()))
                return Name;
            return NameLocalization[currLang.ToString()];
        }

        public string LocalizedDescription()
        {
            var currLang = LocalizedContentManager.CurrentLanguageCode;
            /*if (currLang == LocalizedContentManager.LanguageCode.en)
                return Description;*/
            if (DescriptionLocalization == null || !DescriptionLocalization.ContainsKey(currLang.ToString()))
                return Description;
            return DescriptionLocalization[currLang.ToString()];
        }

        public int GetCraftableId() { return id; }

        internal string GetCraftableInformation()
        {
            string str = $"{Name}/{Price}/-300/Crafting -9/{LocalizedDescription()}/true/true/0";
            if (ProvidesLight)
                str += "/true";
            str += $"/{LocalizedName()}";
            return str;
        }

        internal string GetPurchaseRequirementString()
        {
            if ( PurchaseRequirements == null )
                return "";
            var str = $"1234567890";
            foreach (var cond in PurchaseRequirements)
                str += $"/{cond}";
            return str;
        }
    }
}

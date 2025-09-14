using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JsonAssets.Framework;
using SpaceShared;
using StardewValley;

namespace JsonAssets.Data
{
    public class ObjectRecipe
    {
        /*********
        ** Accessors
        *********/
        public string SkillUnlockName { get; set; } = null;
        public int SkillUnlockLevel { get; set; } = -1;

        public int ResultCount { get; set; } = 1;
        public IList<ObjectIngredient> Ingredients { get; set; } = new List<ObjectIngredient>();

        public bool IsDefault { get; set; } = false;
        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Gus";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();
        public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();


        /*********
        ** Public methods
        *********/
        internal string GetRecipeString(ObjectData parent)
        {
            string str = "";
            foreach (var ingredient in this.Ingredients)
            {
                string ingredientName = ingredient.Object.ToString();
                // If the original object name is an integer, it's a category or an original ID
                if (int.TryParse(ingredientName, out int ingredIndex))
                {
                    ingredientName = ingredIndex.ToString();
                    // Check if it's valid item or category, if it's not then skip adding the item
                    if (ItemRegistry.GetDataOrErrorItem(ingredientName).IsErrorItem && StardewValley.Object.GetCategoryDisplayName(ingredIndex) == "")
                    {
                        Log.Warn($"Invalid recipe ingredient:{ingredient.Object.ToString()}");
                        continue;
                    }
                }
                // If the object is a JA object, then just use that
                else if (ingredient.Object.ToString().FixIdJA("O") != null)
                {
                    ingredientName = ingredient.Object.ToString().FixIdJA("O");
                }
                // If the object isn't an integer, or a JA object, or an existing item, check if it's close to the name of any existing item and use that if so
                else if (ItemRegistry.GetDataOrErrorItem(ingredientName).IsErrorItem)
                {
                    Item tryGetItem = Utility.fuzzyItemSearch(ingredientName);
                    if (tryGetItem != null)
                    {
                        ingredientName = tryGetItem.ItemId;
                    }
                    // Don't add the ingredient if it's not a valid item
                    else
                    {
                        Log.Warn($"Invalid recipe ingredient:{ingredient.Object.ToString()}");
                        continue;
                    }
                }
                // Finally, add the ingredient name if it now matches an existing item
                str += ingredientName + " " + ingredient.Count + " ";
            }
            // If no ingredients were added, add the torch recipe ingredients
            if (str == "")
            {
                Log.Warn($"Recipe with no valid ingredients:{parent.Name}");
                str += "388 1 92 2 ";
            }
            str = str.Substring(0, str.Length - 1);
            if (parent.Category != ObjectCategory.Cooking)
                str += "/what is this for?";
            else
                str += "/9999 9999";
            str += $"/{parent.Name.FixIdJA("O")} {this.ResultCount}/";
            if (parent.Category != ObjectCategory.Cooking)
                str += "false/";
            if (this.SkillUnlockName?.Length > 0 && this.SkillUnlockLevel > 0)
                str += "" + this.SkillUnlockName + " " + this.SkillUnlockLevel;
            else
                str += "null";
            //if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
                str += "/" + parent.LocalizedName();
            return str;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.Ingredients ??= new List<ObjectIngredient>();
            this.PurchaseRequirements ??= new List<string>();
            this.AdditionalPurchaseData ??= new List<PurchaseData>();

            this.Ingredients.FilterNulls();
            this.PurchaseRequirements.FilterNulls();
            this.AdditionalPurchaseData.FilterNulls();
        }
    }
}

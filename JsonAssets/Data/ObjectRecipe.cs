using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

using JsonAssets.Framework;
using JsonAssets.Utilities;

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
                int id = ItemResolver.GetObjectID(ingredient.Object);
                if (id == 0)
                    continue;
                str += id + " " + ingredient.Count + " ";
            }

            if (str.Length == 0)
                throw new InvalidDataException("No ingredients could be resolved.");

            str = str.Substring(0, str.Length - 1);
            str += $"/what is this for?/{parent.Id} {this.ResultCount}/";
            if (parent.Category != ObjectCategory.Cooking)
                str += "false/";
            if (this.SkillUnlockName?.Length > 0 && this.SkillUnlockLevel > 0)
                str += "/" + this.SkillUnlockName + " " + this.SkillUnlockLevel;
            else
                str += "/null";
            if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
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

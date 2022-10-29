using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

using JsonAssets.Framework;
using JsonAssets.Framework.Internal;
using JsonAssets.Utilities;

using StardewValley;

namespace JsonAssets.Data
{
    public class BigCraftableRecipe
    {
        /*********
        ** Accessors
        *********/
        public string SkillUnlockName { get; set; } = null;
        public int SkillUnlockLevel { get; set; } = -1;

        public int ResultCount { get; set; } = 1;
        public IList<BigCraftableIngredient> Ingredients { get; set; } = new List<BigCraftableIngredient>();

        public bool IsDefault { get; set; } = false;
        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Gus";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();
        public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();


        /*********
        ** Public methods
        *********/
        internal string GetRecipeString(BigCraftableData parent)
        {
            StringBuilder str = StringBuilderCache.Acquire();
            foreach (BigCraftableIngredient ingredient in this.Ingredients)
            {
                int id = ItemResolver.GetObjectID(ingredient.Object);
                if (id == 0)
                    continue;
                str.Append(id).Append(' ').Append(ingredient.Count).Append(' ');
            }

            if (str.Length == 0)
                throw new InvalidDataException("No valid ingredients could be found, skipping this recipe.");

            str.Remove(str.Length - 1, 1);
            str.Append("/what is this for?/")
                .Append(parent.Id).Append(' ').Append(this.ResultCount).Append("/true/");
            if (this.SkillUnlockName?.Length > 0 && this.SkillUnlockLevel > 0)
                str.Append(this.SkillUnlockName).Append(' ').Append(this.SkillUnlockLevel);
            else
                str.Append("null");
            //if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
            str.Append('/').Append(parent.LocalizedName());
            return StringBuilderCache.GetStringAndRelease(str);
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.Ingredients ??= new List<BigCraftableIngredient>();
            this.PurchaseRequirements ??= new List<string>();
            this.AdditionalPurchaseData ??= new List<PurchaseData>();

            this.Ingredients.FilterNulls();
            this.PurchaseRequirements.FilterNulls();
            this.AdditionalPurchaseData.FilterNulls();
        }
    }
}

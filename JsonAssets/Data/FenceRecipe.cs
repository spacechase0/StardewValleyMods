using System.Collections.Generic;

namespace JsonAssets.Data
{
    public class FenceRecipe
    {
        public string SkillUnlockName { get; set; } = null;
        public int SkillUnlockLevel { get; set; } = -1;

        public int ResultCount { get; set; } = 1;
        public IList<FenceIngredient> Ingredients { get; set; } = new List<FenceIngredient>();

        public bool IsDefault { get; set; } = false;
        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Robin";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();
        public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();
    }
}

using DynamicGameAssets.Framework;
using DynamicGameAssets.PackData;
using SpaceCore;
using StardewValley;

namespace DynamicGameAssets
{
    public class DGACustomForgeRecipe : CustomForgeRecipe
    {
        private class DGAIngredientMatcher : IngredientMatcher
        {
            private readonly ItemAbstraction ingred;

            public DGAIngredientMatcher(ItemAbstraction theIngred)
            {
                this.ingred = theIngred;
            }

            public override bool HasEnoughFor(Item item)
            {
                if (this.ItemMatches(item) && item.Stack >= this.ingred.Quantity)
                    return true;
                return false;
            }

            public override void Consume(ref Item item)
            {
                int left = this.ingred.Quantity;
                if (this.ItemMatches(item))
                {
                    if (item.Stack <= left)
                        item = null;
                    else
                        item.Stack -= left;
                }
            }

            private bool ItemMatches(Item item)
            {
                return this.ingred.Matches(item);
            }
        }

        internal ForgeRecipePackData data;

        private IngredientMatcher cacheBase;
        private IngredientMatcher cacheIngred;

        public DGACustomForgeRecipe(ForgeRecipePackData theData)
        {
            this.data = theData;
            this.Refresh();
        }

        public void Refresh()
        {
            this.cacheBase = new DGAIngredientMatcher(this.data.BaseItem);
            this.cacheIngred = new DGAIngredientMatcher(this.data.IngredientItem);
        }


        public override IngredientMatcher BaseItem => this.cacheBase;
        public override IngredientMatcher IngredientItem => this.cacheIngred;
        public override int CinderShardCost => this.data.CinderShardCost;

        public override Item CreateResult(Item baseItem, Item ingredItem)
        {
            // TODO: Random based on game seed and day
            return this.data.Result.Choose().Create();
        }
    }
}

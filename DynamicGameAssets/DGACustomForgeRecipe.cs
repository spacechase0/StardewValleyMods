using System;
using System.Collections.Generic;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SpaceShared;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets
{
    public class DGACustomForgeRecipe : CustomForgeRecipe
    {
        private class DGAIngredientMatcher : IngredientMatcher
        {
            private ItemAbstraction ingred;

            public DGAIngredientMatcher( ItemAbstraction theIngred )
            {
                ingred = theIngred;
            }

            public override bool HasEnoughFor(Item item)
            {
                if ( ItemMatches( item ) && item.Stack >= ingred.Quantity )
                    return true;
                return false;
            }

            public override void Consume(ref Item item)
            {
                int left = ingred.Quantity;
                if ( ItemMatches( item ) )
                {
                    if ( item.Stack <= left )
                        item = null;
                    else
                        item.Stack -= left;

                    left -= item.Stack;
                }
            }

            private bool ItemMatches(Item item)
            {
                return ingred.Matches(item);
            }
        }

        internal ForgeRecipePackData data;

        private IngredientMatcher cacheBase;
        private IngredientMatcher cacheIngred;

        public DGACustomForgeRecipe( ForgeRecipePackData theData )
        {
            data = theData;
            Refresh();
        }

        public void Refresh()
        {
            cacheBase = new DGAIngredientMatcher( data.BaseItem );
            cacheIngred = new DGAIngredientMatcher( data.IngredientItem );
        }


        public override IngredientMatcher BaseItem => cacheBase;
        public override IngredientMatcher IngredientItem => cacheIngred;
        public override int CinderShardCost => data.CinderShardCost;

        public override Item CreateResult( Item baseItem, Item ingredItem )
        {
            // TODO: Random based on game seed and day
            return data.Result.Choose().Create();
        }
    }
}

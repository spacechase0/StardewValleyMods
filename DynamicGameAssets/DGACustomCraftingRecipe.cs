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
    public class DGACustomCraftingRecipe : CustomCraftingRecipe
    {
        private class DGAIngredientMatcher : IngredientMatcher
        {
            private CraftingRecipePackData.IngredientAbstraction ingred;

            private string cacheName;
            private Texture2D cacheIconTex;
            private Rectangle cacheIconRect;

            public DGAIngredientMatcher(CraftingRecipePackData.IngredientAbstraction theIngred )
            {
                ingred = theIngred;
                cacheName = ingred.NameOverride ?? ingred.Create().DisplayName;
                cacheIconTex = ingred.Icon;
                cacheIconRect = ingred.IconSubrect;
            }

            public override string DispayName => cacheName;

            public override Texture2D IconTexture => cacheIconTex;

            public override Rectangle? IconSubrect => cacheIconRect;

            public override int Quantity => ingred.Quantity;

            public override int GetAmountInList(IList<Item> items)
            {
                int ret = 0;
                foreach (var item in items)
                {
                    if (ItemMatches(item))
                        ret += item.Stack;
                }

                return ret;
            }

            public override void Consume(IList<Chest> additionalIngredients)
            {
                int left = Quantity;
                for (int i = Game1.player.Items.Count - 1; i >= 0; --i)
                {
                    var item = Game1.player.Items[i];
                    if (ItemMatches(item))
                    {
                        if (item.Stack <= left)
                            Game1.player.items[i] = null;
                        else
                            item.Stack -= left;

                        left -= item.Stack;

                        if (left <= 0)
                            break;
                    }
                }

                if (left > 0)
                {
                    foreach (var chest in additionalIngredients)
                    {
                        bool removed = false;
                        for (int i = chest.items.Count - 1; i >= 0; --i)
                        {
                            var item = chest.items[i];
                            if (ItemMatches(item))
                            {
                                if (item.Stack <= left)
                                {
                                    removed = true;
                                    chest.items[i] = null;
                                }
                                else
                                    item.Stack -= left;

                                left -= item.Stack;

                                if (left <= 0)
                                    break;
                            }
                        }

                        if (removed)
                            chest.clearNulls();
                        if (left <= 0)
                            break;
                    }
                }
            }

            private bool ItemMatches(Item item)
            {
                return ingred.Matches(item);
            }
        }

        internal CraftingRecipePackData data;

        private Texture2D cacheIconTex;
        private Rectangle cacheIconRect;
        private IngredientMatcher[] cacheIngreds;

        public DGACustomCraftingRecipe( CraftingRecipePackData theData )
        {
            data = theData;
            Refresh();
        }

        public void Refresh()
        {
            cacheIconTex = data.Result[ 0 ].Value.Icon;
            cacheIconRect = data.Result[ 0 ].Value.IconSubrect;

            var ingreds = new List<IngredientMatcher>();
            foreach (var ingred in data.Ingredients)
                ingreds.Add(new DGAIngredientMatcher(ingred));
            cacheIngreds = ingreds.ToArray();
        }

        public override string Description => data.Description + "\n\nMod: " + data.parent.smapiPack.Manifest.Name;

        public override Texture2D IconTexture => cacheIconTex;

        public override Rectangle? IconSubrect => cacheIconRect;

        public override IngredientMatcher[] Ingredients => cacheIngreds;

        public override Item CreateResult()
        {
            // TODO: Random based on game seed and day
            return data.Result.Choose().Create();
        }
    }
}

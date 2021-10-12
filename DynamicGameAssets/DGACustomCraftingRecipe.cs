using System.Collections.Generic;
using DynamicGameAssets.Framework;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets
{
    public class DGACustomCraftingRecipe : CustomCraftingRecipe
    {
        private class DGAIngredientMatcher : IngredientMatcher
        {
            private readonly CraftingRecipePackData.IngredientAbstraction ingred;

            private readonly string cacheName;
            private readonly Texture2D cacheIconTex;
            private readonly Rectangle cacheIconRect;

            public DGAIngredientMatcher(CraftingRecipePackData.IngredientAbstraction theIngred)
            {
                this.ingred = theIngred;
                this.cacheName = this.ingred.NameOverride ?? this.ingred.Create().DisplayName;
                this.cacheIconTex = this.ingred.Icon;
                this.cacheIconRect = this.ingred.IconSubrect;
            }

            public override string DispayName => this.cacheName;

            public override Texture2D IconTexture => this.cacheIconTex;

            public override Rectangle? IconSubrect => this.cacheIconRect;

            public override int Quantity => this.ingred.Quantity;

            public override int GetAmountInList(IList<Item> items)
            {
                int ret = 0;
                foreach (var item in items)
                {
                    if (this.ItemMatches(item))
                        ret += item.Stack;
                }

                return ret;
            }

            public override void Consume(IList<Chest> additionalIngredients)
            {
                int left = this.Quantity;
                for (int i = Game1.player.Items.Count - 1; i >= 0; --i)
                {
                    var item = Game1.player.Items[i];
                    if (this.ItemMatches(item))
                    {
                        if (item.Stack <= left)
                            Game1.player.Items[i] = null;
                        else
                            item.Stack -= left;

                        left -= item.Stack;

                        if (left <= 0)
                            break;
                    }
                }

                if (left > 0 && additionalIngredients != null)
                {
                    foreach (var chest in additionalIngredients)
                    {
                        bool removed = false;
                        for (int i = chest.items.Count - 1; i >= 0; --i)
                        {
                            var item = chest.items[i];
                            if (this.ItemMatches(item))
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
                return this.ingred.Matches(item);
            }
        }

        internal CraftingRecipePackData data;

        private Texture2D cacheIconTex;
        private Rectangle cacheIconRect;
        private IngredientMatcher[] cacheIngreds;

        public DGACustomCraftingRecipe(CraftingRecipePackData theData)
        {
            this.data = theData;
            this.Refresh();
        }

        public void Refresh()
        {
            this.cacheIconTex = this.data.Result[0].Value.Icon;
            this.cacheIconRect = this.data.Result[0].Value.IconSubrect;

            var ingreds = new List<IngredientMatcher>();
            foreach (var ingred in this.data.Ingredients)
                ingreds.Add(new DGAIngredientMatcher(ingred));
            this.cacheIngreds = ingreds.ToArray();
        }

        public override string Name => this.data.Name;
        public override string Description => this.data.Description + $"\n\n{I18n.ItemTooltip_AddedByMod(this.data.pack.smapiPack.Manifest.Name)}";

        public override Texture2D IconTexture => this.cacheIconTex;

        public override Rectangle? IconSubrect => this.cacheIconRect;

        public override IngredientMatcher[] Ingredients => this.cacheIngreds;

        public override Item CreateResult()
        {
            // TODO: Random based on game seed and day?
            return this.data.Result.Choose().Create();
        }
    }
}

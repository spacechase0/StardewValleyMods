using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Inventories;

namespace SpaceCore.VanillaAssetExpansion
{
    public class VAECraftingRecipe
    {
        public string ProductQualifiedId { get; set; }
        public int ProductAmount { get; set; }

        public class IngredientData
        {
            public enum IngredientType
            {
                Item,
                ContextTag,
            }

            public IngredientType Type { get; set; }
            public string Value { get; set; }
            public int Amount { get; set; } = 1;

            public bool ContextTagsRequireAll { get; set; } = false;

            public string OverrideText { get; set; }
            public string OverrideTexturePath { get; set; }
            public Rectangle? OverrideTextureRect { get; set; }
        }
        public List<IngredientData> Ingredients { get; set; } = new();
    }


    public class VAECustomCraftingIngredientMatcher : CustomCraftingRecipe.IngredientMatcher
    {
        private readonly VAECraftingRecipe.IngredientData data;

        public VAECustomCraftingIngredientMatcher(VAECraftingRecipe.IngredientData data)
        {
            this.data = data;
        }

        public VAECraftingRecipe.IngredientData Data => data;

        public override string DispayName => data.OverrideText ?? ItemRegistry.GetDataOrErrorItem(data.Value).DisplayName;

        public override Texture2D IconTexture => data.OverrideTexturePath != null ? Game1.content.Load<Texture2D>(data.OverrideTexturePath) : ItemRegistry.GetDataOrErrorItem(data.Value).GetTexture();

        public override Rectangle? IconSubrect => data.OverrideTextureRect ?? ItemRegistry.GetDataOrErrorItem(data.Value).GetSourceRect();

        public override int Quantity => data.Amount;

        public override void Consume(IList<IInventory> additionalIngredients)
        {
            int left = Quantity;
            for (int i = Game1.player.MaxItems - 1; i >= 0; --i)
            {
                var item = Game1.player.Items[i];
                if (Matches(item))
                {
                    if (item.Stack <= left)
                        Game1.player.Items[i] = null;
                    else
                        item.Stack -= left;

                    left -= item.Stack;

                    if (left <= 0)
                        return;
                }
            }

            if (additionalIngredients != null)
            {
                foreach (var chest in additionalIngredients)
                {
                    bool removed = false;
                    for (int i = chest.Count - 1; i >= 0; --i)
                    {
                        var item = chest[i];
                        if (Matches(item))
                        {
                            if (item.Stack <= left)
                            {
                                removed = true;
                                chest[i] = null;
                            }
                            else
                                item.Stack -= left;

                            left -= item.Stack;

                            if (removed)
                                chest.RemoveEmptySlots();
                            if (left <= 0)
                                return;
                        }
                    }
                }
            }
        }

        public override int GetAmountInList(IList<Item> items)
        {
            return items.Sum(i => Matches(i) ? i.Stack : 0);
        }

        private bool Matches(Item i)
        {
            if (i == null)
                return false;

            switch (data.Type)
            {
                case VAECraftingRecipe.IngredientData.IngredientType.Item:
                    return i.QualifiedItemId == data.Value;
                case VAECraftingRecipe.IngredientData.IngredientType.ContextTag:
                    var tags = data.Value.Split(',').Select(s => s.Trim());
                    return data.ContextTagsRequireAll ? tags.All(s => i.HasContextTag(s)) : tags.Any(s => i.HasContextTag(s));
            }

            return false;
        }
    }

    public class VAECustomCraftingRecipe : CustomCraftingRecipe
    {
        private readonly bool cooking;
        private readonly string id;
        private readonly VAECraftingRecipe data;
        private readonly IngredientMatcher[] ingreds;

        public VAECustomCraftingRecipe(bool cooking, string id, VAECraftingRecipe data)
        {
            this.cooking = cooking;
            this.id = id;
            this.data = data;
            this.ingreds = data.Ingredients.Select(i => new VAECustomCraftingIngredientMatcher(i)).ToArray();
        }

        public VAECraftingRecipe Data => data;

        public override string Description
        {
            get
            {
                Dictionary<string, string> dict = CraftingRecipe.craftingRecipes;
                int ind = CraftingRecipe.index_craftingDisplayName;
                if (cooking)
                {
                    dict = CraftingRecipe.cookingRecipes;
                    ind = CraftingRecipe.index_cookingDisplayName;
                }

                string[] split = dict[id].Split('/');
                if (ind < split.Length)
                    return split[ind];

                // Why are we using displayname here? I dunno, the old code returned the display name...
                return ItemRegistry.GetDataOrErrorItem(data.ProductQualifiedId).DisplayName;
            }
        }

        public override Texture2D IconTexture => ItemRegistry.GetDataOrErrorItem(data.ProductQualifiedId).GetTexture();

        public override Rectangle? IconSubrect => ItemRegistry.GetDataOrErrorItem(data.ProductQualifiedId).GetSourceRect();

        public override IngredientMatcher[] Ingredients => ingreds;

        public override Item CreateResult()
        {
            return ItemRegistry.Create(data.ProductQualifiedId, data.ProductAmount, allowNull: false);
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Objects;

namespace SpaceCore
{
    public abstract class CustomCraftingRecipe
    {
        // Note: Custom recipes still need to have a fake entry in their corresponding data file.
        // Example for normal crafting recipe: ("Test Recipe", "0 1//0 1/false//Test Recipe")
        public static readonly Dictionary<string, CustomCraftingRecipe> CraftingRecipes = new Dictionary<string, CustomCraftingRecipe>();
        public static readonly Dictionary<string, CustomCraftingRecipe> CookingRecipes = new Dictionary<string, CustomCraftingRecipe>();

        public abstract class IngredientMatcher
        {
            public virtual string DisplayName => DispayName;

            [Obsolete("this will be removed eventually, override DisplayName instead")]
            public abstract string DispayName { get; }
            
            public abstract Texture2D IconTexture { get; }
            public abstract Rectangle? IconSubrect { get; }

            public abstract int Quantity { get; }

            public abstract int GetAmountInList(IList<Item> items);

            public int HasEnoughFor(IList<Chest> additionalIngredients)
            {
                List<Item> items = new List<Item>();
                items.AddRange(Game1.player.Items);
                foreach (var chest in additionalIngredients)
                    items.AddRange(chest.Items);

                return this.GetAmountInList(items) / this.Quantity;
            }

            public abstract void Consume(IList<IInventory> additionalIngredients);

            public abstract bool Matches(Item item);
        }

        public class ItemIngredientMatcher : IngredientMatcher
        {
            private readonly Item _item;
            private readonly string _itemId;
            private readonly int _quantity;

            public ItemIngredientMatcher(string query, int quantity)
            {
                this._itemId = query;
                this._quantity = quantity;

                this._item = ItemRegistry.Create(this._itemId);
            }

            public override string DispayName
            {
                get
                {
                    if (int.TryParse(this._itemId, out int i ) && i < 0)
                    {
                        return i switch
                        {
                            -1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.568"),
                            -2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.569"),
                            -3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570"),
                            -4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.571"),
                            -5 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572"),
                            -6 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573"),
                            -777 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.574"),
                            _ => "???",
                        };
                    }
                    return _item.DisplayName;
                }
            }

            public override Texture2D IconTexture => ItemRegistry.GetDataOrErrorItem(_item.QualifiedItemId).GetTexture();

            public override Rectangle? IconSubrect => ItemRegistry.GetDataOrErrorItem(_item.QualifiedItemId).GetSourceRect();

            public override int Quantity => this._quantity;

            public override int GetAmountInList(IList<Item> items)
            {
                int ret = 0;
                foreach ( var item in items )
                {
                    if (this.Matches(item))
                        ret += item.Stack;
                }

                return ret;
            }

            public override void Consume(IList<IInventory> additionalIngredients)
            {
                int left = this._quantity;
                for ( int i = Game1.player.Items.Count - 1; i >= 0; --i )
                {
                    var item = Game1.player.Items[i];
                    if (this.Matches(item))
                    {
                        int amt = Math.Min(left, item.Stack);
                        left -= amt;
                        item.Stack -= amt;

                        if ( item.Stack <= 0 )
                            Game1.player.Items[i] = null;
                        if (left <= 0)
                            break;
                    }
                }

                if ( left > 0 )
                {
                    foreach ( var chest in additionalIngredients )
                    {
                        bool removed = false;
                        for (int i = chest.Count - 1; i >= 0; --i)
                        {
                            var item = chest[i];
                            if (this.Matches(item))
                            {
                                int amt = Math.Min(left, item.Stack);
                                left -= amt;
                                item.Stack -= amt;

                                if (item.Stack <= 0)
                                {
                                    removed = true;
                                    chest[i] = null;
                                }
                                if (left <= 0)
                                    break;
                            }
                        }

                        if (removed)
                            chest.RemoveEmptySlots();
                        if (left <= 0)
                            break;
                    }
                }
            }

            public override bool Matches(Item item)
            {
                if (item == null)
                    return false;

                if (item is StardewValley.Object o && this._itemId.StartsWith("-"))
                {
                    return o.Category == int.Parse(this._itemId);
                }
                return item.canStackWith(this._item);
            }
        }

        public virtual string Name { get; } = null;
        public abstract string Description { get; }

        public abstract Texture2D IconTexture { get; }
        public abstract Rectangle? IconSubrect { get; }

        public abstract IngredientMatcher[] Ingredients { get; }

        public abstract Item CreateResult();
    }

    public abstract class CustomForgeRecipe
    {
        public abstract class IngredientMatcher
        {
            public abstract bool HasEnoughFor( Item item );

            public abstract void Consume( ref Item item );
        }

        public static List<CustomForgeRecipe> Recipes { get; set; } = new List<CustomForgeRecipe>();

        public abstract IngredientMatcher BaseItem { get; }
        public abstract IngredientMatcher IngredientItem { get; }
        public abstract int CinderShardCost { get; }

        public abstract Item CreateResult( Item baseItem, Item ingredItem );
    }
 }

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
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
                    items.AddRange(chest.items);

                return this.GetAmountInList(items) / this.Quantity;
            }

            public abstract void Consume(IList<Chest> additionalIngredients);
        }

        public class ObjectIngredientMatcher : IngredientMatcher
        {
            private readonly int objectIndex;
            private readonly int qty;

            public ObjectIngredientMatcher(int index, int quantity )
            {
                this.objectIndex = index;
                this.qty = quantity;
            }

            public override string DispayName
            {
                get
                {
                    if (this.objectIndex < 0)
                    {
                        return this.objectIndex switch
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
                    string retString = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.575");
                    if (Game1.objectInformation.ContainsKey(this.objectIndex))
                    {
                        retString = Game1.objectInformation[this.objectIndex].Split('/')[4];
                    }
                    return retString;
                }
            }

            public override Texture2D IconTexture => Game1.objectSpriteSheet;

            public override Rectangle? IconSubrect => Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.objectIndex, 16, 16);

            public override int Quantity => this.Quantity;

            public override int GetAmountInList(IList<Item> items)
            {
                int ret = 0;
                foreach ( var item in items )
                {
                    if (this.ItemMatches(item))
                        ret += item.Stack;
                }

                return ret;
            }

            public override void Consume(IList<Chest> additionalIngredients)
            {
                int left = this.qty;
                for ( int i = Game1.player.Items.Count - 1; i >= 0; --i )
                {
                    var item = Game1.player.Items[i];
                    if (this.ItemMatches( item ) )
                    {
                        int amt = Math.Min(left, item.Stack);
                        left -= amt;
                        item.Stack -= amt;

                        if ( item.Stack <= 0 )
                            Game1.player.items[i] = null;
                        if (left <= 0)
                            break;
                    }
                }

                if ( left > 0 )
                {
                    foreach ( var chest in additionalIngredients )
                    {
                        bool removed = false;
                        for (int i = chest.items.Count - 1; i >= 0; --i)
                        {
                            var item = chest.items[i];
                            if (this.ItemMatches( item ) )
                            {
                                int amt = Math.Min(left, item.Stack);
                                left -= amt;
                                item.Stack -= amt;

                                if (item.Stack <= 0)
                                {
                                    removed = true;
                                    chest.items[i] = null;
                                }
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
                return item is StardewValley.Object obj && !obj.bigCraftable.Value && (obj.ParentSheetIndex == this.objectIndex || obj.Category == this.objectIndex);
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

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;

namespace SpaceCore.Framework
{
    public class CustomCraftingRecipe : CraftingRecipe
    {
        internal readonly global::SpaceCore.CustomCraftingRecipe recipe;

        public CustomCraftingRecipe(string name, bool isCooking, global::SpaceCore.CustomCraftingRecipe recipeOverride)
            : base(name, isCooking)
        {
            this.recipe = recipeOverride;
            if (this.recipe.Name != null)
                this.DisplayName = this.recipe.Name;

            if (this.recipe.Description == null || this.recipe.Description.Length > 0)
                this.description = this.recipe.Description;
        }

        public override Item createItem()
        {
            var ret = this.recipe.CreateResult();
            this.numberProducedPerCraft = ret.Stack;
            return ret;
        }

        public override bool doesFarmerHaveIngredientsInInventory(IList<Item> extraToCheck = null)
        {
            if (extraToCheck == null)
                extraToCheck = new List<Item>();

            foreach (var ingred in this.recipe.Ingredients)
            {
                if (ingred.GetAmountInList(Game1.player.Items) + ingred.GetAmountInList(extraToCheck) < ingred.Quantity)
                    return false;
            }

            return true;
        }

        public override void drawMenuView(SpriteBatch b, int x, int y, float layerDepth = 0.88F, bool shadow = true)
        {
            // TODO: Allow 2-tall icons, like big craftables
            Utility.drawWithShadow(b, this.recipe.IconTexture, new Vector2(x, y), this.recipe.IconSubrect ?? new Rectangle(0, 0, this.recipe.IconTexture.Width, this.recipe.IconTexture.Height), Color.White, 0f, Vector2.Zero, 4f, flipped: false, layerDepth);
        }

        public override void drawRecipeDescription(SpriteBatch b, Vector2 position, int width, IList<Item> additional_crafting_items)
        {
            int lineExpansion = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 8 : 0);
            b.Draw(Game1.staminaRect, new Rectangle((int)(position.X + 8f), (int)(position.Y + 32f + Game1.smallFont.MeasureString("Ing!").Y) - 4 - 2 - (int)((float)lineExpansion * 1.5f), width - 32, 2), Game1.textColor * 0.35f);
            Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.567"), Game1.smallFont, position + new Vector2(8f, 28f), Game1.textColor * 0.75f);
            for (int i = 0; i < this.recipe.Ingredients.Length; i++)
            {
                var ingred = this.recipe.Ingredients[i];
                int required_count = ingred.Quantity;
                int bag_count = ingred.GetAmountInList(Game1.player.Items);
                int containers_count = 0;
                required_count -= bag_count;
                if (additional_crafting_items != null)
                {
                    containers_count = ingred.GetAmountInList(additional_crafting_items);
                    if (required_count > 0)
                    {
                        required_count -= containers_count;
                    }
                }
                string ingredient_name_text = ingred.DispayName;
                Color drawColor = ((required_count <= 0) ? Game1.textColor : Color.Red);
                b.Draw(ingred.IconTexture, new Vector2(position.X, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4)), ingred.IconSubrect, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.86f);
                Utility.drawTinyDigits(ingred.Quantity, b, new Vector2(position.X + 32f - Game1.tinyFont.MeasureString(ingred.Quantity.ToString() ?? "").X, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 21f), 2f, 0.87f, Color.AntiqueWhite);
                Vector2 text_draw_position = new Vector2(position.X + 32f + 8f, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 4f);
                Utility.drawTextWithShadow(b, ingredient_name_text, Game1.smallFont, text_draw_position, drawColor);
                if (Game1.options.showAdvancedCraftingInformation)
                {
                    text_draw_position.X = position.X + (float)width - 40f;
                    b.Draw(Game1.mouseCursors, new Rectangle((int)text_draw_position.X, (int)text_draw_position.Y + 2, 22, 26), new Rectangle(268, 1436, 11, 13), Color.White);
                    Utility.drawTextWithShadow(b, (bag_count + containers_count).ToString() ?? "", Game1.smallFont, text_draw_position - new Vector2(Game1.smallFont.MeasureString(bag_count + containers_count + " ").X, 0f), drawColor);
                }
            }
            b.Draw(Game1.staminaRect, new Rectangle((int)position.X + 8, (int)position.Y + lineExpansion + 64 + 4 + this.recipe.Ingredients.Length * 36, width - 32, 2), Game1.textColor * 0.35f);
            Utility.drawTextWithShadow(b, Game1.parseText(this.description, Game1.smallFont, width - 8), Game1.smallFont, position + new Vector2(0f, 76 + this.recipe.Ingredients.Length * 36 + lineExpansion), Game1.textColor * 0.75f);
        }

        public override int getCraftableCount(IList<Chest> additional_material_chests)
        {
            int amt = int.MaxValue;
            foreach (var ingred in this.recipe.Ingredients)
            {
                amt = Math.Min(amt, ingred.HasEnoughFor(additional_material_chests));
            }

            return amt;
        }

        public override string getCraftCountText()
        {
            // TODO - fix for cooking, probably need custom data saving? and another patch or two for when it increments
            return base.getCraftCountText();
        }

        public override int getNumberOfIngredients()
        {
            return this.recipe.Ingredients.Length;
        }
    }
}

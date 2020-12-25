using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceShared.APIs
{
    public interface CookingSkillAPI
    {
        /// <summary>
        /// Modify a cooked item based on the player's cooking skill.
        /// Returns if ingredients should be consumed or not.
        /// </summary>
        /// <param name="recipe">The crafting recipe.</param>
        /// <param name="craftedItem">The crafted item from the recipe. Nothing is changed if the recipe isn't cooking.</param>
        /// <param name="additionalIngredients">The additional places to draw ingredients from.</param>
        /// <returns>If ingredients should be consumed or not.</returns>
        bool ModifyCookedItem( CraftingRecipe recipe, Item craftedItem, List<Chest> additionalIngredients );
    }
}

using System;
using System.Collections.Generic;
using StardewValley;

namespace CookingSkill
{
    /// <summary>
    /// Api for cooking skill.
    /// </summary>
    public class CookingSkillAPI
    {
        /// <summary>
        /// This method will set the instance of the function that is used for determining the fridge items.
        /// </summary>
        /// <param name="func">The new function</param>
        /// <returns>The old function</returns>
        public Func<IList<Item>> setFridgeFunction(Func<IList<Item>> func)
        {
            Func<IList<Item>> old = NewCraftingPage.fridge;
            NewCraftingPage.fridge = func;
            return old;
        }
    }
}
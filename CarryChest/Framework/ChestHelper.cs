using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace CarryChest.Framework
{
    /// <summary>Provides utility methods for chest carrying.</summary>
    internal static class ChestHelper
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get whether an item is a chest that can be carried while full.</summary>
        /// <param name="item">The item to check.</param>
        public static bool IsSupported(ISalable item)
        {
            // We're checking the `.ParentSheetIndex` instead of `is Chest` because when you break a chest
            // and pick it up it isn't a chest instance, it's just an object with the chest index.
            return
                item is SObject obj
                && obj.bigCraftable.Value
                && (obj.ParentSheetIndex is 130 or 232) // chest or stone chest
                && (obj is not Chest chest || chest.playerChest.Value);
        }
    }
}

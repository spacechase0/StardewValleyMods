using SObject = StardewValley.Object;

namespace CookingSkill.Framework
{
    internal class ConsumedItem
    {
        public SObject Item { get; }
        public int Amount { get; }

        public ConsumedItem(SObject item)
        {
            this.Item = item;
            this.Amount = this.Item.Stack;
        }
    }
}

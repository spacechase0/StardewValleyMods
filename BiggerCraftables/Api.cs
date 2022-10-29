using System.Linq;
using BiggerCraftables.Framework;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace BiggerCraftables
{
    public interface IApi
    {
        bool IsBiggerCraftable(SObject obj);
        Vector2 GetBaseCraftable(GameLocation loc, Vector2 pos);
    }

    public class Api : IApi
    {
        public bool IsBiggerCraftable(SObject obj)
        {
            if (!obj.bigCraftable.Value)
                return false;

            return Mod.Entries.ContainsKey(obj.Name);
        }

        public Vector2 GetBaseCraftable(GameLocation loc, Vector2 pos)
        {
            if (!loc.Objects.TryGetValue(pos, out SObject obj))
                return new Vector2(-1, -1);

            if (!this.IsBiggerCraftable(obj))
                return new Vector2(-1, -1);

            var entry = Mod.Entries[obj.Name];
            int ind = obj.GetBiggerIndex();

            int relPosX = ind % entry.Width, relPosY = entry.Length - 1 - ind / entry.Width;
            Vector2 basePos = new(pos.X - relPosX, pos.Y - relPosY);
            return basePos;
        }
    }
}

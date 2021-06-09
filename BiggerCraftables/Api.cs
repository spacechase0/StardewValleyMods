using System.Linq;
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

            return Mod.entries.FirstOrDefault(e => e.Name == obj.Name) == null ? false : true;
        }

        public Vector2 GetBaseCraftable(GameLocation loc, Vector2 pos)
        {
            if (!loc.Objects.ContainsKey(pos))
                return new Vector2(-1, -1);

            var obj = loc.Objects[pos];
            if (!this.IsBiggerCraftable(obj))
                return new Vector2(-1, -1);

            var entry = Mod.entries.First(e => e.Name == obj.Name);
            int ind = obj.GetBiggerIndex();

            int relPosX = ind % entry.Width, relPosY = entry.Length - 1 - ind / entry.Width;
            Vector2 basePos = new Vector2(pos.X - relPosX, pos.Y - relPosY);
            return basePos;
        }
    }
}

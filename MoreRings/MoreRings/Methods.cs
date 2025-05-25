using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace MoreRings
{
    public partial class ModEntry
    {

        private static Item? LastItem;
        #region ModEntry
        public static int CountRingsEquipped(string id)
        {
            int count = (Game1.player.leftRing.Value?.GetEffectsOfRingMultiplier(id) ?? 0) + (Game1.player.rightRing.Value?.GetEffectsOfRingMultiplier(id) ?? 0);

            return count;
        }
        public static bool HasRingEquipped(string id)
        {
            return CountRingsEquipped(id) > 0;
        }
        #endregion
        #region CropPatcher
        public static Debris Game1_createItemDebris(Item item, Vector2 origin, int direction, GameLocation location, int groundLevel = -1, bool flopFish = false)
        {
            ModifyCropQuality(item);

            return Game1.createItemDebris(item, origin, direction, location, groundLevel, flopFish);
        }
        public static bool Farmer_addItemToInventoryBool(Farmer farmer, Item item, bool makeActiveObject)
        {
            ModifyCropQuality(item);

            return farmer.addItemToInventoryBool(item, makeActiveObject);
        }
        private static void ModifyCropQuality(Item item)
        {
            if (item is not Object obj || object.ReferenceEquals(item, LastItem)) return;

            LastItem = item;
            if (Game1.random.NextDouble() < CountRingsEquipped("Quality+_Ring") * Config.QualityRing_ChancePerRing)
            {
                obj.Quality = obj.Quality switch
                {
                    Object.lowQuality => Object.medQuality,
                    Object.medQuality => Object.highQuality,
                    Object.highQuality => Object.bestQuality,
                    _ => obj.Quality
                };
            }
        }
        #endregion
        #region Game1Patcher
        private static int Utility_withinRadiusOfPlayer()
        {
            var tool = Game1.player.CurrentTool;

            if (tool is Hoe or Pickaxe or WateringCan or Axe)
            {
                return HasRingEquipped("Ring_of_Far_Reaching") ? Config.RingOfFarReaching_TileDistance : 1;
            }
            else return 1;
        }
        #endregion
    }
}

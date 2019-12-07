using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SpaceCore.Overrides
{
    public class HoeDirtWinterFix
    {
        public static void dayUpdate(HoeDirt hoeDirt, GameLocation environment, Vector2 tileLocation)
        {
            if (hoeDirt.crop != null)
            {
                hoeDirt.crop.newDay((int)((NetFieldBase<int, NetInt>)hoeDirt.state), (int)((NetFieldBase<int, NetInt>)hoeDirt.fertilizer), (int)tileLocation.X, (int)tileLocation.Y, environment);
                /*if ((bool)((NetFieldBase<bool, NetBool>)environment.isOutdoors) && Game1.currentSeason.Equals("winter") && (this.crop != null && !this.crop.isWildSeedCrop()) && !environment.IsGreenhouse)
                    this.destroyCrop(tileLocation, false, environment);
                */
            }
            if ((!hoeDirt.hasPaddyCrop() || !hoeDirt.paddyWaterCheck(environment, tileLocation)) && ((int)((NetFieldBase<int, NetInt>)hoeDirt.fertilizer) != 370 || Game1.random.NextDouble() >= 0.33) && ((int)((NetFieldBase<int, NetInt>)hoeDirt.fertilizer) != 371 || Game1.random.NextDouble() >= 0.66))
                hoeDirt.state.Value = 0;
            if (!environment.IsGreenhouse)
                return;
            hoeDirt.isGreenhouseDirt.Value = true;
            SpaceCore.instance.Helper.Reflection.GetField<NetColor>(hoeDirt, "c").GetValue().Value = Color.White;
        }

        // TODO: Make this do IL hooking instead of pre + no execute original
        public static bool Prefix(HoeDirt __instance, GameLocation environment, Vector2 tileLocation)
        {
            dayUpdate(__instance, environment, tileLocation);
            return false;
        }
    }
}

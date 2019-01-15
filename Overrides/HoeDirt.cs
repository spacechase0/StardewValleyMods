using Microsoft.Xna.Framework;
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
                hoeDirt.crop.newDay(hoeDirt.state, hoeDirt.fertilizer, (int)tileLocation.X, (int)tileLocation.Y, environment);
                /*if (environment.isOutdoors && Game1.currentSeason.Equals("winter") && this.crop != null && !this.crop.isWildSeedCrop())
                {
                    this.destroyCrop(tileLocation, false, environment);
                }*/
            }
            if ((hoeDirt.fertilizer != 370 || Game1.random.NextDouble() >= 0.33) && (hoeDirt.fertilizer != 371 || Game1.random.NextDouble() >= 0.66))
            {
                hoeDirt.state.Value = 0;
            }
        }

        // TODO: Make this do IL hooking instead of pre + no execute original
        public static bool Prefix(HoeDirt __instance, GameLocation environment, Vector2 tileLocation)
        {
            dayUpdate(__instance, environment, tileLocation);
            return false;
        }
    }
}

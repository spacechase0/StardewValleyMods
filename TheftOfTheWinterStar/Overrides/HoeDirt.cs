using Harmony;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace TheftOfTheWinterStar.Overrides
{
    public static class SeasonalDelimiterAllowPlantingHook1
    {
        public static bool Prefix(HoeDirt __instance, int objectIndex, int tileX, int tileY, bool isFertilizer, ref bool __result)
        {
            if (isFertilizer)
                return true;

            int seasonalDelimiter = Mod.ja.GetBigCraftableId("Tempus Globe");

            var loc = Game1.currentLocation;
            for (int ix = -2; ix <= 2; ++ix)
            {
                for (int iy = -2; iy <= 2; ++iy)
                {
                    var key = new Vector2(tileX + ix, tileY + iy);
                    if ( loc.objects.ContainsKey(key) )
                    {
                        var obj = loc.objects[key];
                        if (obj.bigCraftable.Value && obj.ParentSheetIndex == seasonalDelimiter)
                        {
                            if (__instance.crop == null)
                            {
                                Crop crop = new Crop(objectIndex, tileX, tileY);
                                __result = !crop.raisedSeeds || !Utility.doesRectangleIntersectTile(Game1.player.GetBoundingBox(), tileX, tileY);
                            }
                            else
                                __result = false;
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }

    public static class SeasonalDelimiterAllowPlantingHook2
    {
        public static bool Prefix(HoeDirt __instance, int index, int tileX, int tileY, Farmer who, bool isFertilizer, GameLocation location, ref bool __result)
        {
            if (isFertilizer)
                return true;

            int seasonalDelimiter = Mod.ja.GetBigCraftableId("Tempus Globe");

            bool foundDelimiter = false;
            for (int ix = -2; ix <= 2; ++ix)
            {
                for (int iy = -2; iy <= 2; ++iy)
                {
                    var key = new Vector2(tileX + ix, tileY + iy);
                    if (location.objects.ContainsKey(key))
                    {
                        var obj = location.objects[key];
                        if (obj.bigCraftable.Value && obj.ParentSheetIndex == seasonalDelimiter)
                        {
                            foundDelimiter = true;
                        }
                    }
                }
            }

            // Now for the original method
            Crop c = new Crop(index, tileX, tileY);
            if ( c.seasonsToGrowIn.Count == 0 )
            {
                return false;
            }
            if ( !who.currentLocation.isFarm && !who.currentLocation.IsGreenhouse && !who.currentLocation.CanPlantSeedsHere( index, tileX, tileY ) && who.currentLocation.IsOutdoors )
            {
                Game1.showRedMessage( Game1.content.LoadString( "Strings\\StringsFromCSFiles:HoeDirt.cs.13919" ) );
                return false;
            }
            if ( foundDelimiter || !who.currentLocation.isOutdoors || who.currentLocation.IsGreenhouse || c.seasonsToGrowIn.Contains( location.GetSeasonForLocation() ) || who.currentLocation.SeedsIgnoreSeasonsHere() )
            {
                __instance.crop = c;
                if ( ( bool ) c.raisedSeeds )
                {
                    location.playSound( "stoneStep" );
                }
                location.playSound( "dirtyHit" );
                Game1.stats.SeedsSown++;
                AccessTools.Method( __instance.GetType(), "applySpeedIncreases" ).Invoke( __instance, new object[] { who } );
                __instance.nearWaterForPaddy.Value = -1;
                if ( __instance.hasPaddyCrop() && __instance.paddyWaterCheck( location, new Vector2( tileX, tileY ) ) )
                {
                    __instance.state.Value = 1;
                    __instance.updateNeighbors( location, new Vector2( tileX, tileY ) );
                }
                __result = true;
                return false;
            }
            if ( c.seasonsToGrowIn.Count > 0 && !c.seasonsToGrowIn.Contains( location.GetSeasonForLocation() ) )
            {
                Game1.showRedMessage( Game1.content.LoadString( "Strings\\StringsFromCSFiles:HoeDirt.cs.13924" ) );
            }
            else
            {
                Game1.showRedMessage( Game1.content.LoadString( "Strings\\StringsFromCSFiles:HoeDirt.cs.13925" ) );
            }
            return false;
        }
    }
}

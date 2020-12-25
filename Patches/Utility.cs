using Harmony;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiggerCraftables.Patches
{
    [HarmonyPatch( typeof( Utility ), nameof( Utility.playerCanPlaceItemHere ) )]
    public static class UtilityPlacementPatche
    {
        public static bool Prefix( GameLocation location,
            Item item,
            int x,
            int y,
            Farmer f,
            ref bool __result)
        {
            if ( !( item is StardewValley.Object obj && obj.bigCraftable.Value ) )
                return true;
            var entry = Mod.entries.SingleOrDefault( cle => cle.Name == obj.Name );
            if ( entry == null )
                return true;

            if ( Utility.isPlacementForbiddenHere( location ) )
            {
                __result = false;
                return false;
            }
            if ( item == null || item is Tool || Game1.eventUp || ( bool ) f.bathingClothes || f.onBridge.Value )
            {
                __result = false;
                return false;
            }
            if ( Utility.isWithinTileWithLeeway( x, y, item, f ) || ( item is Wallpaper && location is DecoratableLocation ) || ( item is Furniture && location.CanPlaceThisFurnitureHere( item as Furniture ) ) )
            {
                Vector2 tileLocation = new Vector2(x / 64, y / 64);
                if ( item.canBePlacedHere( location, tileLocation ) )
                {
                    if ( !( ( StardewValley.Object ) item ).isPassable() )
                    {
                        foreach ( Farmer farmer in location.farmers )
                        {
                            if ( farmer.GetBoundingBox().Intersects( new Microsoft.Xna.Framework.Rectangle( ( int ) tileLocation.X * 64, ( int ) tileLocation.Y * 64, 64, 64 ) ) )
                            {
                                __result = false;
                                return false;
                            }
                        }
                    }
                    var itemCanBePlaced = Mod.instance.Helper.Reflection.GetMethod( typeof( Utility ), "itemCanBePlaced" );
                    if ( itemCanBePlaced.Invoke< bool >( location, tileLocation, item ) || Utility.isViableSeedSpot( location, tileLocation, item ) )
                    {
                        __result = true;
                        return false;
                    }
                }
            }

            __result = false;
            return false;
        }
    }
}

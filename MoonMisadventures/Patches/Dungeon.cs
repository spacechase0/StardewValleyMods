using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using MoonMisadventures.Game.Locations;
using StardewValley;
using StardewValley.Objects;

namespace MoonMisadventures.Patches
{
    [HarmonyPatch( typeof( Game1 ), nameof( Game1.getLocationFromNameInLocationsList ) )]
    public static class Game1FetchDungeonInstancePatch
    {
        public static bool Prefix( string name, bool isStructure, ref GameLocation __result )
        {
            if ( name.StartsWith( AsteroidsDungeon.BaseLocationName ) )
            {
                __result = AsteroidsDungeon.GetLevelInstance( name );
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( Game1 ), "UpdateLocations" )]
    public static class Game1UpdateDungeonLocationsPatch
    {
        public static void Postfix( GameTime time )
        {
            if ( Game1.menuUp && !Game1.IsMultiplayer )
            {
                return;
            }
            if ( Game1.IsClient )
            {
                return;
            }

            AsteroidsDungeon.UpdateLevels( time );
        }
    }

    [HarmonyPatch( typeof( Multiplayer ), nameof( Multiplayer.updateRoots ) )]
    public static class MultiplayerUpdateDungeonRootsPatch
    {
        public static void Postfix( Multiplayer __instance )
        {
            foreach ( var level in AsteroidsDungeon.activeLevels )
            {
                if ( level.Root.Value is not null )
                {
                    level.Root.Clock.InterpolationTicks = __instance.interpolationTicks();
                    __instance.updateRoot( level.Root );
                }
            }
        }
    }

    [HarmonyPatch( typeof( BreakableContainer ), nameof( BreakableContainer.releaseContents ) )]
    public static class BreakableContainerMoonLootPatch
    {
        public static bool Prefix( BreakableContainer __instance, GameLocation location, Farmer who )
        {
            if ( location is LunarLocation )
            {
                DoDrops( __instance, location, who );
                return false;
            }

            return true;
        }

        private static void DoDrops( BreakableContainer __instance, GameLocation location, Farmer who )
        {
            Random r = new Random((int)__instance.TileLocation.X + (int)__instance.TileLocation.Y * 10000 + (int)Game1.stats.DaysPlayed);
            int x = (int)__instance.TileLocation.X;
            int y = (int)__instance.TileLocation.Y;
            if ( r.NextDouble() < 0.2 )
                return;
            if ( Game1.random.NextDouble() <= 0.075 && Game1.player.team.SpecialOrderRuleActive( "DROP_QI_BEANS" ) )
                Game1.createMultipleObjectDebris( 890, x, y, r.Next( 1, 3 ), who.UniqueMultiplayerID, location );
            if ( r.NextDouble() < 0.01 )
            {
                Game1.createItemDebris( new DynamicGameAssets.Game.CustomObject( DynamicGameAssets.Mod.Find( ItemIds.SoulSapphire ) as ObjectPackData ), __instance.TileLocation * Game1.tileSize, 0, location );
            }
            if ( r.NextDouble() < 0.65 )
            {
                Item item = null;
                Item item2 = null;
                switch ( r.Next( 5 ) )
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        //item = new DynamicGameAssets.Game.CustomObject( DynamicGameAssets.Mod.Find( ItemIds.LunarWheat ) as ObjectPackData ) { Stack = 4 + r.Next( 13 ) };
                        item2 = new DynamicGameAssets.Game.CustomObject( DynamicGameAssets.Mod.Find( ItemIds.LunarWheatSeeds ) as ObjectPackData ) { Stack = 1 + r.Next( 6 ) };
                        break;
                }
                if ( item != null )
                    Game1.createItemDebris( item, __instance.TileLocation * Game1.tileSize, 0, location );
                if ( item2 != null && r.NextDouble() < 0.25 )
                    Game1.createItemDebris( item2, __instance.TileLocation * Game1.tileSize, 0, location );
            }
            else if ( r.NextDouble() < 0.75 )
            {
                Game1.createItemDebris( new DynamicGameAssets.Game.CustomObject( DynamicGameAssets.Mod.Find( ItemIds.MythiciteOre ) as ObjectPackData ) { Stack = 1 + r.Next( 4 ) }, __instance.TileLocation * Game1.tileSize, 0, location );
            }
        }
    }
}

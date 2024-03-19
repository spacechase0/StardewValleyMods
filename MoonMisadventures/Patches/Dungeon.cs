using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if ( Game1.activeClickableMenu != null && !Game1.IsMultiplayer )
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
                if ( level.Root != null )
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
        public static bool Prefix( BreakableContainer __instance, Farmer who )
        {
            if ( __instance.Location is LunarLocation )
            {
                DoDrops( __instance, who );
                return false;
            }

            return true;
        }

        private static void DoDrops( BreakableContainer __instance, Farmer who )
        {
            Random r = new Random((int)__instance.TileLocation.X + (int)__instance.TileLocation.Y * 10000 + (int)Game1.stats.DaysPlayed);
            int x = (int)__instance.TileLocation.X;
            int y = (int)__instance.TileLocation.Y;
            if ( r.NextDouble() < 0.2 )
                return;
            if ( Game1.random.NextDouble() <= 0.075 && Game1.player.team.SpecialOrderRuleActive( "DROP_QI_BEANS" ) )
                Game1.createMultipleObjectDebris( "890", x, y, r.Next( 1, 3 ), who.UniqueMultiplayerID, __instance.Location);
            if ( r.NextDouble() < 0.01 )
            {
                Game1.createItemDebris( new StardewValley.Object( ItemIds.SoulSapphire, 1 ), __instance.TileLocation * Game1.tileSize, 0, __instance.Location);
            }
            if ( r.NextDouble() < 0.65 )
            {
                Item item = null;
                Item item2 = null;
                switch ( r.Next( 5 ) )
                {
                    case 0:
                        item = new StardewValley.Object( ItemIds.Sunbloom, 4 + r.Next( 13 ) );
                        item2 = new StardewValley.Object( ItemIds.SunbloomSeeds, 1 + r.Next( 6 ) );
                        break;
                    case 1:
                        item = new StardewValley.Object( ItemIds.StarPetal, 4 + r.Next( 13 ) );
                        item2 = new StardewValley.Object( ItemIds.StarPetalSeeds, 1 + r.Next( 6 ) );
                        break;
                    case 2:
                        item = new StardewValley.Object( ItemIds.VoidBlossom, 4 + r.Next( 13 ) );
                        item2 = new StardewValley.Object( ItemIds.VoidBlossomSeeds, 1 + r.Next( 6 ) );
                        break;
                    case 3:
                        item = new StardewValley.Object( ItemIds.SoulSprout, 4 + r.Next( 13 ) );
                        item2 = new StardewValley.Object( ItemIds.SoulSproutSeeds, 1 + r.Next( 6 ) );
                        break;
                    case 4:
                        //item = new StardewValley.Object( ItemIds.LunarWheat, 4 + r.Next( 13 ) );
                        item2 = new StardewValley.Object( ItemIds.LunarWheatSeeds, 1 + r.Next( 6 ) );
                        break;
                }
                if ( item != null )
                    Game1.createItemDebris( item, __instance.TileLocation * Game1.tileSize, 0, __instance.Location);
                if ( item2 != null && r.NextDouble() < 0.25 )
                    Game1.createItemDebris( item2, __instance.TileLocation * Game1.tileSize, 0, __instance.Location);
            }
            else if ( r.NextDouble() < 0.75 )
            {
                Game1.createItemDebris( new StardewValley.Object( ItemIds.MythiciteOre, 1 + r.Next( 4 ) ), __instance.TileLocation * Game1.tileSize, 0, __instance.Location);
            }
        }
    }
}

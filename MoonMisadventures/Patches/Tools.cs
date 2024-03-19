using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace MoonMisadventures.Patches
{
    [HarmonyPatch( typeof( Tree ), nameof( Tree.performToolAction ) )]
    public static class TreeToolActionPatch
    {
        public static void Prefix( Tree __instance, Tool t, int explosion )
        {
            if ( t is Axe axe && axe.UpgradeLevel == 6 ) // Mythicite
            {
                if ( explosion <= 0 && __instance.health.Value > -99 )
                    __instance.health.Value = 0;
            }
        }
    }

    [HarmonyPatch( typeof( Pickaxe ), nameof( Pickaxe.DoFunction ) )]
    public static class PickaxeFunctionPatch
    {
        public static void Prefix( Pickaxe __instance, GameLocation location, int x, int y )
        {
            if ( __instance.UpgradeLevel == 6 ) // Mythicite
            {
                Vector2 v2 = new Vector2( x / Game1.tileSize, y / Game1.tileSize );
                if ( location.objects.ContainsKey( v2 ) )
                {
                    var obj = location.objects[ v2 ];
                    if ( obj.Name == "Stone" )
                        obj.MinutesUntilReady = 0;
                }
            }
        }
    }

    [HarmonyPatch( typeof( ResourceClump ), nameof( ResourceClump.performToolAction ) )]
    public static class ResourceClumpToolActionPatch
    {
        public static void Prefix( ResourceClump __instance, Tool t )
        {
            if ( t.UpgradeLevel != 6 ) // Mythicite
                return;

            int[] axeIds = new int[] { 600, 602 };
            int[] pickaxeIds = new int[] { 622, 672, 752, 754, 756, 758 };

            if ( t is Axe && axeIds.Contains( __instance.parentSheetIndex.Value ) ||
                 t is Pickaxe && pickaxeIds.Contains( __instance.parentSheetIndex.Value ) )
                __instance.health.Value = 0;
        }
    }

    [HarmonyPatch( typeof( GiantCrop ), nameof( GiantCrop.performToolAction ) )]
    public static class GiantCropToolActionPatch
    {
        public static void Prefix( GiantCrop __instance, Tool t )
        {
            if ( t is Axe axe && axe.UpgradeLevel == 6 ) // Mythicite
            {
                // Setting it to 1 lets the real method end up getting it to 0 and taking care of everything
                __instance.health.Value = 1;
            }
        }
    }

    [HarmonyPatch( typeof( Tool ), "tilesAffected" )]
    public static class ToolTilesAffectedPatch
    {
        public static bool Prefix( Vector2 tileLocation, ref int power, Farmer who, ref List< Vector2 > __result )
        {
            if ( power >= 5 ) // Radioactive or above
            {
                int rad = 2;
                int len = 5;
                if ( power >= 6 ) // Mythicite
                {
                    ++rad;
                    len += 2;
                }
                if ( power >= 7 ) // Mythicite + Reaching enchantment
                {
                    ++rad;
                    len += 2;
                }

                __result = new();

                Vector2 dir = Vector2.Zero;
                switch ( who.FacingDirection )
                {
                    case Game1.up: dir = new Vector2( 0, -1 ); break;
                    case Game1.right: dir = new Vector2( 1, 0 ); break;
                    case Game1.down: dir = new Vector2( 0, 1 ); break;
                    case Game1.left: dir = new Vector2( -1, 0 ); break;
                }
                Vector2 perp = new Vector2( dir.Y, dir.X );

                for ( int il = 0; il < len; ++il )
                {
                    for ( int ir = -rad; ir <= rad; ++ir )
                    {
                        __result.Add( tileLocation + dir * il + perp * ir );
                    }
                }

                // Goldenrevolver asked for this for their WIP mod
                ++power;

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( ReachingToolEnchantment ), nameof( ReachingToolEnchantment.CanApplyTo ) )]
    public static class ReachingEnchantCanApplyPatch
    {
        public static bool Prefix( Item item, ref bool __result )
        {
            if ( item is Tool t && ( item is WateringCan || item is Hoe ) && t.UpgradeLevel >= 4 )
            {
                __result = true;
                return false;
            }
            __result = false;
            return false;
        }
    }
}

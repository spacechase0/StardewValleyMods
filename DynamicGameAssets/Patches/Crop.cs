using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;

namespace DynamicGameAssets.Patches
{
    [HarmonyPatch( typeof( Crop ), nameof( Crop.ResetPhaseDays ) )]
    public static class CropResetPhaseDaysPatch
    {
        public static bool Prefix( Crop __instance )
        {
            if ( __instance is CustomCrop cc)
            {
                cc.ResetPhaseDays();
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( Crop ), nameof( Crop.harvest ) )]
    public static class CropHarvestPatch
    {
        public static bool Prefix( Crop __instance, int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester, ref bool __result  )
        {
            if ( __instance is CustomCrop cc )
            {
                __result = cc.Harvest( xTile, yTile, soil, junimoHarvester );
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( Crop ), nameof( Crop.draw ) )]
    public static class CropDrawPatch
    {
        public static bool Prefix( Crop __instance, SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation )
        {
            if ( __instance is CustomCrop cc )
            {
                cc.Draw( b, tileLocation, toTint, rotation );
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( Crop ), nameof( Crop.drawInMenu ) )]
    public static class CropDrawInMenuPatch
    {
        public static bool Prefix( Crop __instance, SpriteBatch b, Vector2 screenPosition, Color toTint, float rotation, float scale, float layerDepth )
        {
            if ( __instance is CustomCrop cc )
            {
                cc.DrawInMenu( b, screenPosition, toTint, rotation, scale, layerDepth );
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( Crop ), nameof( Crop.drawWithOffset ) )]
    public static class CropDrawWithOffsetPatch
    {
        public static bool Prefix( Crop __instance, SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation, Vector2 offset )
        {
            if ( __instance is CustomCrop cc )
            {
                cc.DrawWithOffset( b, tileLocation, toTint, rotation, offset );
                return false;
            }

            return true;
        }
    }
}

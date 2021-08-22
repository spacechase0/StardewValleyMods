using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;

namespace DynamicGameAssets.Patches
{
    [HarmonyPatch( typeof( FarmerRenderer ), "ApplyShoeColor" )]
    public static class FarmerRendererApplyShoesPatch
    {
        public static bool Prefix( FarmerRenderer __instance, string texture_name, Color[] pixels )
        {
            var this_shoes = Mod.instance.Helper.Reflection.GetField< NetInt >( __instance, "shoes" ).GetValue();

            if ( Mod.itemLookup.ContainsKey( this_shoes.Value ) )
            {
                Impl( Mod.Find( Mod.itemLookup[ this_shoes.Value ] ) as BootsPackData, __instance, texture_name, pixels );
                return false;
            }

            return true;
        }

        public static void Impl( BootsPackData data, FarmerRenderer __instance, string texture_name, Color[] pixels )
        {
            var this_shoes = Mod.instance.Helper.Reflection.GetField< NetInt >( __instance, "shoes" ).GetValue();
            var this__SwapColor = Mod.instance.Helper.Reflection.GetMethod( __instance, "_SwapColor" );

            var currTex = data.parent.GetTexture( data.FarmerColors, 4, 1 );

            int which = this_shoes.Value;
            Texture2D shoeColors = currTex.Texture;
            Color[] shoeColorsData = new Color[shoeColors.Width * shoeColors.Height];
            shoeColors.GetData( 0, currTex.Rect, shoeColorsData, 0, 4 );
            Color darkest = shoeColorsData[0];
            Color medium = shoeColorsData[1];
            Color lightest = shoeColorsData[2];
            Color lightest2 = shoeColorsData[3];
            this__SwapColor.Invoke( texture_name, pixels, 268, darkest );
            this__SwapColor.Invoke( texture_name, pixels, 269, medium );
            this__SwapColor.Invoke( texture_name, pixels, 270, lightest );
            this__SwapColor.Invoke( texture_name, pixels, 271, lightest2 );
        }
    }
}

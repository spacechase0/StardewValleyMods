using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
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
                FarmerRendererApplyShoesPatch.Impl( Mod.Find( Mod.itemLookup[ this_shoes.Value ] ) as BootsPackData, __instance, texture_name, pixels );
                return false;
            }

            return true;
        }

        public static void Impl( BootsPackData data, FarmerRenderer __instance, string texture_name, Color[] pixels )
        {
            var this_shoes = Mod.instance.Helper.Reflection.GetField< NetInt >( __instance, "shoes" ).GetValue();
            var this__SwapColor = Mod.instance.Helper.Reflection.GetMethod( __instance, "_SwapColor" );

            var currTex = data.pack.GetTexture( data.FarmerColors, 4, 1 );

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

    [HarmonyPatch( typeof( FarmerRenderer), nameof( FarmerRenderer.ApplySleeveColor ) )]
    public static class FarmerRendererApplySleeveColorPatch
    {
        public static bool Prefix( FarmerRenderer __instance, string texture_name, Color[] pixels, Farmer who )
        {
            if ( who.shirtItem.Value is CustomShirt cshirt )
            {
                var this_farmerTextureManager = Mod.instance.Helper.Reflection.GetField< LocalizedContentManager >( __instance, "farmerTextureManager" ).GetValue();
                var this_skin = Mod.instance.Helper.Reflection.GetField< NetInt >( __instance, "skin" ).GetValue();
                bool this__sickFrame = Mod.instance.Helper.Reflection.GetField< bool >( __instance, "_sickFrame" ).GetValue();
                var this_baseTexture = Mod.instance.Helper.Reflection.GetField< Texture2D >( __instance, "baseTexture" ).GetValue();
                var this__SwapColor = Mod.instance.Helper.Reflection.GetMethod( __instance, "_SwapColor" );

                Color[][] shirtData = new Color[ 4 ][]
                {
                    new Color[ 8 * 32 ],
                    new Color[ 8 * 32 ],
                    new Color[ 8 * 32 ],
                    new Color[ 8 * 32 ],
                };// new Color[FarmerRenderer.shirtsTexture.Bounds.Width * FarmerRenderer.shirtsTexture.Bounds.Height];
                foreach ( var colors in shirtData )
                {
                    for (int i = 0; i < colors.Length; i++)
                        colors[i] = Color.Transparent;
                }
                var maleNC = cshirt.Data.pack.GetTexture( cshirt.Data.TextureMale, 8, 32 );
                var maleC = cshirt.Data.TextureMaleColor == null ? null : cshirt.Data.pack.GetTexture( cshirt.Data.TextureMaleColor, 8, 32 );
                var femaleNC = cshirt.Data.TextureFemale == null ? maleNC : cshirt.Data.pack.GetTexture( cshirt.Data.TextureFemale, 8, 32 );
                var femaleC = cshirt.Data.TextureFemaleColor == null ? null :cshirt.Data.pack.GetTexture( cshirt.Data.TextureFemaleColor, 8, 32 );
                maleNC.Texture.GetData( 0, maleNC.Rect, shirtData[ 0 ], 0, 8 * 32 );
                maleC?.Texture?.GetData( 0, maleNC.Rect, shirtData[ 1 ], 0, 8 * 32 );
                femaleNC.Texture.GetData( 0, femaleNC.Rect, shirtData[ 2 ], 0, 8 * 32 );
                femaleC?.Texture?.GetData( 0, maleNC.Rect, shirtData[ 3 ], 0, 8 * 32 );
                //FarmerRenderer.shirtsTexture.GetData( shirtData );
                int index = who.IsMale ? 0 : 2; // __instance.ClampShirt(who.GetShirtIndex()) * 8 / 128 * 32 * FarmerRenderer.shirtsTexture.Bounds.Width + __instance.ClampShirt(who.GetShirtIndex()) * 8 % 128 + FarmerRenderer.shirtsTexture.Width * 4;
                int dye_index = index + 1;
                Color shirtSleeveColor = Color.White;
                if ( who.GetShirtExtraData().Contains( "Sleeveless" ) || index >= shirtData.Length )
                {
                    Texture2D skinColors = this_farmerTextureManager.Load<Texture2D>("Characters\\Farmer\\skinColors");
                    Color[] skinColorsData = new Color[skinColors.Width * skinColors.Height];
                    int skin_index = this_skin.Value;
                    if ( skin_index < 0 )
                    {
                        skin_index = skinColors.Height - 1;
                    }
                    if ( skin_index > skinColors.Height - 1 )
                    {
                        skin_index = 0;
                    }
                    skinColors.GetData( skinColorsData );
                    Color darkest = skinColorsData[skin_index * 3 % (skinColors.Height * 3)];
                    Color medium = skinColorsData[skin_index * 3 % (skinColors.Height * 3) + 1];
                    Color lightest = skinColorsData[skin_index * 3 % (skinColors.Height * 3) + 2];
                    if ( this__sickFrame )
                    {
                        darkest = pixels[ 260 + this_baseTexture.Width ];
                        medium = pixels[ 261 + this_baseTexture.Width ];
                        lightest = pixels[ 262 + this_baseTexture.Width ];
                    }
                    shirtSleeveColor = darkest;
                    this__SwapColor.Invoke( texture_name, pixels, 256, darkest );
                    this__SwapColor.Invoke( texture_name, pixels, 257, medium );
                    this__SwapColor.Invoke( texture_name, pixels, 258, lightest );
                    return false;
                }
                Color color = Utility.MakeCompletelyOpaque(who.GetShirtColor());
                shirtSleeveColor = shirtData[ dye_index ][ 32 ];
                Color clothes_color = color;
                if ( shirtSleeveColor.A < byte.MaxValue )
                {
                    shirtSleeveColor = shirtData[ index ][ 32 ];
                    clothes_color = Color.White;
                }
                shirtSleeveColor = Utility.MultiplyColor( shirtSleeveColor, clothes_color );
                this__SwapColor.Invoke( texture_name, pixels, 256, shirtSleeveColor );
                shirtSleeveColor = shirtData[ dye_index ][ 24 ];
                if ( shirtSleeveColor.A < byte.MaxValue )
                {
                    shirtSleeveColor = shirtData[ index ][ 24 ];
                    clothes_color = Color.White;
                }
                shirtSleeveColor = Utility.MultiplyColor( shirtSleeveColor, clothes_color );
                this__SwapColor.Invoke( texture_name, pixels, 257, shirtSleeveColor );
                shirtSleeveColor = shirtData[ dye_index ][ 16 ];
                if ( shirtSleeveColor.A < byte.MaxValue )
                {
                    shirtSleeveColor = shirtData[ index ][ 16 ];
                    clothes_color = Color.White;
                }
                shirtSleeveColor = Utility.MultiplyColor( shirtSleeveColor, clothes_color );
                this__SwapColor.Invoke( texture_name, pixels, 258, shirtSleeveColor );

                return false;
            }
            return true;
        }
    }

    [HarmonyPatch( typeof( FarmerRenderer ), nameof( FarmerRenderer.ClampShirt ) )]
    public static class FarmerRendererClampShirtPatch
    {
        public static bool Prefix( FarmerRenderer __instance, int shirt_value, ref int __result )
        {
            if ( Mod.itemLookup.ContainsKey( shirt_value ) || Mod.itemLookup.ContainsKey( shirt_value + 1 ) )
            {
                __result = shirt_value;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( FarmerRenderer ), nameof( FarmerRenderer.ClampPants ) )]
    public static class FarmerRendererClampPantsPatch
    {
        public static bool Prefix( FarmerRenderer __instance, int pants_value, ref int __result )
        {
            if ( Mod.itemLookup.ContainsKey( pants_value ) )
            {
                __result = pants_value;
                return false;
            }

            return true;
        }
    }
}

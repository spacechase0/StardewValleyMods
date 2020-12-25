using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreEnchantments.Enchantments;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreEnchantments.Harmony
{
    [HarmonyPatch(typeof(FishingRod), nameof(FishingRod.attachmentSlots))]
    public static class FishingRodAttachmentSlotsPatch
    {
        public static void Postfix(FishingRod __instance, ref int __result)
        {
            if ( __instance.hasEnchantmentOfType<MoreLuresEnchantment>() )
                ++__result;
        }
    }

    [HarmonyPatch(typeof(FishingRod), "calculateTimeUntilFishingBite")]
    public static class FishingRodCalculateBiteTimePatch
    {
        public static bool Prefix(FishingRod __instance, Vector2 bobberTile, bool isFirstCast, Farmer who, ref float __result )
        {
            if ( Game1.currentLocation.isTileBuildingFishable( ( int ) bobberTile.X, ( int ) bobberTile.Y ) && Game1.currentLocation is BuildableGameLocation )
            {
                Building bldg = (Game1.currentLocation as BuildableGameLocation).getBuildingAt(bobberTile);
                if ( bldg != null && bldg is FishPond && ( int ) ( bldg as FishPond ).currentOccupants > 0 )
                {
                    __result = FishPond.FISHING_MILLISECONDS;
                    return false;
                }
            }
            int b = FishingRod.maxFishingBiteTime - 250 * who.FishingLevel;
            int c = 0;
            if ( __instance.attachments[1] != null && __instance.attachments[1].ParentSheetIndex == 686 ||
                 __instance.hasEnchantmentOfType<MoreLuresEnchantment>() && 
                 __instance.attachments[ 2 ] != null && __instance.attachments[ 2 ].ParentSheetIndex == 686 )
            {
                c += 5000;
            }
            if ( __instance.attachments[ 1 ] != null && __instance.attachments[ 1 ].ParentSheetIndex == 687 ||
                      __instance.hasEnchantmentOfType<MoreLuresEnchantment>() &&
                      __instance.attachments[ 2 ] != null && __instance.attachments[ 2 ].ParentSheetIndex == 687 )
            {
                c += 10000;
            }
            b -= c;

            float time = Game1.random.Next(FishingRod.minFishingBiteTime, b);
            if ( Mod.instance.Helper.Reflection.GetField<bool>(__instance, "isFirstCast").GetValue() )
            {
                time *= 0.75f;
            }
            if ( __instance.attachments[ 0 ] != null )
            {
                time *= 0.5f;
                if ( ( int ) __instance.attachments[ 0 ].parentSheetIndex == 774 )
                {
                    time *= 0.75f;
                }
            }
            __result = Math.Max( 500f, time );
            return false;
        }
    }

    [HarmonyPatch( typeof( FishingRod ), "attach" )]
    public static class FishingRodAttachPatch
    {
        public static bool Prefix( FishingRod __instance, StardewValley.Object o, ref StardewValley.Object __result )
        {
            if ( o != null && o.Category == -21 && ( int ) __instance.upgradeLevel > 1 )
            {
                StardewValley.Object tmp = __instance.attachments[0];
                if ( tmp != null && tmp.canStackWith( o ) )
                {
                    tmp.Stack = o.addToStack( tmp );
                    if ( tmp.Stack <= 0 )
                    {
                        tmp = null;
                    }
                }
                __instance.attachments[ 0 ] = o;
                Game1.playSound( "button1" );
                __result = tmp;
                return false;
            }
            if ( o != null && o.Category == -22 && ( int ) __instance.upgradeLevel > 2 )
            {
                // Rewrote this portion
                bool hasEnchant = __instance.hasEnchantmentOfType< MoreLuresEnchantment >();
                if ( __instance.attachments[ 1 ] == null )
                {
                    __instance.attachments[ 1 ] = o;
                }
                else if ( __instance.attachments[ 2 ] == null && hasEnchant )
                {
                    __instance.attachments[ 2 ] = o;
                }
                else if ( __instance.attachments[ 2 ] != null && hasEnchant )
                {
                    __result = __instance.attachments[ 2 ];
                    __instance.attachments[ 2 ] = o;
                }
                else if ( __instance.attachments[ 1 ] != null )
                {
                    __result = __instance.attachments[ 1 ];
                    __instance.attachments[ 1 ] = o;
                }
                Game1.playSound( "button1" );
                return false;
            }
            if ( o == null )
            {
                if ( __instance.attachments[ 0 ] != null )
                {
                    StardewValley.Object result2 = __instance.attachments[0];
                    __instance.attachments[ 0 ] = null;
                    Game1.playSound( "dwop" );
                    __result = result2;
                    return false;
                }
                if ( __instance.attachments[ 2 ] != null )
                {
                    StardewValley.Object result3 = __instance.attachments[2];
                    __instance.attachments[ 2 ] = null;
                    Game1.playSound( "dwop" );
                    __result = result3;
                    return false;
                }
                if ( __instance.attachments[ 1 ] != null )
                {
                    StardewValley.Object result3 = __instance.attachments[1];
                    __instance.attachments[ 1 ] = null;
                    Game1.playSound( "dwop" );
                    __result = result3;
                    return false;
                }
            }
            __result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(FishingRod), nameof(FishingRod.drawAttachments))]
    public static class FishingRodDrawAttachmentsPatch
    {
        public static void Postfix( FishingRod __instance, SpriteBatch b, int x, int y )
        {
            if ( ( int ) __instance.upgradeLevel > 2 && __instance.hasEnchantmentOfType< MoreLuresEnchantment >() )
            {
                if ( __instance.attachments[ 2 ] == null )
                {
                    b.Draw( Game1.menuTexture, new Vector2( x, y + 64 + 4 + 64 + 4 ), Game1.getSourceRectForStandardTileSheet( Game1.menuTexture, 37 ), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f );
                    return;
                }
                b.Draw( Game1.menuTexture, new Vector2( x, y + 64 + 4 + 64 + 4 ), Game1.getSourceRectForStandardTileSheet( Game1.menuTexture, 10 ), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f );
                __instance.attachments[ 2 ].drawInMenu( b, new Vector2( x, y + 64 + 4 + 64 + 4 ), 1f );
            }
        }
    }
}

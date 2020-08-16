using Harmony;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Network;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Overrides
{
    [HarmonyPatch( typeof( Fence ), MethodType.Constructor, new Type[] { typeof( Vector2 ), typeof( int ), typeof( bool ) } )]
    public static class FenceConstructorPatch
    {
        public static void Postfix( Fence __instance, Vector2 tileLocation, int whichType, bool isGate )
        {
            foreach ( var fence in Mod.instance.fences )
            {
                if ( whichType == fence.correspondingObject.GetObjectId() )
                {
                    __instance.health.Value = ( float ) ( fence.MaxHealth + Game1.random.Next( -100, 101 ) / 100.0 );
                    __instance.name = fence.Name;
                    __instance.ParentSheetIndex = -whichType;

                    __instance.health.Value *= 2;
                    __instance.maxHealth.Value = __instance.health.Value;
                    return;
                }
            }
        }
    }

    [HarmonyPatch( typeof( Fence ), nameof( Fence.repair ) )]
    public static class FenceRepairPatch
    {
        public static bool Prefix( Fence __instance )
        {
            foreach ( var fence in Mod.instance.fences )
            {
                if ( __instance.whichType.Value == fence.correspondingObject.GetObjectId() )
                {
                    __instance.health.Value = ( float ) ( fence.MaxHealth + Game1.random.Next( -100, 101 ) / 100.0 );
                    __instance.name = fence.Name;
                    __instance.ParentSheetIndex = -__instance.whichType.Value;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( Fence ), nameof( Fence.dropItem ) )]
    public static class FenceDropItemPatch
    {
        public static bool Prefix( Fence __instance, GameLocation location, Vector2 origin, Vector2 destination )
        {
            if ( __instance.isGate.Value )
                return true;

            foreach ( var fence in Mod.instance.fences )
            {
                if ( __instance.whichType.Value == fence.correspondingObject.GetObjectId() )
                {
                    location.debris.Add( new Debris( fence.correspondingObject.GetObjectId(), origin, destination ) );
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( Fence ), nameof( Fence.performToolAction ) )]
    public static class FencePerformToolActionPatch
    {
        public static bool Prefix( Fence __instance, Tool t, GameLocation location, ref bool __result )
        {
            if ( __instance.heldObject.Value != null && t != null && ( !( t is MeleeWeapon ) && t.isHeavyHitter() ) )
                return true;
            else if ( __instance.isGate.Value && t != null && ( t is Axe || t is Pickaxe ) )
                return true;

            foreach ( var fence in Mod.instance.fences )
            {
                if ( __instance.whichType.Value == fence.correspondingObject.GetObjectId() )
                {
                    __result = false;

                    if ( fence.BreakTool == Data.FenceData.ToolType.Pickaxe && t is Pickaxe ||
                         fence.BreakTool == Data.FenceData.ToolType.Axe && t is Axe )
                    {
                    }
                    else return false;

                    location.playSound( t is Axe ? "axchop" : "hammer", NetAudio.SoundContext.Default );
                    location.objects.Remove( __instance.tileLocation.Value );
                    for ( int i = 0; i < 4; ++i )
                        location.temporarySprites.Add( new CosmeticDebris( __instance.fenceTexture.Value,
                                                                            new Vector2( __instance.tileLocation.X * 64 + 32, __instance.tileLocation.Y * 64 + 32 ),
                                                                            Game1.random.Next( -5, 5 ) / 100f,
                                                                            Game1.random.Next( -64, 64 ) / 30f,
                                                                            Game1.random.Next( -800, -100 ) / 100f,
                                                                            ( int ) ( ( __instance.tileLocation.Y + 1 ) * 64 ),
                                                                            new Rectangle( 32 + Game1.random.Next( 2 ) * 16 / 2, 96 + Game1.random.Next( 2 ) * 16 / 2, 8, 8 ),
                                                                            Color.White,
                                                                            Game1.soundBank != null ? Game1.soundBank.GetCue( "shiny4" ) : null,
                                                                            null,
                                                                            0,
                                                                            200 ) );
                    Game1.createRadialDebris( location, t is Axe ? 12 : 14, ( int ) __instance.tileLocation.X, ( int ) __instance.tileLocation.Y, 6, false, -1, false, -1 );
                    Mod.instance.Helper.Reflection.GetField<Multiplayer>( typeof( Game1 ), "multiplayer" ).GetValue()
                        .broadcastSprites( location, new TemporaryAnimatedSprite( 12, new Vector2( __instance.tileLocation.X * 64, __instance.tileLocation.Y * 64 ),
                                                                                    Color.White, 8, Game1.random.NextDouble() < 0.5, 50, 0, -1, -1, -1, 0 ) );
                    if ( __instance.maxHealth.Value - __instance.health.Value < 0.5 )
                    {
                        location.debris.Add( new Debris( new StardewValley.Object( fence.correspondingObject.GetObjectId(), 1, false, -1, 0 ),
                                                            __instance.tileLocation.Value * 64 + new Vector2( 32, 32 ) ) );
                    }
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( Fence ), nameof( Fence.performObjectDropInAction ) )]
    public static class FencePerformObjectDropInPatch
    {
        public static bool Prefix( Fence __instance, Item dropIn, bool probe, Farmer who, ref bool __result )
        {
            if ( __instance.health.Value > 1 || !__instance.CanRepairWithThisItem( dropIn ) )
                return true;

            foreach ( var fence in Mod.instance.fences )
            {
                if ( __instance.whichType.Value == fence.correspondingObject.GetObjectId() )
                {
                    if ( probe )
                    {
                        __result = true;
                        return false;
                    }

                    if ( dropIn.ParentSheetIndex == fence.correspondingObject.GetObjectId() )
                    {
                        __instance.health.Value = fence.MaxHealth + Game1.random.Next( -1000, 1000 ) / 100f; // Technically I should add a field to the json to make this changeable, but meh.
                        who.currentLocation.playSound( fence.RepairSound, NetAudio.SoundContext.Default );
                        __result = true;
                        return false;
                    }
                }
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( Fence ), nameof( Fence.CanRepairWithThisItem ) )]
    public static class FenceCanRepairWithThisPatch
    {
        public static bool Prefix( Fence __instance, Item item, ref  bool __result )
        {
            if ( __instance.health.Value > 1 || !( item is StardewValley.Object ) )
                return true;

            foreach ( var fence in Mod.instance.fences )
            {
                if ( __instance.whichType.Value == fence.correspondingObject.GetObjectId() )
                {
                    __result = Utility.IsNormalObjectAtParentSheetIndex( item, fence.correspondingObject.GetObjectId() );
                    return false;
                }
            }

            return true;
        }
    }
}

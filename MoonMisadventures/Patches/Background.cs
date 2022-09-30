using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonMisadventures.Game;
using StardewValley;

namespace MoonMisadventures.Patches
{
    [HarmonyPatch( typeof( Background ), nameof( Background.update ) )]
    public static class BackgroundUpdatePatch
    {
        public static void Postfix( Background __instance, xTile.Dimensions.Rectangle viewport )
        {
            if ( __instance is SpaceBackground bg )
            {
                bg.Update( viewport );
            }
        }
    }

    [HarmonyPatch( typeof( Background ), nameof( Background.draw ) )]
    public static class BackgroundDrawPatch
    {
        public static void Postfix( Background __instance, SpriteBatch b )
        {
            if ( __instance is SpaceBackground bg )
            {
                bg.Draw( b );
            }
        }
    }

    [HarmonyPatch( typeof( SpriteBatch ), nameof( SpriteBatch.Begin ) )]
    public static class SpriteBatchForceStencilPatch
    {
        private static AlphaTestEffect ate;
        public static void Prefix( ref DepthStencilState depthStencilState, ref BlendState blendState, ref Effect effect )
        {
            if (Game1.currentLocation is not Game.Locations.LunarLocation)
                return;

            if ( Mod.DefaultStencilOverride != null && depthStencilState == null )
            {
                if ( ate == null || true )
                {
                    ate = new AlphaTestEffect( Game1.graphics.GraphicsDevice )
                    {
                        Projection = Matrix.CreateOrthographicOffCenter( 0, Game1.viewport.Width, Game1.viewport.Height, 0, 0, 1 ),
                        VertexColorEnabled = true
                    };
                }
                if ( effect == null )
                    effect = ate;

                depthStencilState = Mod.DefaultStencilOverride;
                //SpaceShared.Log.Debug( "darkening" );
            }

            if ( Game1CatchLightingRenderPatch.IsDoingLighting )
            {

                /*
                var x = new DepthStencilState()
                {
                    StencilEnable = true,
                    StencilFunction = CompareFunction.NotEqual,
                    StencilPass = StencilOperation.Replace,
                    ReferenceStencil = 0,
                    DepthBufferEnable = false,
                };
                Game1CatchLightingRenderPatch.IsDoingLighting = false;
                Game1.spriteBatch.Begin( depthStencilState: x );
                Game1.spriteBatch.Draw( Game1.staminaRect, new Rectangle( 0, 0, Game1.viewport.Width, Game1.viewport.Height ), Color.Red );
                Game1.spriteBatch.End();
                x.Dispose();
                Game1CatchLightingRenderPatch.IsDoingLighting = true;*/
                /*
                effect = new AlphaTestEffect( Game1.graphics.GraphicsDevice )
                {
                    Projection = Matrix.CreateOrthographicOffCenter( 0, Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth / 4, Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight / 4, 0, 0, 1 ),
                    VertexColorEnabled = true,
                };
                */
                effect = null;

                //blendState = BlendState.NonPremultiplied;
                depthStencilState = Mod.StencilRenderOnDark;
                //SpaceShared.Log.Debug( "mask:" + depthStencilState.StencilPass+" "+depthStencilState.StencilFail);
                //depthStencilState = Mod.StencilRenderOnDark;
            }
        }
    }

    [HarmonyPatch( typeof( SpriteBatch ), nameof( SpriteBatch.End ) )]
    public static class SpriteBatchFinishLightingPatch
    {
        public static void Postfix()
        {
            if ( Game1CatchLightingRenderPatch.IsDoingLighting )
            {
                Game1CatchLightingRenderPatch.IsDoingLighting = false;
                Mod.DefaultStencilOverride = null;
            }
        }
    }

    // Can't [HarmonyPatch] SGame since it is internal
    public static class Game1CatchLightingRenderPatch
    {
        public static bool IsDoingLighting = false;

        public static void DoStuff()
        {
            IsDoingLighting = true;
        }

        public static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> insns, ILGenerator ilgen )
        {
            List< CodeInstruction > ret = new();

            int countdown = 0;
            foreach ( var insn in insns )
            {
                if ( insn.opcode == OpCodes.Ldsfld && insn.operand == typeof( Game1 ).GetField( "drawLighting" ) )
                {
                    countdown = 4;
                }
                else if ( countdown > 0 && --countdown == 0 )
                {
                    ret.Add( new CodeInstruction( OpCodes.Call, typeof( Game1CatchLightingRenderPatch ).GetMethod( "DoStuff" ) ) );
                }
                
                ret.Add( insn );
            }

            return ret;
        }
    }

    [HarmonyPatch(typeof(Game1), nameof(Game1.SetWindowSize))]
    public static class Game1AddStencilToScreenPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, ILGenerator ilgen)
        {
            List<CodeInstruction> ret = new();

            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Ldstr && insn.operand is string str && str == "Screen")
                {
                    ret[ret.Count - 7].opcode = OpCodes.Ldc_I4_3;
                }

                ret.Add(insn);
            }

            return ret;
        }
    }

    [HarmonyPatch( typeof( Game1 ), nameof( Game1.ShouldDrawOnBuffer ) )]
    public static class Game1ForceRenderOnBufferOnMoonPatch
    {
        public static void Postfix( ref bool __result )
        {
            if (Game1.background is SpaceBackground)
                __result = true;
        }
    }
}

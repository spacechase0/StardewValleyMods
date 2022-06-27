using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using xTile.Layers;

namespace FloodedValleyFarm
{
    [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Begin))]
    public static class SpriteBatchForceStencilPatch
    {
        private static AlphaTestEffect ate;
        public static void Prefix(ref DepthStencilState depthStencilState, ref BlendState blendState, ref Effect effect)
        {
            if (Game1.currentLocation?.Name != "Farm")
                return;

            if (Mod.DefaultStencilOverride != null && depthStencilState == null)
            {
                if (ate == null || true)
                {
                    ate = new AlphaTestEffect(Game1.graphics.GraphicsDevice)
                    {
                        Projection = Matrix.CreateOrthographicOffCenter(0, Game1.viewport.Width, Game1.viewport.Height, 0, 0, 1),
                        VertexColorEnabled = true
                    };
                }
                if (effect == null)
                    effect = ate;

                depthStencilState = Mod.DefaultStencilOverride;
                //SpaceShared.Log.Debug( "darkening" );
            }

            if (Game1CatchLightingRenderPatch.IsDoingLighting)
            {
                depthStencilState = null;
            }
        }
    }

    [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.End))]
    public static class SpriteBatchFinishLightingPatch
    {
        public static void Postfix()
        {
            if (Game1CatchLightingRenderPatch.IsDoingLighting)
            {
                Game1CatchLightingRenderPatch.IsDoingLighting = false;
            }
            else if (LayerDontStencilOnBackPatch.justStopped != null)
            {
                Mod.DefaultStencilOverride = LayerDontStencilOnBackPatch.justStopped;
                LayerDontStencilOnBackPatch.justStopped = null;
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

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, ILGenerator ilgen)
        {
            List<CodeInstruction> ret = new();

            int countdown = 0;
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Ldsfld && insn.operand == typeof(Game1).GetField("drawLighting"))
                {
                    countdown = 4;
                }
                else if (countdown > 0 && --countdown == 0)
                {
                    ret.Add(new CodeInstruction(OpCodes.Call, typeof(Game1CatchLightingRenderPatch).GetMethod("DoStuff")));
                }

                ret.Add(insn);
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

    [HarmonyPatch(typeof(Game1), nameof(Game1.ShouldDrawOnBuffer))]
    public static class Game1ForceRenderOnBufferOnMoonPatch
    {
        public static void Postfix(ref bool __result)
        {
            if (Game1.currentLocation?.Name == "Farm")
                __result = true;
        }
    }

    [HarmonyPatch(typeof(Layer), nameof(Layer.Draw))]
    public static class LayerDontStencilOnBackPatch
    {
        internal static DepthStencilState justStopped = null;
        public static void Prefix(Layer __instance)
        {
            if (__instance.Id == "Back")
            {
                justStopped = Mod.DefaultStencilOverride;
                Mod.DefaultStencilOverride = null;
            }
        }
    }
}

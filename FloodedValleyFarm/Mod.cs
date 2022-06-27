using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI.Events;
using StardewValley;

namespace FloodedValleyFarm
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        internal static DepthStencilState DefaultStencilOverride = null;
        internal static DepthStencilState StencilBrighten = new()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 1,
            DepthBufferEnable = false,
        };
        internal static DepthStencilState StencilDarken = new()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 0,
            DepthBufferEnable = false,
        };
        internal static DepthStencilState StencilRenderOnDark = new()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.NotEqual,
            StencilPass = StencilOperation.Keep,
            ReferenceStencil = 1,
            DepthBufferEnable = false,
        };

        private static Effect waterEffect;

        public override void Entry(StardewModdingAPI.IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.Display.RenderingWorld += this.Display_RenderingWorld;
            helper.Events.Display.RenderedWorld += this.Display_RenderedWorld;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            harmony.Patch(AccessTools.Method("StardewModdingAPI.Framework.SGame:DrawImpl"), transpiler: new HarmonyMethod(typeof(Game1CatchLightingRenderPatch).GetMethod("Transpiler")));
        }

        private void Display_RenderingWorld(object sender, RenderingWorldEventArgs e)
        {
            if (Game1.currentLocation.Name != "Farm")
                return;

            DefaultStencilOverride = StencilBrighten;
            Game1.graphics.GraphicsDevice.Clear(ClearOptions.Stencil, Color.Transparent, 0, 0);
        }

        private RenderTarget2D rtarget;

        [EventPriority(EventPriority.Low)]
        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            DefaultStencilOverride = null;

            if (Game1.currentLocation.Name != "Farm")
                return;

            var rtargetsOld = Game1.graphics.GraphicsDevice.GetRenderTargets();
            if (rtargetsOld.Length == 0 || rtargetsOld[0].RenderTarget == null)
            {
                Log.Warn("not rendertarget-ing?");
                return;
            }

            var rtargetOld = rtargetsOld[0].RenderTarget as RenderTarget2D;
            e.SpriteBatch.End();
            bool sopen = false;
            try
            {
                /*
                if (rtarget == null || rtarget.Width != rtargetOld.Width || rtarget.Height != rtargetOld.Height)
                {
                    rtarget?.Dispose();
                    rtarget = new RenderTarget2D(Game1.graphics.GraphicsDevice, rtargetOld.Width, rtargetOld.Height);
                }
                Game1.graphics.GraphicsDevice.SetRenderTarget(rtarget);

                Game1.graphics.GraphicsDevice.Clear(Color.White);
                e.SpriteBatch.Begin(depthStencilState: StencilRenderOnDark);
                sopen = true;
                e.SpriteBatch.Draw(Game1.staminaRect, new Rectangle(0, 0, rtarget.Width, rtarget.Height), Color.Black);
                e.SpriteBatch.End();
                sopen = false;*/

                //Game1.graphics.GraphicsDevice.SetRenderTargets(rtargetsOld);
                e.SpriteBatch.Begin(effect: waterEffect, depthStencilState: StencilRenderOnDark);
                sopen = true;
                e.SpriteBatch.Draw(Game1.staminaRect, new Rectangle(0,0,rtargetOld.Width,rtargetOld.Height), Color.White);
                e.SpriteBatch.End();
                sopen = false;
            }
            finally
            {
                if ( sopen )
                    e.SpriteBatch.End();
                Game1.graphics.GraphicsDevice.SetRenderTargets(rtargetsOld);
                e.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
        }
    }
}

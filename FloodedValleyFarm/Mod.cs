using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI.Events;
using StardewValley;

namespace FloodedValleyFarm
{
    public static class SpriteBatcherWaterPatch
    {
        public static void Prefix(ref Effect effect, Texture texture)
        {
            if (Game1.currentLocation == null)
                return;

            string texName = texture?.Name?.Replace('/', '_');
            if (texName != null && Game1.currentLocation.Name == "Farm" && Mod.maskTextures.ContainsKey(texName))
            {
                Color wcol = Game1.currentLocation.waterColor.Value;

                effect = Mod.waterEffect;
                effect.Parameters["SpriteTexture"].SetValue(texture);
                effect.Parameters["WaterTexture"].SetValue(Mod.waterTex);
                effect.Parameters["MaskTexture"].SetValue(Mod.maskTextures[texName]);
                effect.Parameters["WaterColor"].SetValue(wcol.ToVector4());
            }
        }
        public static void Postfix(ref Effect effect, Texture texture)
        {
            if (Game1.currentLocation == null)
                return;

            string texName = texture?.Name?.Replace('/', '_');
            if (texName != null && Game1.currentLocation.Name == "Farm" && Mod.maskTextures.ContainsKey(texName))
            {
                Mod.instance.Helper.Reflection.GetField<EffectPass>(Game1.spriteBatch, "_spritePass").GetValue().Apply();
            }
        }
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

#if false
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
#endif

        internal static Texture2D waterTex;
        internal static Effect waterEffect;

        internal static Dictionary<string, Texture2D> maskTextures = new();

        public override void Entry(StardewModdingAPI.IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            waterTex = Helper.ModContent.Load<Texture2D>("assets/water.png");
            waterEffect = Helper.ModContent.Load<Effect>("assets/PartialWater.xnb");

            //helper.Events.Display.RenderingWorld += this.Display_RenderingWorld;
            //helper.Events.Display.RenderedWorld += this.Display_RenderedWorld;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            //harmony.Patch(AccessTools.Method("StardewModdingAPI.Framework.SGame:DrawImpl"), transpiler: new HarmonyMethod(typeof(Game1CatchLightingRenderPatch).GetMethod("Transpiler")));
            harmony.Patch(
                AccessTools.Method("Microsoft.Xna.Framework.Graphics.SpriteBatcher:FlushVertexArray"),
                prefix: new HarmonyMethod(typeof(SpriteBatcherWaterPatch).GetMethod("Prefix")),
                postfix: new HarmonyMethod(typeof(SpriteBatcherWaterPatch).GetMethod("Postfix"))
            );

            //maskTextures.Add("TileSheets_Craftables", Helper.ModContent.Load<Texture2D>("assets/masks/TileSheets_Craftables.png"));

            helper.Events.Content.AssetRequested += this.Content_AssetRequested;
        }

        [EventPriority(EventPriority.Low - 1 ) ]
        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("TileSheets/Craftables"))
            {
                e.Edit(ad =>
                {
                    Texture2D tex = ad.AsImage().Data;
                    string texName = ad.NameWithoutLocale.Name.Replace('/', '_');

                    if (maskTextures.ContainsKey(texName))
                        maskTextures.Remove(texName);

                    var baseMask = Helper.ModContent.Load<Texture2D>("assets/masks/" + texName + ".png");

                    Color[] baseCols = new Color[baseMask.Width * baseMask.Height];
                    baseMask.GetData(baseCols);

                    Color[] cols = new Color[tex.Width * tex.Height];
                    for (int ix = 0; ix < tex.Width; ++ix)
                    {
                        for (int iy = 0; iy < tex.Height; ++iy)
                        {
                            cols[ix + iy * tex.Width] = baseCols[ix % 16 + (iy % 32) * 16];
                        }
                    }

                    Texture2D newTex = new(Game1.graphics.GraphicsDevice, tex.Width, tex.Height);
                    newTex.SetData(cols);
                    maskTextures.Add(texName, newTex);
                }, AssetEditPriority.Late + 1);
            }
        }


#if false
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
#endif
    }
}

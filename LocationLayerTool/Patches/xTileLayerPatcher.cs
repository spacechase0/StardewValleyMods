using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.ObjectModel;

namespace LocationLayerTool.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Layer"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class xTileLayerPatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        private static int Rendering;
        private static IDisplayDevice DisplayDevice;
        private static SpriteBatch SpriteBatch;
        private static RenderTarget2D RenderTarget;
        private static RenderTarget2D LightMap;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Layer>(nameof(Layer.Draw)),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Layer.Draw"/>.</summary>
        private static void Before_Draw(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, bool wrapAround, int pixelZoom)
        {
            if (__instance.Id == "Back" && xTileLayerPatcher.Rendering == 0 && Game1.currentLocation.Map.Properties.TryGetValue("RenderBehind", out PropertyValue renderBehind))
            {
                xTileLayerPatcher.Rendering++;
                try
                {
                    string[] fields = renderBehind.ToString().Split(' ');
                    string locName = fields[0];
                    int offsetX = 0, offsetY = 0;
                    if (fields.Length >= 3)
                    {
                        offsetX = int.Parse(fields[1]);
                        offsetY = int.Parse(fields[2]);
                    }
                    float scale = 1f;
                    if (fields.Length >= 4)
                    {
                        scale = float.Parse(fields[3]);
                    }
                    xTileLayerPatcher.DoRendering(locName, offsetX, offsetY, scale);
                }
                catch (Exception e)
                {
                    Log.Error("Exception while rendering: " + e);
                }
                xTileLayerPatcher.Rendering--;
            }
        }

        private static void DoRendering(string locName, int offsetX, int offsetY, float scale)
        {
            if (xTileLayerPatcher.DisplayDevice == null)
            {
                var ddType = Type.GetType("StardewModdingAPI.Framework.Rendering.SDisplayDevice, StardewModdingAPI");
                var ddCon = ddType.GetConstructor(new[] { typeof(ContentManager), typeof(GraphicsDevice) });
                xTileLayerPatcher.DisplayDevice = (IDisplayDevice)ddCon.Invoke(new object[] { Game1.content, Game1.graphics.GraphicsDevice });
                xTileLayerPatcher.SpriteBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);
            }
            if (xTileLayerPatcher.RenderTarget == null || xTileLayerPatcher.RenderTarget.Width != Game1.graphics.GraphicsDevice.Viewport.Width || xTileLayerPatcher.RenderTarget.Height != Game1.graphics.GraphicsDevice.Viewport.Height)
            {
                xTileLayerPatcher.RenderTarget = new RenderTarget2D(Game1.graphics.GraphicsDevice, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height, false, Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

                /*
                if (Game1.game1.screen.RenderTargetUsage != RenderTargetUsage.PreserveContents)
                    Game1.game1.screen = new RenderTarget2D(Game1.graphics.GraphicsDevice, screen.Width, screen.Height, false, screen.Format, screen.DepthStencilFormat, screen.MultiSampleCount, RenderTargetUsage.PreserveContents);
                */
            }
            xTileLayerPatcher.LightMap ??= xTileLayerPatcher.AllocateLightMapNoDispose();

            var lightmapField = Mod.Instance.Helper.Reflection.GetField<RenderTarget2D>(typeof(Game1), "_lightmap");
            var lightingBlend = Mod.Instance.Helper.Reflection.GetField<BlendState>(Game1.game1, "lightingBlend");

            var oldDd = Game1.mapDisplayDevice;
            var oldSb = Game1.spriteBatch;
            var oldLoc = Game1.currentLocation;
            var oldTarget = (RenderTarget2D)(Game1.graphics.GraphicsDevice.GetRenderTargets().Length > 0 ? Game1.graphics.GraphicsDevice.GetRenderTargets()[0].RenderTarget : null);
            var oldDebris = Game1.debrisWeather;
            bool oldLighting = Game1.drawLighting;
            var oldLights = Game1.currentLightSources;
            var oldLightmap = Game1.lightmap;
            var oldOutdoor = Game1.outdoorLight;
            var oldAmbient = Game1.ambientLight;
            var oldLightBlend = lightingBlend.GetValue();
            Game1.displayHUD = false;
            Game1.mapDisplayDevice = xTileLayerPatcher.DisplayDevice;
            Game1.spriteBatch = xTileLayerPatcher.SpriteBatch;
            Game1.currentLocation = Game1.getLocationFromName(locName);
            Game1.debrisWeather = null;
            //Game1.drawLighting = false;
            Game1.currentLightSources = new HashSet<LightSource>(); xTileLayerPatcher.BuildLightSources();
            lightmapField.SetValue(xTileLayerPatcher.LightMap);
            //Game1.outdoorLight = Color.White;
            Game1.ambientLight = new Color(254, 254, 254, 0);
            lightingBlend.SetValue(BlendState.Additive);
            Game1.graphics.GraphicsDevice.SetRenderTarget(xTileLayerPatcher.RenderTarget);
            try
            {
                Game1.currentLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
                Mod.Instance.Helper.Reflection.GetMethod(Game1.game1, "_draw").Invoke(new GameTime(), xTileLayerPatcher.RenderTarget);
            }
            catch (Exception e)
            {
                Log.Trace("Exception rendering: " + e);
            }
            Game1.displayHUD = true;
            Game1.mapDisplayDevice = oldDd;
            Game1.spriteBatch = oldSb;
            Game1.currentLocation = oldLoc;
            Game1.debrisWeather = oldDebris;
            Game1.game1.takingMapScreenshot = false;
            Game1.drawLighting = oldLighting;
            Game1.currentLightSources = oldLights;
            lightmapField.SetValue(oldLightmap);
            Game1.outdoorLight = oldOutdoor;
            Game1.ambientLight = oldAmbient;
            lightingBlend.SetValue(oldLightBlend);
            Game1.graphics.GraphicsDevice.SetRenderTarget(oldTarget);

            Game1.spriteBatch.Draw(xTileLayerPatcher.RenderTarget, new Vector2(offsetX * Game1.tileSize, offsetY * Game1.tileSize), null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        private static RenderTarget2D AllocateLightMapNoDispose()
        {
            int width = 2048, height = 2048;
            int num1 = 32;
            float num2 = 1f;
            if (Game1.options != null)
            {
                num1 = Game1.options.lightingQuality;
                num2 = Game1.options.zoomLevel;
            }
            int width1 = (int)(width * (1.0 / num2) + 64.0) / (num1 / 2);
            int height1 = (int)(height * (1.0 / num2) + 64.0) / (num1 / 2);
            //if ( Game1.lightmap != null && Game1.lightmap.Width == width1 && Game1.lightmap.Height == height1 )
            //    return null;
            //if ( Game1._lightmap != null )
            //    Game1._lightmap.Dispose();
            return new RenderTarget2D(Game1.graphics.GraphicsDevice, width1, height1, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        private static void BuildLightSources()
        {
            Game1.currentLocation.Map.Properties.TryGetValue("Light", out PropertyValue lightProp);
            if (lightProp != null && !Game1.currentLocation.ignoreLights.Value)
            {
                string[] strArray = lightProp.ToString().Split(' ');
                for (int index = 0; index < strArray.Length; index += 3)
                    Game1.currentLightSources.Add(new LightSource(Convert.ToInt32(strArray[index + 2]), new Vector2(Convert.ToInt32(strArray[index]) * 64 + 32, Convert.ToInt32(strArray[index + 1]) * 64 + 32), 1f, LightSource.LightContext.MapLight));
            }
            Game1.currentLightSources.UnionWith(Game1.currentLocation.sharedLights.Values);
        }
    }
}

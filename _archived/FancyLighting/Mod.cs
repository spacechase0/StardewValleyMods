using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Penumbra;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Tools;
using xTile.Layers;
using xTile.Tiles;

namespace FancyLighting
{
    public class Mod : StardewModdingAPI.Mod, IAssetLoader
    {
        public static Mod instance;
        public static bool ShouldRun { get; private set; }
        public static PenumbraComponent penumbra;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            int expectedMajor = 4, expectedMinor = 0, expectedPatch = 0;
            if (Constants.ApiVersion.MajorVersion != expectedMajor && Constants.ApiVersion.MinorVersion != expectedMinor &&
                 Constants.ApiVersion.PatchVersion != expectedPatch)
            {
                Log.Error($"SMAPI version {expectedMajor}.{expectedMinor}.{expectedPatch} required! This mod will not run.");
                ShouldRun = false;
                return;
            }

            penumbra = new(GameRunner.instance);
            penumbra.Initialize();

            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            harmony.Patch(AccessTools.Method("StardewModdingAPI.Framework.SGame:DrawImpl"),
                          new HarmonyMethod(AccessTools.Method(typeof(SGameDrawOverride), nameof(SGameDrawOverride.Prefix))));
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (asset.Name.IsEquivalentTo("PenumbraHull") || asset.Name.IsEquivalentTo("PenumbraLight") ||
                asset.Name.IsEquivalentTo("PenumbraShadow") || asset.Name.IsEquivalentTo("PenumbraTexture"))
                return true;
            return false;
        }

        public T Load<T>(IAssetInfo asset)
        {
            if (asset.Name.IsEquivalentTo("PenumbraHull"))
                return (T)(object)Helper.Content.Load<Effect>("PenumbraHull");
            else if (asset.Name.IsEquivalentTo("PenumbraLight"))
                return (T)(object)Helper.Content.Load<Effect>("PenumbraLight");
            else if (asset.Name.IsEquivalentTo("PenumbraShadow"))
                return (T)(object)Helper.Content.Load<Effect>("PenumbraShadow");
            else if (asset.Name.IsEquivalentTo("PenumbraTexture"))
                return (T)(object)Helper.Content.Load<Effect>("PenumbraTexture");
            return default(T);
        }

        private Dictionary<StardewValley.LightSource, Penumbra.Light> lights = new();
        private Dictionary<object, Penumbra.Hull> hulls = new();
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !Game1.drawLighting)
                return;

            Color lighting = ((Game1.currentLocation is StardewValley.Locations.MineShaft && Game1.currentLocation.Name.StartsWith("UndergroundMine")) ? (Game1.currentLocation as StardewValley.Locations.MineShaft).getLightingColor(Game1.currentGameTime) : ((Game1.ambientLight.Equals(Color.White) || (Game1.IsRainingHere() && (bool)Game1.currentLocation.isOutdoors)) ? Game1.outdoorLight : Game1.ambientLight));
            penumbra.AmbientColor = new Color(255 - lighting.R, 255 - lighting.G, 255 - lighting.B);
            penumbra.Transform = Matrix.CreateTranslation(new Vector3(-Game1.viewport.X, -Game1.viewport.Y, 0));
            penumbra.Debug = false;

            List<StardewValley.LightSource> toRemove = new();
            foreach (var light in lights.Keys)
            {
                if (!Game1.currentLightSources.Contains(light))
                    toRemove.Add(light);
            }
            foreach (var remove in toRemove)
            {
                penumbra.Lights.Remove(lights[remove]);
                lights.Remove(remove);
            }

            foreach (var light in Game1.currentLightSources)
            {
                if (!lights.ContainsKey(light))
                {
                    lights.Add(light, new TexturedLight()
                    {
                        Color = new Color( 255 - light.color.Value.R, 255 - light.color.Value.G, 255 - light.color.Value.B ),
                        Scale = new Vector2( light.lightTexture.Width, light.lightTexture.Height ) * light.radius.Value,
                        Texture = light.lightTexture
                    });
                    penumbra.Lights.Add(lights[light]);
                }

                lights[light].Position = light.position;
            }

            List<object> toRemove2 = new();
            foreach (var hull in hulls)
            {
                if (hull.Key is NPC && !Game1.currentLocation.characters.Contains((NPC)hull.Key))
                    toRemove2.Add(hull.Key);
            }
            foreach (var remove in toRemove2)
            {
                penumbra.Hulls.Remove(hulls[remove]);
                hulls.Remove(remove);
            }

            foreach (var npc in Game1.currentLocation.characters)
            {
                if (!hulls.ContainsKey(npc))
                {
                    var box = npc.GetBoundingBox();
                    box.Height = npc.Sprite.SpriteHeight * Game1.pixelZoom * 3 / 4;
                    hulls.Add(npc, Hull.CreateRectangle(box.Location.ToVector2(), box.Size.ToVector2(), 0, Vector2.Zero));
                    penumbra.Hulls.Add(hulls[npc]);
                }

                hulls[npc].Position = npc.GetBoundingBox().Location.ToVector2() - new Vector2( 0, npc.Sprite.SpriteHeight * Game1.pixelZoom - npc.GetBoundingBox().Height * 3 / 2);
            }
        }
    }

    [HarmonyPatch(typeof(Game1), nameof(Game1.ShouldDrawOnBuffer))]
    public static class Game1ForceRenderOnBufferPatch
    {
        public static void Postfix(ref bool __result)
        {
            __result = true;
        }
    }

    public static class SGameDrawOverride
    {
        public static bool Prefix(GameTime gameTime, RenderTarget2D target_screen,
                                  Task ____newDayTask, IMonitor ___Monitor, Multiplayer ___multiplayer)
        {
            if (!Context.IsWorldReady)
                return true;

            Impl(gameTime, target_screen,
                 ____newDayTask, ___Monitor, ___multiplayer);
            return false;
        }

        private static void Impl(GameTime gameTime, RenderTarget2D target_screen,
                                 Task ____newDayTask, IMonitor ___Monitor, Multiplayer ___multiplayer)
        {
            var graphicsDevice = Game1.graphics.GraphicsDevice;

            Game1.showingHealthBar = false;
            if (____newDayTask != null && Game1.game1.isLocalMultiplayerNewDayActive)
            {
                graphicsDevice.Clear(Game1.bgColor);
                return;
            }
            if (target_screen != null)
            {
                //Game1.SetRenderTarget(target_screen);
                Game1.graphics.GraphicsDevice.SetRenderTarget(target_screen);
            }

            if (Game1.game1.IsSaving)
            {
                graphicsDevice.Clear(Game1.bgColor);
                Game1.PushUIMode();
                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu != null)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                    //events.Rendering.RaiseEmpty();
                    try
                    {
                        //events.RenderingActiveMenu.RaiseEmpty();
                        menu.draw(Game1.spriteBatch);
                        //events.RenderedActiveMenu.RaiseEmpty();
                    }
                    catch (Exception ex)
                    {
                        ___Monitor.Log($"The {Game1.activeClickableMenu.GetType().FullName} menu crashed while drawing itself during save. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                        menu.exitThisMenu();
                    }
                    //events.Rendered.RaiseEmpty();
                    Game1.spriteBatch.End();
                }
                if (Game1.overlayMenu != null)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                    Game1.overlayMenu.draw(Game1.spriteBatch);
                    Game1.spriteBatch.End();
                }
                Game1.PopUIMode();
                return;
            }
            graphicsDevice.Clear(Game1.bgColor);
            if (Game1.activeClickableMenu != null && Game1.options.showMenuBackground && Game1.activeClickableMenu.showWithoutTransparencyIfOptionIsSet() && !Game1.game1.takingMapScreenshot)
            {
                Game1.PushUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

                //events.Rendering.RaiseEmpty();
                IClickableMenu curMenu = null;
                try
                {
                    Game1.activeClickableMenu.drawBackground(Game1.spriteBatch);
                    //events.RenderingActiveMenu.RaiseEmpty();
                    for (curMenu = Game1.activeClickableMenu; curMenu != null; curMenu = curMenu.GetChildMenu())
                    {
                        curMenu.draw(Game1.spriteBatch);
                    }
                    //events.RenderedActiveMenu.RaiseEmpty();
                }
                catch (Exception ex)
                {
                    ___Monitor.Log($"The {curMenu.GetMenuChainLabel()} menu crashed while drawing itself. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                    Game1.activeClickableMenu.exitThisMenu();
                }
                //events.Rendered.RaiseEmpty();
                if (Game1.specialCurrencyDisplay != null)
                {
                    Game1.specialCurrencyDisplay.Draw(Game1.spriteBatch);
                }
                Game1.spriteBatch.End();
                AccessTools.Method(Game1.game1.GetType(), "drawOverlays").Invoke(Game1.game1, new object[] { Game1.spriteBatch });
                Game1.PopUIMode();
                return;
            }
            if (Game1.gameMode == 11)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                //events.Rendering.RaiseEmpty();
                Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3685"), new Vector2(16f, 16f), Color.HotPink);
                Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3686"), new Vector2(16f, 32f), new Color(0, 255, 0));
                Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.parseText(Game1.errorMessage, Game1.dialogueFont, Game1.graphics.GraphicsDevice.Viewport.Width), new Vector2(16f, 48f), Color.White);
                //events.Rendered.RaiseEmpty();
                Game1.spriteBatch.End();
                return;
            }
            if (Game1.currentMinigame != null)
            {
                /*
                if (events.Rendering.HasListeners())
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    //events.Rendering.RaiseEmpty();
                    Game1.spriteBatch.End();
                }
                */

                Game1.currentMinigame.draw(Game1.spriteBatch);
                if (Game1.globalFade && !Game1.menuUp && (!Game1.nameSelectUp || Game1.messagePause))
                {
                    Game1.PushUIMode();
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((Game1.gameMode == 0) ? (1f - Game1.fadeToBlackAlpha) : Game1.fadeToBlackAlpha));
                    Game1.spriteBatch.End();
                    Game1.PopUIMode();
                }
                Game1.PushUIMode();

                AccessTools.Method(Game1.game1.GetType(), "drawOverlays").Invoke(Game1.game1, new object[] { Game1.spriteBatch });
                Game1.PopUIMode();
                /*
                if (events.Rendered.HasListeners())
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    //events.Rendered.RaiseEmpty();
                    Game1.spriteBatch.End();
                }
                */
                //Game1.SetRenderTarget(target_screen);
                Game1.graphics.GraphicsDevice.SetRenderTarget(target_screen);
                return;
            }
            if (Game1.showingEndOfNightStuff)
            {
                Game1.PushUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                //events.Rendering.RaiseEmpty();
                if (Game1.activeClickableMenu != null)
                {
                    IClickableMenu curMenu = null;
                    try
                    {
                        //events.RenderingActiveMenu.RaiseEmpty();
                        for (curMenu = Game1.activeClickableMenu; curMenu != null; curMenu = curMenu.GetChildMenu())
                        {
                            curMenu.draw(Game1.spriteBatch);
                        }
                        //events.RenderedActiveMenu.RaiseEmpty();
                    }
                    catch (Exception ex)
                    {
                        ___Monitor.Log($"The {curMenu.GetMenuChainLabel()} menu crashed while drawing itself. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                        Game1.activeClickableMenu.exitThisMenu();
                    }
                }
                Game1.spriteBatch.End();

                AccessTools.Method(Game1.game1.GetType(), "drawOverlays").Invoke(Game1.game1, new object[] { Game1.spriteBatch });
                Game1.PopUIMode();
                return;
            }
            if (Game1.gameMode == 6 || (Game1.gameMode == 3 && Game1.currentLocation == null))
            {
                Game1.PushUIMode();
                graphicsDevice.Clear(Game1.bgColor);
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                //events.Rendering.RaiseEmpty();
                string addOn = "".PadRight((int)Math.Ceiling(gameTime.TotalGameTime.TotalMilliseconds % 999.0 / 333.0), '.');
                string text = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3688");
                string msg = text + addOn;
                string largestMessage = text + "... ";
                int msgw = SpriteText.getWidthOfString(largestMessage);
                int msgh = 64;
                int msgx = 64;
                int msgy = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - msgh;
                SpriteText.drawString(Game1.spriteBatch, msg, msgx, msgy, 999999, msgw, msgh, 1f, 0.88f, junimoText: false, 0, largestMessage);
                //events.Rendered.RaiseEmpty();
                Game1.spriteBatch.End();

                AccessTools.Method(Game1.game1.GetType(), "drawOverlays").Invoke(Game1.game1, new object[] { Game1.spriteBatch });
                Game1.PopUIMode();
                return;
            }
            if (Game1.gameMode == 0)
            {
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                //events.Rendering.RaiseEmpty();
            }
            else
            {
                if (Game1.gameMode == 3 && Game1.dayOfMonth == 0 && Game1.newDay)
                {
                    // This was commented out in the SMAPI code, not by me
                    //base.Draw(gameTime);
                    return;
                }
                Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                bool renderingRaised = false;
                //*
                if (Game1.drawLighting)
                {
                    Mod.penumbra.BeginDraw();
                    // I'll need to redo this anyways
                    //Game1.game1.DrawLighting(gameTime, target_screen, out renderingRaised);
                }
                graphicsDevice.Clear(Game1.bgColor);
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (!renderingRaised)
                    ;// events.Rendering.RaiseEmpty();
                //events.RenderingWorld.RaiseEmpty();
                if (Game1.background != null)
                {
                    Game1.background.draw(Game1.spriteBatch);
                }
                Game1.currentLocation.drawBackground(Game1.spriteBatch);
                Game1.spriteBatch.End();
                for (int i = 0; i < Game1.currentLocation.backgroundLayers.Count; i++)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);
                    Game1.currentLocation.backgroundLayers[i].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, wrapAround: false, 4, -1f);
                    Game1.spriteBatch.End();
                }
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                Game1.currentLocation.drawWater(Game1.spriteBatch);
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
                Game1.currentLocation.drawFloorDecorations(Game1.spriteBatch);
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                var _farmerShadows = Mod.instance.Helper.Reflection.GetField<List<Farmer>>(Game1.game1, "_farmerShadows").GetValue();
                _farmerShadows.Clear();
                if (Game1.currentLocation.currentEvent != null && !Game1.currentLocation.currentEvent.isFestival && Game1.currentLocation.currentEvent.farmerActors.Count > 0)
                {
                    foreach (Farmer f in Game1.currentLocation.currentEvent.farmerActors)
                    {
                        if ((f.IsLocalPlayer && Game1.displayFarmer) || !f.hidden)
                        {
                            _farmerShadows.Add(f);
                        }
                    }
                }
                else
                {
                    foreach (Farmer f2 in Game1.currentLocation.farmers)
                    {
                        if ((f2.IsLocalPlayer && Game1.displayFarmer) || !f2.hidden)
                        {
                            _farmerShadows.Add(f2);
                        }
                    }
                }
                if (!Game1.currentLocation.shouldHideCharacters())
                {
                    if (Game1.CurrentEvent == null)
                    {
                        foreach (NPC n in Game1.currentLocation.characters)
                        {
                            if (!n.swimming && !n.HideShadow && !n.IsInvisible && !Game1.game1.checkCharacterTilesForShadowDrawFlag(n))
                            {
                                n.DrawShadow(Game1.spriteBatch);
                            }
                        }
                    }
                    else
                    {
                        foreach (NPC n2 in Game1.CurrentEvent.actors)
                        {
                            if ((Game1.CurrentEvent == null || !Game1.CurrentEvent.ShouldHideCharacter(n2)) && !n2.swimming && !n2.HideShadow && !Game1.game1.checkCharacterTilesForShadowDrawFlag(n2))
                            {
                                n2.DrawShadow(Game1.spriteBatch);
                            }
                        }
                    }
                    foreach (Farmer f3 in _farmerShadows)
                    {
                        if (!___multiplayer.isDisconnecting(f3.UniqueMultiplayerID) && !f3.swimming && !f3.isRidingHorse() && !f3.IsSitting() && (Game1.currentLocation == null || !Game1.game1.checkCharacterTilesForShadowDrawFlag(f3)))
                        {
                            f3.DrawShadow(Game1.spriteBatch);
                        }
                    }
                }
                float layer_sub_sort = 0.1f;
                for (int j = 0; j < Game1.currentLocation.buildingLayers.Count; j++)
                {
                    float layer = 0f;
                    if (Game1.currentLocation.buildingLayers.Count > 1)
                    {
                        layer = (float)j / (float)(Game1.currentLocation.buildingLayers.Count - 1);
                    }
                    Game1.currentLocation.buildingLayers[j].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, wrapAround: false, 4, layer_sub_sort * layer);
                }
                Layer building_layer = Game1.currentLocation.Map.GetLayer("Buildings");
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (!Game1.currentLocation.shouldHideCharacters())
                {
                    if (Game1.CurrentEvent == null)
                    {
                        foreach (NPC n3 in Game1.currentLocation.characters)
                        {
                            if (!n3.swimming && !n3.HideShadow && !n3.isInvisible && Game1.game1.checkCharacterTilesForShadowDrawFlag(n3))
                            {
                                n3.DrawShadow(Game1.spriteBatch);
                            }
                        }
                    }
                    else
                    {
                        foreach (NPC n4 in Game1.CurrentEvent.actors)
                        {
                            if ((Game1.CurrentEvent == null || !Game1.CurrentEvent.ShouldHideCharacter(n4)) && !n4.swimming && !n4.HideShadow && Game1.game1.checkCharacterTilesForShadowDrawFlag(n4))
                            {
                                n4.DrawShadow(Game1.spriteBatch);
                            }
                        }
                    }
                    foreach (Farmer f4 in _farmerShadows)
                    {
                        Math.Max(0.0001f, f4.getDrawLayer() + 0.00011f);
                        if (!f4.swimming && !f4.isRidingHorse() && !f4.IsSitting() && Game1.currentLocation != null && Game1.game1.checkCharacterTilesForShadowDrawFlag(f4))
                        {
                            f4.DrawShadow(Game1.spriteBatch);
                        }
                    }
                }
                if ((Game1.eventUp || Game1.killScreen) && !Game1.killScreen && Game1.currentLocation.currentEvent != null)
                {
                    Game1.currentLocation.currentEvent.draw(Game1.spriteBatch);
                }
                Game1.currentLocation.draw(Game1.spriteBatch);
                foreach (Vector2 tile_position in Game1.crabPotOverlayTiles.Keys)
                {
                    Tile tile = building_layer.Tiles[(int)tile_position.X, (int)tile_position.Y];
                    if (tile != null)
                    {
                        Vector2 vector_draw_position = Game1.GlobalToLocal(Game1.viewport, tile_position * 64f);
                        xTile.Dimensions.Location draw_location = new xTile.Dimensions.Location((int)vector_draw_position.X, (int)vector_draw_position.Y);
                        Game1.mapDisplayDevice.DrawTile(tile, draw_location, (tile_position.Y * 64f - 1f) / 10000f);
                    }
                }
                if (Game1.player.ActiveObject == null && (Game1.player.UsingTool || Game1.pickingTool) && Game1.player.CurrentTool != null && (!Game1.player.CurrentTool.Name.Equals("Seeds") || Game1.pickingTool))
                {
                    Game1.drawTool(Game1.player);
                }
                if (Game1.tvStation >= 0)
                {
                    Game1.spriteBatch.Draw(Game1.tvStationTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(400f, 160f)), new Microsoft.Xna.Framework.Rectangle(Game1.tvStation * 24, 0, 24, 15), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
                }
                if (Game1.panMode)
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)Math.Floor((double)(Game1.getOldMouseX() + Game1.viewport.X) / 64.0) * 64 - Game1.viewport.X, (int)Math.Floor((double)(Game1.getOldMouseY() + Game1.viewport.Y) / 64.0) * 64 - Game1.viewport.Y, 64, 64), Color.Lime * 0.75f);
                    foreach (Warp w in Game1.currentLocation.warps)
                    {
                        Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(w.X * 64 - Game1.viewport.X, w.Y * 64 - Game1.viewport.Y, 64, 64), Color.Red * 0.75f);
                    }
                }
                for (int l = 0; l < Game1.currentLocation.frontLayers.Count; l++)
                {
                    float layer2 = 0f;
                    if (Game1.currentLocation.frontLayers.Count > 1)
                    {
                        layer2 = (float)l / (float)(Game1.currentLocation.frontLayers.Count - 1);
                    }
                    Game1.currentLocation.frontLayers[l].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, wrapAround: false, 4, 64f + layer_sub_sort * layer2);
                }
                Game1.currentLocation.drawAboveFrontLayer(Game1.spriteBatch);
                Game1.spriteBatch.End();
                for (int m = 0; m < Game1.currentLocation.alwaysFrontLayers.Count; m++)
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);
                    Game1.currentLocation.alwaysFrontLayers[m].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, wrapAround: false, 4, -1f);
                    Game1.spriteBatch.End();
                }
                Game1.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (!Game1.IsFakedBlackScreen())
                {
                    Game1.game1.drawWeather(gameTime, target_screen);
                }
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (Game1.currentLocation.LightLevel > 0f && Game1.timeOfDay < 2000)
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * Game1.currentLocation.LightLevel);
                }
                if (Game1.screenGlow)
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Game1.screenGlowColor * Game1.screenGlowAlpha);
                }
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (Game1.toolHold > 400f && Game1.player.CurrentTool.UpgradeLevel >= 1 && Game1.player.canReleaseTool)
                {
                    Color barColor = Color.White;
                    switch ((int)(Game1.toolHold / 600f) + 2)
                    {
                        case 1:
                            barColor = Tool.copperColor;
                            break;
                        case 2:
                            barColor = Tool.steelColor;
                            break;
                        case 3:
                            barColor = Tool.goldColor;
                            break;
                        case 4:
                            barColor = Tool.iridiumColor;
                            break;
                    }
                    Game1.spriteBatch.Draw(Game1.littleEffect, new Microsoft.Xna.Framework.Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X - 2, (int)Game1.player.getLocalPosition(Game1.viewport).Y - ((!Game1.player.CurrentTool.Name.Equals("Watering Can")) ? 64 : 0) - 2, (int)(Game1.toolHold % 600f * 0.08f) + 4, 12), Color.Black);
                    Game1.spriteBatch.Draw(Game1.littleEffect, new Microsoft.Xna.Framework.Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X, (int)Game1.player.getLocalPosition(Game1.viewport).Y - ((!Game1.player.CurrentTool.Name.Equals("Watering Can")) ? 64 : 0), (int)(Game1.toolHold % 600f * 0.08f), 8), barColor);
                }
                Game1.currentLocation.drawAboveAlwaysFrontLayer(Game1.spriteBatch);
                if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is FishingRod && ((Game1.player.CurrentTool as FishingRod).isTimingCast || (Game1.player.CurrentTool as FishingRod).castingChosenCountdown > 0f || (Game1.player.CurrentTool as FishingRod).fishCaught || (Game1.player.CurrentTool as FishingRod).showingTreasure))
                {
                    Game1.player.CurrentTool.draw(Game1.spriteBatch);
                }
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
                if (Game1.eventUp && Game1.currentLocation.currentEvent != null)
                {
                    foreach (NPC n5 in Game1.currentLocation.currentEvent.actors)
                    {
                        if (n5.isEmoting)
                        {
                            Vector2 emotePosition = n5.getLocalPosition(Game1.viewport);
                            if (n5.NeedsBirdieEmoteHack())
                            {
                                emotePosition.X += 64f;
                            }
                            emotePosition.Y -= 140f;
                            if (n5.Age == 2)
                            {
                                emotePosition.Y += 32f;
                            }
                            else if (n5.Gender == 1)
                            {
                                emotePosition.Y += 10f;
                            }
                            Game1.spriteBatch.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(n5.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, n5.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)n5.getStandingY() / 10000f);
                        }
                    }
                }
                Game1.spriteBatch.End();
                Game1.mapDisplayDevice.EndScene();
                /*
                if (Game1.drawLighting && !Game1.IsFakedBlackScreen())
                {
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, Mod.instance.Helper.Reflection.GetField<BlendState>(Game1.game1, "lightingBlend").GetValue(), SamplerState.LinearClamp);
                    Viewport vp = graphicsDevice.Viewport;
                    vp.Bounds = target_screen?.Bounds ?? graphicsDevice.PresentationParameters.Bounds;
                    graphicsDevice.Viewport = vp;
                    float render_zoom = Game1.options.lightingQuality / 2;
                    if (Game1.game1.useUnscaledLighting)
                    {
                        render_zoom /= Game1.options.zoomLevel;
                    }
                    Game1.spriteBatch.Draw(Game1.lightmap, Vector2.Zero, Game1.lightmap.Bounds, Color.White, 0f, Vector2.Zero, render_zoom, SpriteEffects.None, 1f);
                    if (Game1.IsRainingHere() && (bool)Game1.currentLocation.isOutdoors)
                    {
                        Game1.spriteBatch.Draw(Game1.staminaRect, vp.Bounds, Color.OrangeRed * 0.45f);
                    }
                    Game1.spriteBatch.End();
                }*/
                if (Game1.drawLighting)
                    Mod.penumbra.Draw(gameTime);
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                //events.RenderedWorld.RaiseEmpty();
                if (Game1.drawGrid)
                {
                    int startingX = -Game1.viewport.X % 64;
                    float startingY = -Game1.viewport.Y % 64;
                    for (int x = startingX; x < Game1.graphics.GraphicsDevice.Viewport.Width; x += 64)
                    {
                        Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(x, (int)startingY, 1, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Red * 0.5f);
                    }
                    for (float y = startingY; y < (float)Game1.graphics.GraphicsDevice.Viewport.Height; y += 64f)
                    {
                        Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(startingX, (int)y, Game1.graphics.GraphicsDevice.Viewport.Width, 1), Color.Red * 0.5f);
                    }
                }
                if (Game1.ShouldShowOnscreenUsernames() && Game1.currentLocation != null)
                {
                    Game1.currentLocation.DrawFarmerUsernames(Game1.spriteBatch);
                }
                if (Game1.currentBillboard != 0 && !Game1.game1.takingMapScreenshot)
                {
                    Game1.game1.drawBillboard();
                }
                if (!Game1.eventUp && Game1.farmEvent == null && Game1.currentBillboard == 0 && Game1.gameMode == 3 && !Game1.game1.takingMapScreenshot && Game1.isOutdoorMapSmallerThanViewport())
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, -Game1.viewport.X, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(-Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64, 0, Game1.graphics.GraphicsDevice.Viewport.Width - (-Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64), Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, -Game1.viewport.Y), Color.Black);
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, -Game1.viewport.Y + Game1.currentLocation.map.Layers[0].LayerHeight * 64, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height - (-Game1.viewport.Y + Game1.currentLocation.map.Layers[0].LayerHeight * 64)), Color.Black);
                }
                Game1.spriteBatch.End();
                //*/
                Game1.PushUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                if ((Game1.displayHUD || Game1.eventUp) && Game1.currentBillboard == 0 && Game1.gameMode == 3 && !Game1.freezeControls && !Game1.panMode && !Game1.HostPaused && !Game1.game1.takingMapScreenshot)
                {
                    //events.RenderingHud.RaiseEmpty();
                    Mod.instance.Helper.Reflection.GetMethod(Game1.game1, "drawHUD").Invoke();
                    //events.RenderedHud.RaiseEmpty();
                }
                else if (Game1.activeClickableMenu == null)
                {
                    _ = Game1.farmEvent;
                }
                if (Game1.hudMessages.Count > 0 && !Game1.game1.takingMapScreenshot)
                {
                    for (int k = Game1.hudMessages.Count - 1; k >= 0; k--)
                    {
                        Game1.hudMessages[k].draw(Game1.spriteBatch, k);
                    }
                }
                Game1.spriteBatch.End();
                Game1.PopUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
            if (Game1.farmEvent != null)
            {
                Game1.farmEvent.draw(Game1.spriteBatch);
                Game1.spriteBatch.End();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
            Game1.PushUIMode();
            if (Game1.dialogueUp && !Game1.nameSelectUp && !Game1.messagePause && (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is DialogueBox)) && !Game1.game1.takingMapScreenshot)
            {
                Mod.instance.Helper.Reflection.GetMethod(Game1.game1, "drawDialogueBox").Invoke();
            }
            if (Game1.progressBar && !Game1.game1.takingMapScreenshot)
            {
                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Game1.dialogueWidth) / 2, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 128, Game1.dialogueWidth, 32), Color.LightGray);
                Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Game1.dialogueWidth) / 2, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 128, (int)(Game1.pauseAccumulator / Game1.pauseTime * (float)Game1.dialogueWidth), 32), Color.DimGray);
            }
            Game1.spriteBatch.End();
            Game1.PopUIMode();
            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            if (Game1.eventUp && Game1.currentLocation != null && Game1.currentLocation.currentEvent != null)
            {
                Game1.currentLocation.currentEvent.drawAfterMap(Game1.spriteBatch);
            }
            if (!Game1.IsFakedBlackScreen() && Game1.IsRainingHere() && Game1.currentLocation != null && (bool)Game1.currentLocation.isOutdoors)
            {
                Game1.spriteBatch.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Blue * 0.2f);
            }
            if ((Game1.fadeToBlack || Game1.globalFade) && !Game1.menuUp && (!Game1.nameSelectUp || Game1.messagePause) && !Game1.game1.takingMapScreenshot)
            {
                Game1.spriteBatch.End();
                Game1.PushUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((Game1.gameMode == 0) ? (1f - Game1.fadeToBlackAlpha) : Game1.fadeToBlackAlpha));
                Game1.spriteBatch.End();
                Game1.PopUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
            else if (Game1.flashAlpha > 0f && !Game1.game1.takingMapScreenshot)
            {
                if (Game1.options.screenFlash)
                {
                    Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.White * Math.Min(1f, Game1.flashAlpha));
                }
                Game1.flashAlpha -= 0.1f;
            }
            if ((Game1.messagePause || Game1.globalFade) && Game1.dialogueUp && !Game1.game1.takingMapScreenshot)
            {
                Mod.instance.Helper.Reflection.GetMethod(Game1.game1, "drawDialogueBox").Invoke();
            }
            if (!Game1.game1.takingMapScreenshot)
            {
                foreach (TemporaryAnimatedSprite screenOverlayTempSprite in Game1.screenOverlayTempSprites)
                {
                    screenOverlayTempSprite.draw(Game1.spriteBatch, localPosition: true);
                }
                Game1.spriteBatch.End();
                Game1.PushUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
                foreach (TemporaryAnimatedSprite uiOverlayTempSprite in Game1.uiOverlayTempSprites)
                {
                    uiOverlayTempSprite.draw(Game1.spriteBatch, localPosition: true);
                }
                Game1.spriteBatch.End();
                Game1.PopUIMode();
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
            /*
            if (Game1.debugMode)
            {
                StringBuilder sb = Game1._debugStringBuilder;
                sb.Clear();
                if (Game1.panMode)
                {
                    sb.Append((Game1.getOldMouseX() + Game1.viewport.X) / 64);
                    sb.Append(",");
                    sb.Append((Game1.getOldMouseY() + Game1.viewport.Y) / 64);
                }
                else
                {
                    sb.Append("player: ");
                    sb.Append(Game1.player.getStandingX() / 64);
                    sb.Append(", ");
                    sb.Append(Game1.player.getStandingY() / 64);
                }
                sb.Append(" mouseTransparency: ");
                sb.Append(Game1.mouseCursorTransparency);
                sb.Append(" mousePosition: ");
                sb.Append(Game1.getMouseX());
                sb.Append(",");
                sb.Append(Game1.getMouseY());
                sb.Append(Environment.NewLine);
                sb.Append(" mouseWorldPosition: ");
                sb.Append(Game1.getMouseX() + Game1.viewport.X);
                sb.Append(",");
                sb.Append(Game1.getMouseY() + Game1.viewport.Y);
                sb.Append("  debugOutput: ");
                sb.Append(Game1.debugOutput);
                Game1.spriteBatch.DrawString(Game1.smallFont, sb, new Vector2(graphicsDevice.Viewport.GetTitleSafeArea().X, base.GraphicsDevice.Viewport.GetTitleSafeArea().Y + Game1.smallFont.LineSpacing * 8), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
            }
            */
            Game1.spriteBatch.End();
            Game1.PushUIMode();
            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            if (Game1.showKeyHelp && !Game1.game1.takingMapScreenshot)
            {
                Game1.spriteBatch.DrawString(Game1.smallFont, Game1.keyHelpString, new Vector2(64f, (float)(Game1.viewport.Height - 64 - (Game1.dialogueUp ? (192 + (Game1.isQuestion ? (Game1.questionChoices.Count * 64) : 0)) : 0)) - Game1.smallFont.MeasureString(Game1.keyHelpString).Y), Color.LightGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
            }
            if (Game1.activeClickableMenu != null && !Game1.game1.takingMapScreenshot)
            {
                IClickableMenu curMenu = null;
                try
                {
                    //events.RenderingActiveMenu.RaiseEmpty();
                    for (curMenu = Game1.activeClickableMenu; curMenu != null; curMenu = curMenu.GetChildMenu())
                    {
                        curMenu.draw(Game1.spriteBatch);
                    }
                    //events.RenderedActiveMenu.RaiseEmpty();
                }
                catch (Exception ex)
                {
                    ___Monitor.Log($"The {curMenu.GetMenuChainLabel()} menu crashed while drawing itself. SMAPI will force it to exit to avoid crashing the game.\n{ex.GetLogSummary()}", LogLevel.Error);
                    Game1.activeClickableMenu.exitThisMenu();
                }
            }
            else if (Game1.farmEvent != null)
            {
                Game1.farmEvent.drawAboveEverything(Game1.spriteBatch);
            }
            if (Game1.specialCurrencyDisplay != null)
            {
                Game1.specialCurrencyDisplay.Draw(Game1.spriteBatch);
            }
            if (Game1.emoteMenu != null && !Game1.game1.takingMapScreenshot)
            {
                Game1.emoteMenu.draw(Game1.spriteBatch);
            }
            if (Game1.HostPaused && !Game1.game1.takingMapScreenshot)
            {
                string msg2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10378");
                SpriteText.drawStringWithScrollBackground(Game1.spriteBatch, msg2, 96, 32);
            }
            //events.Rendered.RaiseEmpty();
            Game1.spriteBatch.End();
            AccessTools.Method(Game1.game1.GetType(), "drawOverlays").Invoke(Game1.game1, new object[] { Game1.spriteBatch });
            Game1.PopUIMode();
        }
    }
}

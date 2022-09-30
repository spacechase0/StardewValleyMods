using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ContentPatcherEditor
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static ImGuiRenderer imgui { get; private set; }
        public static RenderTarget2D imguiTarget { get; private set; }

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            imgui = new(GameRunner.instance);
            imgui.RebuildFontAtlas();

            Helper.Events.Display.Rendering += this.Display_Rendering;
            Helper.Events.Display.Rendered += this.Display_Rendered;

            Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(AccessTools.Method("StardewModdingAPI.Framework.Input.SInputState:GetMouseState"),
                prefix: new HarmonyMethod(this.GetType().GetMethod("MouseStatePatch", BindingFlags.Public | BindingFlags.Static)));
            harmony.Patch(AccessTools.Method("StardewModdingAPI.Framework.Input.SInputState:GetKeyboardState"),
                prefix: new HarmonyMethod(this.GetType().GetMethod("KeyboardStatePatch", BindingFlags.Public | BindingFlags.Static)));
        }

        public static bool MouseStatePatch(ref MouseState __result)
        {
            if (ImGui.GetIO().WantCaptureMouse)
            {
                __result = new MouseState(Mouse.GetState().X, Mouse.GetState().Y, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
                return false;
            }
            return true;
        }

        public static bool KeyboardStatePatch(ref KeyboardState __result)
        {
            if (ImGui.GetIO().WantCaptureKeyboard)
            {
                __result = default(KeyboardState);
                return false;
            }
            return true;
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (ImGui.GetIO().WantCaptureMouse)
                GameRunner.instance.IsMouseVisible = true;
            else
                GameRunner.instance.IsMouseVisible = Game1.options.hardwareCursor;
        }

        private void Display_Rendering(object sender, RenderingEventArgs e)
        {
            imgui.BeforeLayout(Game1.currentGameTime);
            /*
            ImGui.Begin("test");
            ImGui.Text("Meow");
            string str = "";
            ImGui.InputText("MEOW", ref str, 100);
            ImGui.End();
            */
        }

        [EventPriority(EventPriority.Low)]
        private void Display_Rendered(object sender, RenderedEventArgs e)
        {
            if (imguiTarget == null || imguiTarget.Width != Game1.graphics.PreferredBackBufferWidth || imguiTarget.Height != Game1.graphics.PreferredBackBufferHeight)
            {
                imguiTarget?.Dispose();
                imguiTarget = new RenderTarget2D(Game1.graphics.GraphicsDevice, Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight);
            }

            var oldTargets = Game1.graphics.GraphicsDevice.GetRenderTargets();
            Game1.graphics.GraphicsDevice.SetRenderTarget(imguiTarget);
            Game1.graphics.GraphicsDevice.Clear(Color.Transparent);
            imgui.AfterLayout();
            Game1.graphics.GraphicsDevice.SetRenderTargets(oldTargets);
            e.SpriteBatch.Draw(imguiTarget, Vector2.Zero, Color.White);
        }
    }
}

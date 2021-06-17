using System;
using System.Collections.Generic;
using GenericModConfigMenu.Framework;
using GenericModConfigMenu.Framework.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace GenericModConfigMenu
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        private RootElement Ui;
        private Button ConfigButton;
        internal Dictionary<IManifest, ModConfig> Configs = new();

        public override void Entry(IModHelper helper)
        {
            this.Monitor.Log($"GMCM version {ModManifest.Version} loading...", LogLevel.Info);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            Api.ApiLog = Monitor.Log;

            this.Ui = new RootElement();

            Texture2D tex = this.Helper.Content.Load<Texture2D>("assets/config-button.png");
            this.ConfigButton = new Button(tex)
            {
                LocalPosition = new Vector2(36, Game1.viewport.Height - 100),
                Callback = e =>
                {
                    Game1.playSound("newArtifact");
                    TitleMenu.subMenu = new ModConfigMenu(false);
                }
            };

            this.Ui.AddChild(this.ConfigButton);

            helper.Events.GameLoop.GameLaunched += this.OnLaunched;
            helper.Events.GameLoop.UpdateTicking += this.OnUpdate;
            helper.Events.Display.WindowResized += this.OnWindowResized;
            helper.Events.Display.Rendered += this.OnRendered;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
        }


        public override object GetApi()
        {
            return new Api();
        }


        public class DummyConfig
        {
            public bool DummyBool = true;
            public int DummyInt1 = 50;
            public int DummyInt2 = 50;
            public float DummyFloat1 = 0.5f;
            public float DummyFloat2 = 0.5f;
            public string DummyString1 = "Kirby";
            public string DummyString2 = "Default";
            internal static string[] DummyString2Choices = new[] { "Default", "Kitties", "Cats", "Meow" };
            public SButton DummyKeybinding = SButton.K;
            public KeybindList DummyKeybinding2 = new(new Keybind(SButton.LeftShift, SButton.S));
            public Color DummyColor = Color.White;
        }
        public DummyConfig Config;

        public class RandomColorWidgetState
        {
            public Color Color;
        }

        private void OnLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.Config = this.Helper.ReadConfig<DummyConfig>();
            var api = this.Helper.ModRegistry.GetApi<IApi>(this.ModManifest.UniqueID);
            api.RegisterModConfig(this.ModManifest, () => this.Config = new DummyConfig(), () => this.Helper.WriteConfig(this.Config));
            api.SetDefaultIngameOptinValue(this.ModManifest, true);
            api.RegisterLabel(this.ModManifest, "Dummy Label", "Testing labels");
            api.RegisterParagraph(this.ModManifest, "Testing paragraph text. These are smaller than labels and should wrap based on length. In theory. They should also (in theory) support multiple rows. Whether that will look good or not is another question. But hey, it looks like it worked! Imagine that. Should I support images in documentation, too?");
            api.RegisterImage(this.ModManifest, "Maps\\springobjects", new Rectangle(32, 48, 16, 16));
            api.RegisterImage(this.ModManifest, "Portraits\\Penny", null, 1);
            api.SetDefaultIngameOptinValue(this.ModManifest, false);
            api.RegisterPageLabel(this.ModManifest, "Go to page: Simple Options", "", "Simple Options");
            api.SetDefaultIngameOptinValue(this.ModManifest, true);
            api.RegisterPageLabel(this.ModManifest, "Go to page: Complex Options", "", "Complex Options");
            api.SetDefaultIngameOptinValue(this.ModManifest, false);
            api.StartNewPage(this.ModManifest, "Simple Options");
            api.RegisterPageLabel(this.ModManifest, "Back to main page", "", "");
            api.RegisterSimpleOption(this.ModManifest, "Dummy Bool", "Testing a checkbox", () => this.Config.DummyBool, (bool val) => this.Config.DummyBool = val);
            api.RegisterSimpleOption(this.ModManifest, "Dummy Int (1)", "Testing an int (simple)", () => this.Config.DummyInt1, (int val) => this.Config.DummyInt1 = val);
            api.RegisterClampedOption(this.ModManifest, "Dummy Int (2)", "Testing an int (range)", () => this.Config.DummyInt2, (int val) => this.Config.DummyInt2 = val, 0, 100);
            api.RegisterSimpleOption(this.ModManifest, "Dummy Float (1)", "Testing a float (simple)", () => this.Config.DummyFloat1, (float val) => this.Config.DummyFloat1 = val);
            api.RegisterClampedOption(this.ModManifest, "Dummy Float (2)", "Testing a float (range)", () => this.Config.DummyFloat2, (float val) => this.Config.DummyFloat2 = val, 0, 1);
            api.SetDefaultIngameOptinValue(this.ModManifest, true);
            api.StartNewPage(this.ModManifest, "Complex Options");
            api.RegisterPageLabel(this.ModManifest, "Back to main page", "", "");
            api.RegisterSimpleOption(this.ModManifest, "Dummy String (1)", "Testing a string", () => this.Config.DummyString1, (string val) => this.Config.DummyString1 = val);
            api.RegisterChoiceOption(this.ModManifest, "Dummy String (2)", "Testing a dropdown box", () => this.Config.DummyString2, (string val) => this.Config.DummyString2 = val, DummyConfig.DummyString2Choices);
            api.RegisterSimpleOption(this.ModManifest, "Dummy Keybinding", "Testing a keybinding", () => this.Config.DummyKeybinding, (SButton val) => this.Config.DummyKeybinding = val);
            api.RegisterSimpleOption(this.ModManifest, "Dummy Keybinding 2", "Testing a keybinding list", () => this.Config.DummyKeybinding2, (KeybindList val) => this.Config.DummyKeybinding2 = val);

            api.RegisterLabel(this.ModManifest, "", "");

            // Complex widget - this just generates a random  color on click.
            object RandomColorUpdate(Vector2 pos, object rawState)
            {
                var state = rawState as RandomColorWidgetState ?? new RandomColorWidgetState { Color = this.Config.DummyColor };

                var bounds = new Rectangle((int)pos.X + 12, (int)pos.Y + 12, 50 - 12 * 2, 50 - 12 * 2);
                bool hover = bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY());
                if (hover && Game1.oldMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    Game1.playSound("drumkit6");
                    Random r = new Random();
                    state.Color.R = (byte)r.Next(256);
                    state.Color.G = (byte)r.Next(256);
                    state.Color.B = (byte)r.Next(256);
                }

                return state;
            }

            object RandomColorDraw(SpriteBatch b, Vector2 pos, object rawState)
            {
                var state = rawState as RandomColorWidgetState;
                IClickableMenu.drawTextureBox(b, (int)pos.X, (int)pos.Y, 50, 50, Color.White);
                var colorBox = new Rectangle((int)pos.X + 12, (int)pos.Y + 12, 50 - 12 * 2, 50 - 12 * 2);
                b.Draw(Game1.staminaRect, colorBox, state.Color);
                return state;
            }

            void RandomColorSave(object state)
            {
                if (state == null) return;
                this.Config.DummyColor = (state as RandomColorWidgetState).Color;
            }

            api.RegisterComplexOption(this.ModManifest, "Dummy Color", "Testing a complex widget (random color on click)", RandomColorUpdate, RandomColorDraw, RandomColorSave);
        }

        private bool IsTitleMenuInteractable()
        {
            if (!(Game1.activeClickableMenu is TitleMenu tm))
                return false;
            if (TitleMenu.subMenu != null)
                return false;

            var method = this.Helper.Reflection.GetMethod(tm, "ShouldAllowInteraction", false);
            if (method != null)
                return method.Invoke<bool>();
            else // method isn't available on Android
                return this.Helper.Reflection.GetField<bool>(tm, "titleInPosition").GetValue();
        }

        private void OnUpdate(object sender, UpdateTickingEventArgs e)
        {
            if (this.IsTitleMenuInteractable())
                this.Ui.Update();
        }

        private void OnWindowResized(object sender, WindowResizedEventArgs e)
        {
            this.ConfigButton.LocalPosition = new Vector2(this.ConfigButton.Position.X, Game1.viewport.Height - 100);
        }

        private void OnRendered(object sender, RenderedEventArgs e)
        {
            if (this.IsTitleMenuInteractable())
                this.Ui.Draw(e.SpriteBatch);
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is GameMenu gm)
            {
                (gm.pages[GameMenu.optionsTab] as OptionsPage).options.Add(new OptionsButton("Mod Options", () =>
                {
                    Game1.activeClickableMenu = new ModConfigMenu(true);
                }));
            }
        }

        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            Dropdown.ActiveDropdown?.ReceiveScrollWheelAction(e.Delta);
            if (ModConfigMenu.ActiveConfigMenu is ModConfigMenu mcm)
                mcm.ReceiveScrollWheelActionSmapi(e.Delta);
            if (SpecificModConfigMenu.ActiveConfigMenu is SpecificModConfigMenu smcm)
                smcm.ReceiveScrollWheelActionSmapi(e.Delta);
        }
    }
}

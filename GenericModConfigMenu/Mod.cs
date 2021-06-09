using System;
using System.Collections.Generic;
using GenericModConfigMenu.UI;
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
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private RootElement ui;
        private Button configButton;
        internal Dictionary<IManifest, ModConfig> configs = new Dictionary<IManifest, ModConfig>();

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            this.ui = new RootElement();

            Texture2D tex = this.Helper.Content.Load<Texture2D>("assets/config-button.png");
            this.configButton = new Button(tex);
            this.configButton.LocalPosition = new Vector2(36, Game1.viewport.Height - 100);
            this.configButton.Callback = (Element e) =>
            {
                Game1.playSound("newArtifact");
                TitleMenu.subMenu = new ModConfigMenu(false);
            };

            this.ui.AddChild(this.configButton);

            helper.Events.GameLoop.GameLaunched += this.onLaunched;
            helper.Events.GameLoop.UpdateTicking += this.onUpdate;
            helper.Events.Display.WindowResized += this.onWindowResized;
            helper.Events.Display.Rendered += this.onRendered;
            helper.Events.Display.MenuChanged += this.onMenuChanged;
            helper.Events.Input.MouseWheelScrolled += this.onMouseWheelScrolled;
        }


        public override object GetApi()
        {
            return new Api();
        }


        public class DummyConfig
        {
            public bool dummyBool = true;
            public int dummyInt1 = 50;
            public int dummyInt2 = 50;
            public float dummyFloat1 = 0.5f;
            public float dummyFloat2 = 0.5f;
            public string dummyString1 = "Kirby";
            public string dummyString2 = "Default";
            internal static string[] dummyString2Choices = new string[] { "Default", "Kitties", "Cats", "Meow" };
            public SButton dummyKeybinding = SButton.K;
            public KeybindList dummyKeybinding2 = new KeybindList(new Keybind(SButton.LeftShift, SButton.S));
            public Color dummyColor = Color.White;
        }
        public DummyConfig config;

        public class RandomColorWidgetState
        {
            public Color color;
        }

        private void onLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.config = this.Helper.ReadConfig<DummyConfig>();
            var api = this.Helper.ModRegistry.GetApi<IApi>(this.ModManifest.UniqueID);
            api.RegisterModConfig(this.ModManifest, () => this.config = new DummyConfig(), () => this.Helper.WriteConfig(this.config));
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
            api.RegisterSimpleOption(this.ModManifest, "Dummy Bool", "Testing a checkbox", () => this.config.dummyBool, (bool val) => this.config.dummyBool = val);
            api.RegisterSimpleOption(this.ModManifest, "Dummy Int (1)", "Testing an int (simple)", () => this.config.dummyInt1, (int val) => this.config.dummyInt1 = val);
            api.RegisterClampedOption(this.ModManifest, "Dummy Int (2)", "Testing an int (range)", () => this.config.dummyInt2, (int val) => this.config.dummyInt2 = val, 0, 100);
            api.RegisterSimpleOption(this.ModManifest, "Dummy Float (1)", "Testing a float (simple)", () => this.config.dummyFloat1, (float val) => this.config.dummyFloat1 = val);
            api.RegisterClampedOption(this.ModManifest, "Dummy Float (2)", "Testing a float (range)", () => this.config.dummyFloat2, (float val) => this.config.dummyFloat2 = val, 0, 1);
            api.SetDefaultIngameOptinValue(this.ModManifest, true);
            api.StartNewPage(this.ModManifest, "Complex Options");
            api.RegisterPageLabel(this.ModManifest, "Back to main page", "", "");
            api.RegisterSimpleOption(this.ModManifest, "Dummy String (1)", "Testing a string", () => this.config.dummyString1, (string val) => this.config.dummyString1 = val);
            api.RegisterChoiceOption(this.ModManifest, "Dummy String (2)", "Testing a dropdown box", () => this.config.dummyString2, (string val) => this.config.dummyString2 = val, DummyConfig.dummyString2Choices);
            api.RegisterSimpleOption(this.ModManifest, "Dummy Keybinding", "Testing a keybinding", () => this.config.dummyKeybinding, (SButton val) => this.config.dummyKeybinding = val);
            api.RegisterSimpleOption(this.ModManifest, "Dummy Keybinding 2", "Testing a keybinding list", () => this.config.dummyKeybinding2, (KeybindList val) => this.config.dummyKeybinding2 = val);

            api.RegisterLabel(this.ModManifest, "", "");

            // Complex widget - this just generates a random  color on click.
            Func<Vector2, object, object> randomColorUpdate =
            (Vector2 pos, object state_) =>
            {
                var state = state_ as RandomColorWidgetState;
                if (state == null)
                    state = new RandomColorWidgetState() { color = this.config.dummyColor };

                var bounds = new Rectangle((int)pos.X + 12, (int)pos.Y + 12, 50 - 12 * 2, 50 - 12 * 2);
                bool hover = bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY());
                if (hover && Game1.oldMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    Game1.playSound("drumkit6");
                    Random r = new Random();
                    state.color.R = (byte)r.Next(256);
                    state.color.G = (byte)r.Next(256);
                    state.color.B = (byte)r.Next(256);
                }

                return state;
            };
            Func<SpriteBatch, Vector2, object, object> randomColorDraw =
            (SpriteBatch b, Vector2 pos, object state_) =>
            {
                var state = state_ as RandomColorWidgetState;
                IClickableMenu.drawTextureBox(b, (int)pos.X, (int)pos.Y, 50, 50, Color.White);
                var colorBox = new Rectangle((int)pos.X + 12, (int)pos.Y + 12, 50 - 12 * 2, 50 - 12 * 2);
                b.Draw(Game1.staminaRect, colorBox, state.color);
                return state;
            };
            Action<object> randomColorSave =
            (object state) =>
            {
                if (state == null)
                    return;
                this.config.dummyColor = (state as RandomColorWidgetState).color;
            };
            api.RegisterComplexOption(this.ModManifest, "Dummy Color", "Testing a complex widget (random color on click)", randomColorUpdate, randomColorDraw, randomColorSave);
        }

        private bool isTitleMenuInteractable()
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

        private void onUpdate(object sender, UpdateTickingEventArgs e)
        {
            if (this.isTitleMenuInteractable())
                this.ui.Update();
        }

        private void onWindowResized(object sender, WindowResizedEventArgs e)
        {
            this.configButton.LocalPosition = new Vector2(this.configButton.Position.X, Game1.viewport.Height - 100);
        }

        private void onRendered(object sender, RenderedEventArgs e)
        {
            if (this.isTitleMenuInteractable())
                this.ui.Draw(e.SpriteBatch);
        }

        private void onMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is GameMenu gm)
            {
                (gm.pages[GameMenu.optionsTab] as OptionsPage).options.Add(new OptionsButton("Mod Options", () =>
                {
                    Game1.activeClickableMenu = new ModConfigMenu(true);
                }));
            }
        }

        private void onMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            Dropdown.ActiveDropdown?.receiveScrollWheelAction(e.Delta);
            if (ModConfigMenu.ActiveConfigMenu is ModConfigMenu mcm)
                mcm.receiveScrollWheelActionSmapi(e.Delta);
            if (SpecificModConfigMenu.ActiveConfigMenu is SpecificModConfigMenu smcm)
                smcm.receiveScrollWheelActionSmapi(e.Delta);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericModConfigMenu.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace GenericModConfigMenu
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private Button configButton;
        internal Dictionary<IManifest, ModConfig> configs = new Dictionary<IManifest, ModConfig>();

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            Texture2D tex = Helper.Content.Load<Texture2D>("assets/config-button.png");
            configButton = new Button(tex);
            configButton.LocalPosition = new Vector2(36, Game1.viewport.Height - 100);
            configButton.Callback = (Element e) => TitleMenu.subMenu = new ModConfigMenu();

            helper.Events.GameLoop.GameLaunched += onLaunched;
            helper.Events.GameLoop.UpdateTicking += onUpdate;
            helper.Events.Display.WindowResized += onWindowResized;
            helper.Events.Display.Rendered += onRendered;
            helper.Events.Input.MouseWheelScrolled += onMouseWheelScrolled;
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
            public Color dummyColor = Color.White;
        }
        public DummyConfig config;

        public class RandomColorWidgetState
        {
            public Color color;
        }

        private void onLaunched(object sender, GameLaunchedEventArgs e)
        {
            config = Helper.ReadConfig<DummyConfig>();
            var api = Helper.ModRegistry.GetApi<IApi>(ModManifest.UniqueID);
            api.RegisterModConfig(ModManifest, () => config = new DummyConfig(), () => Helper.WriteConfig(config));
            api.RegisterLabel(ModManifest, "Dummy Label", "Testing labels");
            api.RegisterSimpleOption(ModManifest, "Dummy Bool", "Testing a checkbox", () => config.dummyBool, (bool val) => config.dummyBool = val);
            api.RegisterSimpleOption(ModManifest, "Dummy Int (1)", "Testing an int (simple)", () => config.dummyInt1, (int val) => config.dummyInt1 = val);
            api.RegisterClampedOption(ModManifest, "Dummy Int (2)", "Testing an int (range)", () => config.dummyInt2, (int val) => config.dummyInt2 = val, 0, 100);
            api.RegisterSimpleOption(ModManifest, "Dummy Float (1)", "Testing a float (simple)", () => config.dummyFloat1, (float val) => config.dummyFloat1 = val);
            api.RegisterClampedOption(ModManifest, "Dummy Float (2)", "Testing a float (range)", () => config.dummyFloat2, (float val) => config.dummyFloat2 = val, 0, 1);
            api.RegisterSimpleOption(ModManifest, "Dummy String (1)", "Testing a string", () => config.dummyString1, (string val) => config.dummyString1 = val);
            api.RegisterChoiceOption(ModManifest, "Dummy String (2)", "Testing a dropdown box", () => config.dummyString2, (string val) => config.dummyString2 = val, DummyConfig.dummyString2Choices);
            api.RegisterSimpleOption(ModManifest, "Dummy Keybinding", "Testing a keybinding", () => config.dummyKeybinding, (SButton val) => config.dummyKeybinding = val);

            api.RegisterLabel(ModManifest, "", "");

            // Complex widget - this just generates a random  color on click.
            Func<Vector2, object, object> randomColorUpdate =
            (Vector2 pos, object state_) =>
            {
                var state = state_ as RandomColorWidgetState;
                if (state == null)
                    state = new RandomColorWidgetState() { color = config.dummyColor };

                var bounds = new Rectangle((int)pos.X + 12, (int)pos.Y + 12, 50 - 12 * 2, 50 - 12 * 2);
                bool hover = bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY());
                if ( hover && Game1.oldMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    Random r = new Random();
                    state.color.R = (byte) r.Next(256);
                    state.color.G = (byte) r.Next(256);
                    state.color.B = (byte) r.Next(256);
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
            (object state) => config.dummyColor = (state as RandomColorWidgetState).color;
            api.RegisterComplexOption(ModManifest, "Dummy Color", "Testing a complex widget (random color on click)", randomColorUpdate, randomColorDraw, randomColorSave);
        }

        private void onUpdate(object sender, UpdateTickingEventArgs e)
        {
            if (Game1.activeClickableMenu is TitleMenu tm)
            {
                if (TitleMenu.subMenu == null && Helper.Reflection.GetField<bool>(tm, "titleInPosition").GetValue())
                    configButton.Update();
            }
        }

        private void onWindowResized(object sender, WindowResizedEventArgs e)
        {
            configButton.LocalPosition = new Vector2(configButton.Position.X, Game1.viewport.Height - 100);
        }

        private void onRendered(object sender, RenderedEventArgs e)
        {
            if ( Game1.activeClickableMenu is TitleMenu tm )
            {
                if (TitleMenu.subMenu == null && Helper.Reflection.GetField<bool>(tm, "titleInPosition").GetValue())
                    configButton.Draw(e.SpriteBatch);
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

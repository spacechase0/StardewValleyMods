using System.Collections.Generic;
using GenericModConfigMenu.Framework;
using GenericModConfigMenu.Framework.UI;
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
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        private OwnModConfig Config;
        private RootElement Ui;
        private Button ConfigButton;


        /*********
        ** Accessors
        *********/
        public static Mod Instance;
        internal Dictionary<IManifest, ModConfig> Configs = new();


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            this.Config = helper.ReadConfig<OwnModConfig>();

            this.SetupTitleMenuButton();

            // helper.Events.GameLoop.GameLaunched += this.OnLaunched;
            helper.Events.GameLoop.UpdateTicking += this.OnUpdate;
            helper.Events.Display.WindowResized += this.OnWindowResized;
            helper.Events.Display.Rendered += this.OnRendered;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return new Api();
        }

        /// <summary>Open the config UI for a mod.</summary>
        /// <param name="mod">The mod whose config menu to display.</param>
        public void OpenMenu(IManifest mod)
        {
            if (Game1.activeClickableMenu is TitleMenu)
                TitleMenu.subMenu = new SpecificModConfigMenu(mod, inGame: false, scrollSpeed: this.Config.ScrollSpeed);
            else
                Game1.activeClickableMenu = new SpecificModConfigMenu(mod, inGame: false, scrollSpeed: this.Config.ScrollSpeed);
        }


        /*********
        ** Private methods
        *********/
        private void SetupTitleMenuButton()
        {
            this.Ui = new RootElement();

            Texture2D tex = this.Helper.Content.Load<Texture2D>("assets/config-button.png");
            this.ConfigButton = new Button(tex)
            {
                LocalPosition = new Vector2(36, Game1.viewport.Height - 100),
                Callback = _ =>
                {
                    Game1.playSound("newArtifact");
                    TitleMenu.subMenu = new ModConfigMenu(false, this.Config.ScrollSpeed);
                }
            };

            this.Ui.AddChild(this.ConfigButton);
        }

        private bool IsTitleMenuInteractable()
        {
            if (Game1.activeClickableMenu is not TitleMenu tm || TitleMenu.subMenu != null)
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
            if (e.NewMenu is GameMenu menu)
            {
                OptionsPage page = (OptionsPage)menu.pages[GameMenu.optionsTab];
                page.options.Add(new OptionsButton("Mod Options", () =>
                    Game1.activeClickableMenu = new ModConfigMenu(true, this.Config.ScrollSpeed)
                ));
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (SpecificModConfigMenu.ActiveConfigMenu is SpecificModConfigMenu menu && e.Button.TryGetKeyboard(out Keys key))
                menu.receiveKeyPress(key);
        }

        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            Dropdown.ActiveDropdown?.ReceiveScrollWheelAction(e.Delta);

            if (ModConfigMenu.ActiveConfigMenu is ModConfigMenu modConfigMenu)
                modConfigMenu.ReceiveScrollWheelActionSmapi(e.Delta);

            if (SpecificModConfigMenu.ActiveConfigMenu is SpecificModConfigMenu specificConfigMenu)
                specificConfigMenu.ReceiveScrollWheelActionSmapi(e.Delta);
        }
    }
}

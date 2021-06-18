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
    public class GMCMConfig
        {
        public int ScrollSpeed { get; set; } = 120;
        }

    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static GMCMConfig Config;

        private RootElement Ui;
        private Button ConfigButton;
        internal Dictionary<IManifest, ModConfig> Configs = new();

        public override void Entry(IModHelper helper)
        {
            this.Monitor.Log($"GMCM version {ModManifest.Version} loading...", LogLevel.Info);
            Mod.Instance = this;
            Mod.Config = helper.ReadConfig<GMCMConfig>();
            Log.Monitor = this.Monitor;

            this.SetupTitleMenuButton();

            // helper.Events.GameLoop.GameLaunched += this.OnLaunched;
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

        public class RandomColorWidgetState
        {
            public Color Color;
        }

        private void SetupTitleMenuButton() {
            this.Ui = new RootElement();

            Texture2D tex = this.Helper.Content.Load<Texture2D>("assets/config-button.png");
            this.ConfigButton = new Button(tex) {
                LocalPosition = new Vector2(36, Game1.viewport.Height - 100),
                Callback = e => {
                    Game1.playSound("newArtifact");
                    TitleMenu.subMenu = new ModConfigMenu(false);
                }
                };

            this.Ui.AddChild(this.ConfigButton);
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

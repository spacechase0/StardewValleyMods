using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ObjectTimeLeft.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SObject = StardewValley.Object;

namespace ObjectTimeLeft
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static Configuration Config;

        private bool Showing;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();
            this.Showing = Mod.Config.ShowOnStart;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.RenderingHud += this.OnRenderingHud;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => Mod.Config = new Configuration(),
                    save: () => this.Helper.WriteConfig(Mod.Config)
                );
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: I18n.Config_ShowOnStart_Name,
                    tooltip: I18n.Config_ShowOnStart_Tooltip,
                    getValue: () => Mod.Config.ShowOnStart,
                    setValue: value => Mod.Config.ShowOnStart = value
                );
                configMenu.AddKeybind(
                    mod: this.ModManifest,
                    name: I18n.Config_ToggleKey_Name,
                    tooltip: I18n.Config_ShowOnStart_Tooltip,
                    getValue: () => Mod.Config.ToggleKey,
                    setValue: value => Mod.Config.ToggleKey = value
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: I18n.Config_TextScale_Name,
                    tooltip: I18n.Config_TextScale_Tooltip,
                    getValue: () => Mod.Config.TextScale,
                    setValue: value => Mod.Config.TextScale = value
                );
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == Mod.Config.ToggleKey)
                this.Showing = !this.Showing;
        }

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            if (!this.Showing || !Context.IsPlayerFree)
                return;

            Color shadowColor = Color.Black * 0.5f;
            float zoomLevel = this.GetZoomLevel();
            float scale = zoomLevel * Mod.Config.TextScale;

            void DrawString(string str, Vector2 position, Color color)
            {
                e.SpriteBatch.DrawString(
                    spriteFont: Game1.dialogueFont,
                    text: str,
                    position: position,
                    color: color,
                    rotation: 0.0f,
                    origin: Vector2.Zero,
                    scale: scale,
                    effects: SpriteEffects.None,
                    layerDepth: 0.0f
                );
            }

            foreach (var pair in Game1.currentLocation.Objects.Pairs)
            {
                SObject obj = pair.Value;
                if (obj.MinutesUntilReady is <= 0 or 999999 || obj.Name == "Stone")
                    continue;

                string text = (obj.MinutesUntilReady / 10).ToString();
                Vector2 pos = this.GetTimeLeftPosition(pair.Key, text);

                // draw text outline for contrast
                void DrawOutline(int offsetX, int offsetY)
                {
                    Vector2 offset = this.ModifyCoordinatesForUiScale(new Vector2(offsetX, offsetY));
                    DrawString(text, pos + offset, shadowColor);
                }
                DrawOutline(0, 3);
                DrawOutline(3, 0);
                DrawOutline(0, -3);
                DrawOutline(-3, 0);

                // draw text
                DrawString(text, pos, Color.White);
            }
        }

        /// <summary>Get the position at which to draw the given text for a machine.</summary>
        /// <param name="tile">The tile position containing the machine.</param>
        /// <param name="text">The text to draw over the machine.</param>
        private Vector2 GetTimeLeftPosition(Vector2 tile, string text)
        {
            // get screen pixel position
            Vector2 pos = Game1.GlobalToLocal(
                Game1.uiViewport,
                new Vector2(x: tile.X * Game1.tileSize, y: tile.Y * Game1.tileSize)
            );

            // center text over tile
            float textWidth = Game1.dialogueFont.MeasureString(text).X;
            pos.X += (Game1.tileSize - textWidth) / 2;

            // apply zoom level
            return this.ModifyCoordinatesForUiScale(pos);
        }

        /// <summary>Apply zoom and UI scaling to a set of coordinates.</summary>
        /// <param name="coordinates">The coordinates to adjust.</param>
        private Vector2 ModifyCoordinatesForUiScale(Vector2 coordinates)
        {
            if (Constants.TargetPlatform == GamePlatform.Android)
                return coordinates * this.GetZoomLevel();

            return Utility.ModifyCoordinatesForUIScale(coordinates);
        }

        /// <summary>Get the game's current zoom level.</summary>
        private float GetZoomLevel()
        {
            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                var options = this.Helper.Reflection.GetField<Options>(typeof(Game1), nameof(Game1.options)).GetValue();
                return options.zoomLevel;
            }

            return Game1.options.zoomLevel;
        }
    }
}

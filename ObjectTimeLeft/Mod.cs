using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ObjectTimeLeft.Framework;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ObjectTimeLeft
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static Configuration Config;

        private bool Showing = true;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.RenderingHud += this.OnRenderingHud;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(this.ModManifest, () => Mod.Config = new Configuration(), () => this.Helper.WriteConfig(Mod.Config));
                capi.RegisterSimpleOption(this.ModManifest, "Key: Toggle Display", "The key to toggle the display on objects.", () => Mod.Config.ToggleKey, (SButton val) => Mod.Config.ToggleKey = val);
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

            var sb = e.SpriteBatch;
            Color half_blk = Color.Black * 0.5f;
            float zoom = this.GetOptions().zoomLevel;

            void DrawString(string str, Vector2 vec, Color clr) {
                sb.DrawString(
                    spriteFont: Game1.dialogueFont,
                    text: str,
                    position: vec,
                    color: clr,
                    rotation: 0.0f,
                    origin: Vector2.Zero,
                    scale: zoom,
                    effects: SpriteEffects.None,
                    layerDepth: 0.0f
                    );
                }

            Vector2 Adjust(Vector2 vec, float dx, float dy)
            {
                return new Vector2(vec.X + (dx * zoom), vec.Y + (dy * zoom));
            }

            foreach (var entryKey in Game1.currentLocation.netObjects.Keys)
            {
                var obj = Game1.currentLocation.netObjects[entryKey];
                if (obj.MinutesUntilReady <= 0 || obj.MinutesUntilReady == 999999 || obj.Name == "Stone")
                    continue;

                float x = entryKey.X * Game1.tileSize;
                float y = entryKey.Y * Game1.tileSize;
                Vector2 pos = Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y));
                x = pos.X;
                y = pos.Y;
                string str = (obj.MinutesUntilReady / 10).ToString();
                float w = Game1.dialogueFont.MeasureString(str).X;
                x += (Game1.tileSize - w) / 2;

                Vector2 vec_xy = new Vector2(x, y) * zoom;

                // Outline/shadow to make text contrasting
                DrawString(str, Adjust(vec_xy,  0,  3), half_blk);
                DrawString(str, Adjust(vec_xy,  3,  0), half_blk);
                DrawString(str, Adjust(vec_xy,  0, -3), half_blk);
                DrawString(str, Adjust(vec_xy, -3,  0), half_blk);
                // Actual text itself
                DrawString(str, vec_xy, Color.White);
                }
            }

        private Options GetOptions()
        {
            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                return this.Helper.Reflection.GetField<Options>(typeof(Game1), "options").GetValue();
            }
            else
            {
                return Game1.game1.instanceOptions;  // using .game1.instanceOptions for split-screen-friendly (??)
            }
        }
    }
}

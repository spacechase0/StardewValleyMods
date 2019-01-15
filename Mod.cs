using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ObjectTimeLeft
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static Configuration Config;

        private bool showing = true;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Config = helper.ReadConfig<Configuration>();

            helper.Events.Display.RenderingHud += onRenderingHud;
            helper.Events.Input.ButtonPressed += onButtonPressed;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == Config.ToggleKey)
                showing = !showing;
        }

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onRenderingHud(object sender, RenderingHudEventArgs e)
        {
            if (!showing || !Context.IsPlayerFree)
                return;

            var sb = e.SpriteBatch;
            foreach ( var entryKey in Game1.currentLocation.netObjects.Keys )
            {
                var obj = Game1.currentLocation.netObjects[ entryKey ];
                if (obj.MinutesUntilReady <= 0 || obj.MinutesUntilReady == 999999 || obj.Name == "Stone")
                    continue;

                //float num = (float)(4.0 * Math.Round(Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds / 250.0), 2));
                float x = entryKey.X;
                float y = entryKey.Y;
                Vector2 pos = Game1.GlobalToLocal(Game1.viewport, new Vector2( x * Game1.tileSize, y * Game1.tileSize ));
                x = pos.X;
                y = pos.Y;
                string str = "" + obj.MinutesUntilReady / 10;
                float w = Game1.dialogueFont.MeasureString(str).X;
                x += (Game1.tileSize - w) / 2;

                sb.DrawString(Game1.dialogueFont, str, new Vector2(x + 0, y + 3), (Color.Black) * 0.5f);
                sb.DrawString(Game1.dialogueFont, str, new Vector2(x + 3, y + 0), (Color.Black) * 0.5f);
                sb.DrawString(Game1.dialogueFont, str, new Vector2(x + 0, y - 3), (Color.Black) * 0.5f);
                sb.DrawString(Game1.dialogueFont, str, new Vector2(x - 3, y - 0), (Color.Black) * 0.5f);
                sb.DrawString(Game1.dialogueFont, str, new Vector2(x, y), Color.White);
                //sb.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize - 8), (float)(y * Game1.tileSize - Game1.tileSize * 3 / 2 - 16) + num)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((double)((y + 1) * Game1.tileSize) / 10000.0 + 9.99999997475243E-07 + (double)obj.tileLocation.X / 10000.0 + (obj.parentSheetIndex == 105 ? 0.00150000001303852 : 0.0)));
                //StardewValley.BellsAndWhistles.SpriteText.drawString(sb, "" + obj.minutesUntilReady, (int)pos.X, (int)pos.Y);
            }
        }
    }
}

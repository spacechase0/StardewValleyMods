using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace ObjectTimeLeft
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static Configuration Config;

        private bool showing = true;

        public override void Entry(IModHelper helper)
        {
            base.Entry(helper);
            instance = this;
            Config = helper.ReadConfig<Configuration>();

            GraphicsEvents.OnPreRenderHudEvent += draw;

            ControlEvents.KeyPressed += checkToggle;
        }

        public void checkToggle(object sender, EventArgs args)
        {
            EventArgsKeyPressed pressed = args as EventArgsKeyPressed;
            if (pressed.KeyPressed == Config.ToggleDisplay.key)
            {
                showing = !showing;
            }
        }

        private void draw(object sender, EventArgs args)
        {
            if (!showing)
                return;
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;
            var sb = Game1.spriteBatch;

            foreach ( var entry in Game1.currentLocation.objects )
            {
                var obj = entry.Value;
                if (obj.minutesUntilReady <= 0 || obj.minutesUntilReady == 999999)
                    continue;

                float num = (float)(4.0 * Math.Round(Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds / 250.0), 2));
                float x = entry.Key.X;
                float y = entry.Key.Y;
                Vector2 pos = Game1.GlobalToLocal(Game1.viewport, new Vector2( x * Game1.tileSize, y * Game1.tileSize ));
                x = pos.X;
                y = pos.Y;
                string str = "" + obj.minutesUntilReady / 10;
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

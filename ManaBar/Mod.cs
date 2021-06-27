using System;
using ManaBar.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ManaBar
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        private static Texture2D ManaBg;
        private static Texture2D ManaFg;

        /// <summary>Handles migrating legacy data for a save file.</summary>
        private LegacyDataMigrator LegayDataMigrator;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            this.LegayDataMigrator = new(helper.Data, this.Monitor);

            Command.Register("player_addmana", Mod.AddManaCommand);
            Command.Register("player_setmaxmana", Mod.SetMaxManaCommand);

            helper.Events.GameLoop.DayStarted += Mod.OnDayStarted;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.Display.RenderedHud += Mod.OnRenderedHud;

            Mod.ManaBg = helper.Content.Load<Texture2D>("assets/manabg.png");

            Color manaCol = new Color(0, 48, 255);
            Mod.ManaFg = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            Mod.ManaFg.SetData(new[] { manaCol });
        }

        private static void AddManaCommand(string[] args)
        {
            Game1.player.AddMana(int.Parse(args[0]));
        }
        private static void SetMaxManaCommand(string[] args)
        {
            Game1.player.SetMaxMana(int.Parse(args[0]));
        }

        private IApi Api;
        public override object GetApi()
        {
            return this.Api ??= new Api();
        }

        public static void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Game1.activeClickableMenu != null || Game1.eventUp || Game1.player.GetMaxMana() == 0)
                return;

            SpriteBatch b = e.SpriteBatch;

            Vector2 manaPos = new Vector2(20, Game1.uiViewport.Height - Mod.ManaBg.Height * 4 - 20);
            b.Draw(Mod.ManaBg, manaPos, new Rectangle(0, 0, Mod.ManaBg.Width, Mod.ManaBg.Height), Color.White, 0, new Vector2(), 4, SpriteEffects.None, 1);
            if (Game1.player.GetCurrentMana() > 0)
            {
                Rectangle targetArea = new Rectangle(3, 13, 6, 41);
                float perc = Game1.player.GetCurrentMana() / (float)Game1.player.GetMaxMana();
                int h = (int)(targetArea.Height * perc);
                targetArea.Y += targetArea.Height - h;
                targetArea.Height = h;

                targetArea.X *= 4;
                targetArea.Y *= 4;
                targetArea.Width *= 4;
                targetArea.Height *= 4;
                targetArea.X += (int)manaPos.X;
                targetArea.Y += (int)manaPos.Y;
                b.Draw(Mod.ManaFg, targetArea, new Rectangle(0, 0, 1, 1), Color.White);

                if (Game1.getOldMouseX() >= (double)targetArea.X && Game1.getOldMouseY() >= (double)targetArea.Y && Game1.getOldMouseX() < (double)targetArea.X + targetArea.Width && Game1.getOldMouseY() < targetArea.Y + targetArea.Height)
                    Game1.drawWithBorder(Math.Max(0, Game1.player.GetCurrentMana()).ToString() + "/" + Game1.player.GetMaxMana(), Color.Black * 0.0f, Color.White, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY() - 32));
            }
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            Game1.player.AddMana(Game1.player.GetMaxMana());
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                this.LegayDataMigrator.OnSaveLoaded();
            }
            catch (Exception ex)
            {
                Log.Warn($"Exception migrating legacy save data: {ex}");
            }
        }
    }
}

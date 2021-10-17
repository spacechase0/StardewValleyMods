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
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        private static Texture2D ManaBg;
        private static Texture2D ManaFg;
        private IApi Api;

        /// <summary>Handles migrating legacy data for a save file.</summary>
        private LegacyDataMigrator LegacyDataMigrator;


        /*********
        ** Accessors
        *********/
        public static Mod Instance;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            this.LegacyDataMigrator = new(helper.Data, this.Monitor);

            Command.Register("player_addmana", Mod.HandleAddManaCommand);
            Command.Register("player_setmaxmana", Mod.HandleSetMaxManaCommand);

            helper.Events.GameLoop.DayStarted += Mod.OnDayStarted;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.Display.RenderedHud += Mod.OnRenderedHud;

            Mod.ManaBg = helper.Content.Load<Texture2D>("assets/manabg.png");

            Color manaCol = new Color(0, 48, 255);
            Mod.ManaFg = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            Mod.ManaFg.SetData(new[] { manaCol });
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return this.Api ??= new Api();
        }

        /// <inheritdoc cref="IDisplayEvents.RenderedHud"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        [EventPriority(EventPriority.Low)]
        public static void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            // skip if not applicable
            if (Game1.activeClickableMenu != null || Game1.eventUp)
                return;

            // fetch info
            SpriteBatch spriteBatch = e.SpriteBatch;
            int currentMana = Math.Max(0, Game1.player.GetCurrentMana());
            int maxMana = Game1.player.GetMaxMana();

            // skip if no mana to draw
            if (maxMana <= 0)
                return;

            // draw UI background
            Rectangle manaArea = new(
                x: 5 * Game1.pixelZoom,
                y: Game1.uiViewport.Height - ((Mod.ManaBg.Height + 5) * Game1.pixelZoom),
                width: Mod.ManaBg.Width * Game1.pixelZoom,
                height: Mod.ManaBg.Height * Game1.pixelZoom
            );
            spriteBatch.Draw(Mod.ManaBg, new Vector2(manaArea.X, manaArea.Y), new Rectangle(0, 0, Mod.ManaBg.Width, Mod.ManaBg.Height), Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1);

            // draw current-mana bar
            if (currentMana > 0)
            {
                float filledPercent = currentMana / (float)maxMana;
                const int maxHeight = 41;
                int filledHeight = (int)(maxHeight * filledPercent);
                Rectangle filledArea = new Rectangle(
                    x: manaArea.X + 3 * Game1.pixelZoom,
                    y: (manaArea.Y + (13 + maxHeight - filledHeight) * Game1.pixelZoom),
                    width: 6 * Game1.pixelZoom,
                    height: filledHeight * Game1.pixelZoom
                );

                spriteBatch.Draw(Mod.ManaFg, filledArea, new Rectangle(0, 0, 1, 1), Color.White);
            }

            // draw tooltip
            var mousePos = Game1.getMousePosition();
            if (manaArea.Contains(mousePos))
                Game1.drawWithBorder($"{currentMana}/{maxMana}", Color.Black * 0.0f, Color.White, new Vector2(mousePos.X, mousePos.Y - 32));
        }


        /*********
        ** Private methods
        *********/
        private static void HandleAddManaCommand(string[] args)
        {
            Game1.player.AddMana(int.Parse(args[0]));
        }

        private static void HandleSetMaxManaCommand(string[] args)
        {
            Game1.player.SetMaxMana(int.Parse(args[0]));
        }

        /// <inheritdoc cref="IGameLoopEvents.DayStarted"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            Game1.player.AddMana(Game1.player.GetMaxMana());
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                this.LegacyDataMigrator.OnSaveLoaded();
            }
            catch (Exception ex)
            {
                Log.Warn($"Exception migrating legacy save data: {ex}");
            }
        }
    }
}

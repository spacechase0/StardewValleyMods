using System;
using ManaBar.Framework;
using ManaBar.Interactions.GMCM;
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
        private static Texture2D manaFg;

        private static Texture2D ManaBg;

        private static Texture2D ManaFg
        {
            get
            {
                Color manaCol;

                if (Instance.GetApi() is Api)
                {
                    double offset = GetManaRatio();

                    manaCol = ApplyColorOffset(new Color(0, 48, 255), offset);
                    manaFg.SetData(new[] { manaCol });
                }

                else
                {
                    manaCol = new Color(0, 48, 255);
                    manaFg.SetData(new[] { manaCol });
                }

                return manaFg;
            }

            set
            {
                manaFg = value;
            }
        }

        private IApi Api;

        /// <summary>Handles migrating legacy data for a save file.</summary>
        private LegacyDataMigrator LegacyDataMigrator;


        /*********
        ** Accessors
        *********/
        public static Mod Instance;

        public static ModConfig Config;

        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Log.Monitor = this.Monitor;
            Config = helper.ReadConfig<ModConfig>();
            this.LegacyDataMigrator = new(helper.Data, this.Monitor);

            Command.Register("player_addmana", HandleAddManaCommand);
            Command.Register("player_setmaxmana", HandleSetMaxManaCommand);

            helper.Events.GameLoop.GameLaunched += OnGameLaunch;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.Display.RenderedHud += OnRenderedHud;

            Mod.ManaBg = helper.ModContent.Load<Texture2D>("assets/manabg.png");

            Color manaCol = new(0, 48, 255);
            Mod.ManaFg = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            Mod.ManaFg.SetData(new[] { manaCol });
        }

        private void OnGameLaunch(object sender, GameLaunchedEventArgs e) =>
                     Initializer.InitilizeModMenu(Helper);

        /// <inheritdoc />
        public override object GetApi() =>
                               this.Api ??= new Api();

        /// <inheritdoc cref="IDisplayEvents.RenderedHud"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        [EventPriority(EventPriority.Low)]
        public static void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            // Fetch Info.
            SpriteBatch spriteBatch = e.SpriteBatch;

            // Skip if not applicable.
            if (Game1.activeClickableMenu != null || Game1.eventUp)
                return;

            // Skip if no Mana to draw or Render is disabled.
            if (Game1.player.GetMaxMana() <= 0 || !Config.RenderManaBar)
                return;

            // Else begin to draw ManaBar.
            else
                BeginDrawManaBar(spriteBatch);
        }

        #region Mana Bar Render Functions.

        private static void BeginDrawManaBar(SpriteBatch e)
        {
            #region Used Variables.

            int safeXCoordinate = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Left;
            int safeYCoordinate = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Bottom;

            int barWidth = 12;
            int barHeaderHeight = 16;
            int barBottomPosition = Mod.ManaBg.Height - barHeaderHeight;
            int drawedBarsHeight = default;

            int overchargeHeight = Convert.ToInt32(Math.Ceiling(GetManaOvercharge() * Config.SizeMultiplier));

            Rectangle srcRect;
            Vector2 topOfBar = new(safeXCoordinate + 20 + Config.XManaBarOffset,
                                   safeYCoordinate - CalculateYOffsetToManaBar(barHeaderHeight, overchargeHeight, barBottomPosition) +
                                                     Config.YManaBarOffset);
            #endregion

            // Drawing Bar Layout.
            srcRect = DrawManaBarTop(e, barWidth, barHeaderHeight, ref drawedBarsHeight, topOfBar);
            srcRect = DrawManaBarMiddle(e, barWidth, barHeaderHeight, ref drawedBarsHeight, overchargeHeight, out srcRect, out Rectangle destRect, topOfBar);
            srcRect = DrawManaBarBottom(e, barWidth, barBottomPosition, ref drawedBarsHeight, topOfBar);

            // Filling Layout with Content.
            DrawManaBarFiller(e, barHeaderHeight, barBottomPosition, drawedBarsHeight, out srcRect, out destRect, topOfBar);
            DrawManaBarShade(e, destRect);
            DrawManaBarHoverText(e, drawedBarsHeight, topOfBar);
        }

        private static Rectangle DrawManaBarTop(SpriteBatch e, int barWidth, int barHeaderHeight, ref int drawedBarsHeight, Vector2 topOfBar)
        {
            Rectangle srcRect = new Rectangle(0, 0, barWidth, barHeaderHeight);
            e.Draw(
                Mod.ManaBg,
                topOfBar,
                srcRect,
                Color.White,
                0f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                1f
            );
            drawedBarsHeight += srcRect.Height;
            return srcRect;
        }

        private static Rectangle DrawManaBarMiddle(SpriteBatch e, int barWidth, int barHeaderHeight, ref int drawedBarsHeight, int overchargeHeight, out Rectangle srcRect, out Rectangle destRect, Vector2 topOfBar)
        {
            srcRect = new Rectangle(0, barHeaderHeight, barWidth, 20);
            destRect = new Rectangle(Convert.ToInt32(topOfBar.X),
                                     Convert.ToInt32(topOfBar.Y + drawedBarsHeight * Game1.pixelZoom),
                                     barWidth * 4,
                                     barHeaderHeight + (Mod.ManaBg.Height - barHeaderHeight * 2) +
                                                        Convert.ToInt32(overchargeHeight * Game1.pixelZoom));

            e.Draw(
                Mod.ManaBg,
                destRect,
                srcRect,
                Color.White
            );

            drawedBarsHeight += destRect.Height;
            return srcRect;
        }

        private static Rectangle DrawManaBarBottom(SpriteBatch e, int barWidth, int barBottomPosition, ref int drawedBarsHeight, Vector2 topOfBar)
        {
            Rectangle srcRect = new Rectangle(0, barBottomPosition, barWidth, 16);
            e.Draw(
                Mod.ManaBg,
                new Vector2(topOfBar.X, topOfBar.Y + drawedBarsHeight + barBottomPosition),
                srcRect,
                Color.White,
                0f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                1f
            );
            drawedBarsHeight += srcRect.Height + barBottomPosition;
            return srcRect;
        }

        private static void DrawManaBarFiller(SpriteBatch e, int barHeaderHeight, int barBottomPosition, int drawedBarsHeight, out Rectangle srcRect, out Rectangle destRect, Vector2 topOfBar)
        {
            double currentManaPercent = GetManaRatio();
            int srcHeight = Convert.ToInt32(barBottomPosition);
            int fillerWidth = 6;

            /** Magical Numbers:
             * Yes, here we are using magical numbers. There are two of them.
             * 40 — Additional offset, to prevent render magic filler above bottom of bar;
             * 12 — Additional negative offset, to prevent magic filler overflow bar.
             * 
             * Also, we using check to current mana percent, to prevent magic overflow too.
             **/
            srcRect = new Rectangle(barHeaderHeight, barBottomPosition, fillerWidth, srcHeight);
            destRect = new Rectangle(Convert.ToInt32(topOfBar.X + (ManaBg.Width * (int)Math.PI)),
                                     Convert.ToInt32(topOfBar.Y + drawedBarsHeight + 40),
                                     fillerWidth * Game1.pixelZoom,
                                     Convert.ToInt32((drawedBarsHeight - 12) *
                                                     (currentManaPercent > 1.0 ? 1.0 : currentManaPercent)));

            e.Draw(
                Mod.ManaFg,
                destRect,
                srcRect,
                Color.White,
                (float)Math.PI,
                Vector2.Zero,
                SpriteEffects.None,
                1f
            );
        }

        private static void DrawManaBarShade(SpriteBatch e, Rectangle destRect)
        {
            destRect.Height = 4;
            e.Draw(
                Game1.staminaRect,
                destRect,
                Game1.staminaRect.Bounds,
                Color.Black * 0.3f,
                (float)Math.PI,
                Vector2.Zero,
                SpriteEffects.None,
                1f
            );
        }

        private static void DrawManaBarHoverText(SpriteBatch e, int drawedBarsHeight, Vector2 topOfBar)
        {
            if (CheckXAxisToMouseIntersection(topOfBar.X, out int mouseX) &&
                CheckYAxisToMouseIntersection(drawedBarsHeight, topOfBar.Y, out int mouseY))
            {
                e.DrawString(
                    Game1.dialogueFont,
                    $"{Game1.player.GetCurrentMana()}/{Game1.player.GetMaxMana()}",
                    new Vector2(mouseX, mouseY - 32),
                    new Color(0, 48, 255)

                );
            }
        }
        #endregion

        private static int CalculateYOffsetToManaBar(int barHeaderHeight, double oversize, int barBottomPosition)
        {
            /** Variable: 'bottomMargin'.
             *              
             * After base calculations, we get value, that lies right on game screen border.
             * But we need to make small margin. So we need this variable to this needs.
             * Value set to 24, cause with this value mana bar will have same margin as other bars.
             **/
            int bottomMargin = 24;
            int height = Mod.ManaBg.Height;

            height += barHeaderHeight * 2;
            height += (Mod.ManaBg.Height - barHeaderHeight * 2) + Convert.ToInt32(oversize * Game1.pixelZoom);
            height += barBottomPosition;
            height += bottomMargin;

            return height;
        }

        private static bool CheckXAxisToMouseIntersection(float xTopPosition, out int xPosition)
        {
            xPosition = Game1.getOldMouseX();

            return xPosition >= xTopPosition && xPosition < xTopPosition + 36f;
        }

        private static bool CheckYAxisToMouseIntersection(int drawedBarsHeight, float yTopPosition, out int yPosition)
        {
            yPosition = Game1.getOldMouseY();

            return yPosition >= yTopPosition && yPosition < yTopPosition + drawedBarsHeight + 46f;
        }

        /*********
        ** Private methods
        *********/
        private static Color ApplyColorOffset(Color color, double offset)
        {
            byte redMaxOffset = 255;
            byte greenMaxOffset = 207;

            byte currentRedOffset = (byte)(Math.Abs(offset - 1) * redMaxOffset);
            byte currentGreenOffset = (byte)(Math.Abs(offset - 1) * greenMaxOffset);

            return new Color(color.R + currentRedOffset, color.G + currentGreenOffset, color.B);
        }

        private static double GetManaRatio()
        {
            double currentMana = Game1.player.GetCurrentMana() * 1.0;
            double maxMana = Game1.player.GetMaxMana() * 1.0;

            return currentMana / maxMana;
        }

        private static double GetManaOvercharge()
        {
            double maxMana = Game1.player.GetMaxMana();

            return maxMana / ManaBar.Api.BaseMaxMana;
        }

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

using JumpOver.Framework;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace JumpOver
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static Configuration Config;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(this.ModManifest, () => Mod.Config = new Configuration(), () => this.Helper.WriteConfig(Mod.Config));
                capi.RegisterSimpleOption(this.ModManifest, "Jump Key", "The key to jump", () => Mod.Config.KeyJump, (SButton val) => Mod.Config.KeyJump = val);
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || Game1.activeClickableMenu != null)
                return;

            if (e.Button == Mod.Config.KeyJump && Game1.player.yJumpVelocity == 0)
            {
                // This is terrible for this case, redo it
                new Jump(Game1.player, this.Helper.Events);
            }
        }

        internal class Jump
        {
            private readonly Farmer Player;
            private readonly IModEvents Events;
            private float PrevJumpVel;

            //private bool wasGoingOver = false;

            public Jump(Farmer thePlayer, IModEvents events)
            {
                this.Player = thePlayer;
                this.Events = events;
                this.PrevJumpVel = this.Player.yJumpVelocity;

                this.Player.synchronizedJump(8);

                events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            }

            /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
            /// <param name="sender">The event sender.</param>
            /// <param name="e">The event arguments.</param>
            private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
            {
                if (this.Player.yJumpVelocity == 0 && this.PrevJumpVel < 0)
                {
                    this.Player.canMove = true;

                    this.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
                }
                else
                {
                    int tx = (int)this.Player.position.X / Game1.tileSize;
                    int ty = (int)this.Player.position.Y / Game1.tileSize;
                    int ox = 0, oy = 0; // Offset x, y
                    switch (this.Player.facingDirection.Value)
                    {
                        case Game1.up: oy = -1; break;
                        case Game1.down: oy = 1; break;
                        case Game1.left: ox = -1; break;
                        case Game1.right: ox = 1; break;
                    }

                    var bb = this.Player.GetBoundingBox();
                    var bb1 = this.Player.GetBoundingBox();
                    bb1.X += ox * Game1.tileSize;
                    bb1.Y += oy * Game1.tileSize;
                    var bb2 = this.Player.GetBoundingBox();
                    bb2.X += ox * Game1.tileSize * 2;
                    bb2.Y += oy * Game1.tileSize * 2;

                    bool n0 = this.Player.currentLocation.isCollidingPosition(bb, Game1.viewport, true, 0, false, this.Player);
                    bool n1 = this.Player.currentLocation.isCollidingPosition(bb1, Game1.viewport, true, 0, false, this.Player);
                    bool n2 = this.Player.currentLocation.isCollidingPosition(bb2, Game1.viewport, true, 0, false, this.Player);

                    //Log.trace($"{n0} {n1} {n2}");
                    if (n0 || (!n0 && n1 && !n2) /*|| wasGoingOver*/ )
                    {
                        //wasGoingOver = true;
                        Game1.player.canMove = false;
                        this.Player.position.X += ox * 5;
                        this.Player.position.Y += oy * 5;
                    }
                }

                this.PrevJumpVel = this.Player.yJumpVelocity;
            }
        }
    }
}
